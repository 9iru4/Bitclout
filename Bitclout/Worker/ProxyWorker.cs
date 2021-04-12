using Bitclout.Model;
using System;
using System.IO;
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
            Proxy prx = new Proxy();
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Делаем запрос на получение прокси ->");
                WebRequest request = WebRequest.Create("https://proxy6.net/api/" + MainWindowViewModel.settings.ProxyApiKey + "/buy?count=1&period=3&country=" + MainWindowViewModel.settings.ProxyCode);
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
                                NLog.LogManager.GetCurrentClassLogger().Info($"Прокси {proxy.ip} успешно получен с кодом {data.status}");
                                return prx;
                            }
                            throw new Exception($"Ошибка обработки данных прокси {proxy}");
                        }
                        else
                        {
                            prx.StatusCode = data.error;
                            NLog.LogManager.GetCurrentClassLogger().Info($"Ошибка получения прокси {data.error}");
                            return prx;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка в методе получения прокси {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Удаление прокси
        /// </summary>
        /// <returns>Прокси</returns>
        public static bool DeleteProxy(Proxy proxy)
        {
            try
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
                            throw new Exception("Не удалось удалить прокси {data.error}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Не удалось удалить прокси {ex.Message}");
                return false;
            }
        }
    }
}
