using Bitclout.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Web.Helpers;

namespace Bitclout.Worker
{
    public class ProxyWorker
    {
        /// <summary>
        /// Получение IPv6 прокси
        /// </summary>
        /// <returns>Прокси</returns>
        public static Proxy GetProxy()
        {

            WebRequest request = WebRequest.Create("https://proxy6.net/api/" + MainWindowViewModel.settings.ProxyApiKey + "/buy?count=1&period=3&country=ru");
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    dynamic data = Json.Decode(reader.ReadToEnd());
                    if (data.status == "yes")
                    {
                        foreach (var item in data.list)
                        {
                            var proxy = item.Value;
                            return new Proxy(proxy.id, proxy.ip, proxy.host, proxy.port, proxy.user, proxy.pass);
                        }
                        throw new Exception("Не удалось получить прокси");
                    }
                    else
                    {
                        throw new Exception("Не удалось получить прокси");
                    }
                }
            }
        }

        /// <summary>
        /// Удаление прокси
        /// </summary>
        /// <returns>Прокси</returns>
        public static bool DeleteProxy(Proxy proxy)
        {

            WebRequest request = WebRequest.Create("https://proxy6.net/api/" + MainWindowViewModel.settings.ProxyApiKey + "/delete?ids=" + proxy.ID);
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    dynamic data = Json.Decode(reader.ReadToEnd());
                    if (data.status == "yes")
                    {
                        return true;
                    }
                    else
                    {
                        //Сделать логирование
                        throw new Exception("Не удалось получить прокси");
                    }
                }
            }
        }

        /// <summary>
        /// Обновление данных прокси в файле расширения
        /// </summary>
        /// <returns>Обновлены ли данные</returns>
        public static bool UpdateProxyExtension()
        {
            try
            {
                List<string> background = new List<string>();
                using (StreamReader sr = new StreamReader(@"proxy\background.js"))
                {
                    while (!sr.EndOfStream)
                    {
                        background.Add(sr.ReadLine());
                    }
                }
                background[4] = $"      host: \"{MainWindowViewModel.settings.CurrentProxy.Host}\",";
                background[5] = $"      port: parseInt({MainWindowViewModel.settings.CurrentProxy.Port})";
                background[18] = $"      username: \"{MainWindowViewModel.settings.CurrentProxy.UserName}\",";
                background[19] = $"      password: \"{MainWindowViewModel.settings.CurrentProxy.Pass}\"";
                using (StreamWriter sw = new StreamWriter(@"proxy\background.js"))
                {
                    foreach (var str in background)
                    {
                        sw.WriteLine(str);
                    }
                }
                string startPath = @"proxy";
                string zipPath = @"proxy.zip";

                ZipFile.CreateFromDirectory(startPath, zipPath);
                return true;
            }
            catch (Exception ex)
            {
                //Реализовать логирование
                return false;
            }
            
        }
    }
}
