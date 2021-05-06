using Bitclout.Exceptions;
using Bitclout.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Helpers;

namespace Bitclout.Worker
{
    public class ProxyWorker
    {
        public static List<string> ProxyCodes { get; set; } = new List<string>("ru end".Split(' '));

        public static bool ChangeProxyCountry()
        {
            if (ProxyCodes[1] != "end")
            {
                var s = ProxyCodes[0];
                ProxyCodes.RemoveAt(0);
                ProxyCodes.Add(s);
                return true;
            }
            return false;
        }

        public static Proxy GetProxyFromCollection(List<Proxy> proxy)
        {
            var col = proxy.Where(x => x.AccountsRegistred == 0);
            if (col.Count() != 0)
                return col.FirstOrDefault();
            else
            {
                var col1 = proxy.Where(x => x.AccountsRegistred == 1);
                if (col1.Count() != 0)
                    return col1.FirstOrDefault();
                else
                {
                    foreach (var item in MainWindowViewModel._Proxy)
                    {
                        item.AccountsRegistred = 0;
                    }
                    return MainWindowViewModel._Proxy.Where(x => x.AccountsRegistred == 0).FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Получение IPv6 прокси
        /// </summary>
        /// <returns>Прокси</returns>
        public static Proxy GetProxy()
        {
            Proxy prx = new Proxy();
            NLog.LogManager.GetCurrentClassLogger().Info($"Делаем запрос на получение прокси ->");
            WebRequest request = WebRequest.Create("https://proxy6.net/api/" + MainWindowViewModel.settings.ProxyApiKey + "/buy?count=1&period=3&country=" + ProxyCodes[0]);
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    dynamic data = Json.Decode(reader.ReadToEnd());
                    if (data.status == "yes")
                    {
                        dynamic proxy = data;
                        foreach (var item in data.list)
                        {
                            proxy = item.Value;
                            prx = new Proxy(proxy.id, proxy.ip, proxy.host, proxy.port, proxy.user, proxy.pass);
                            prx.StatusCode = data.status;
                        }
                        NLog.LogManager.GetCurrentClassLogger().Info($"Прокси {prx.IP} успешно получен с кодом {prx.StatusCode}");
                        return prx;
                    }
                    else
                    {
                        prx.StatusCode = data.error;
                        NLog.LogManager.GetCurrentClassLogger().Info($"Ошибка получения прокси {data.error}");
                        if (prx.StatusCode.Contains("Error active proxy allow"))
                            throw new OutOfProxyException("Закончилсь доступные прокси для выбранной страны");
                        return null;
                    }
                }
            }
        }

        public static Proxy GetProxy(string code)
        {
            Proxy prx = new Proxy();
            NLog.LogManager.GetCurrentClassLogger().Info($"Делаем запрос на получение прокси ->");
            WebRequest request = WebRequest.Create("https://proxy6.net/api/" + MainWindowViewModel.settings.ProxyApiKey + "/buy?count=1&period=3&country=" + code);
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    dynamic data = Json.Decode(reader.ReadToEnd());
                    if (data.status == "yes")
                    {
                        dynamic proxy = data;
                        foreach (var item in data.list)
                        {
                            proxy = item.Value;
                            prx = new Proxy(proxy.id, proxy.ip, proxy.host, proxy.port, proxy.user, proxy.pass);
                            prx.StatusCode = data.status;
                        }
                        NLog.LogManager.GetCurrentClassLogger().Info($"Прокси {prx.IP} успешно получен с кодом {prx.StatusCode}");
                        return prx;
                    }
                    else
                    {
                        prx.StatusCode = data.error;
                        NLog.LogManager.GetCurrentClassLogger().Info($"Ошибка получения прокси {data.error}");
                        if (prx.StatusCode.Contains("Error active proxy allow"))
                            throw new OutOfProxyException("Закончилсь доступные прокси для выбранной страны");
                        return null;
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
            NLog.LogManager.GetCurrentClassLogger().Info($"Делаем запрос на удаление использованного прокси {proxy.IP} ->");
            WebRequest request = WebRequest.Create("https://proxy6.net/api/" + MainWindowViewModel.settings.ProxyApiKey + "/delete?ids=" + proxy.ID);
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    dynamic data = Json.Decode(reader.ReadToEnd());
                    if (data.status == "yes")
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info($"Прокси успешно удален");
                        return true;
                    }
                    else
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info($"Не удалось удалить прокси {data.error}");
                        return false;
                    }
                }
            }
        }
        public static List<Proxy> LoadProxy()
        {
            try
            {
                using (StreamReader sr = new StreamReader("bin\\Proxy.dat"))
                {
                    return SerializeHelper.Desirialize<List<Proxy>>(sr.ReadToEnd());
                }
            }
            catch
            {
                return new List<Proxy>();
            }
        }

        /// <summary>
        /// Сохранение настроек в файл
        /// </summary>
        /// <returns></returns>
        public static bool SaveProxy(List<Proxy> proxy)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("bin\\Proxy.dat"))
                {
                    sw.Write(SerializeHelper.Serialize(proxy));
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

    }
}
