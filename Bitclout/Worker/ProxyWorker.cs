using Bitclout.Exceptions;
using Bitclout.Model;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Helpers;

namespace Bitclout.Worker
{
    public class ProxyWorker
    {
        public static List<string> ProxyCodes { get; set; } = new List<string>("us de ru end".Split(' '));

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
    }
}
