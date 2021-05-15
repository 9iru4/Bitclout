using Bitclout.Exceptions;
using Bitclout.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web.Helpers;

namespace Bitclout.Worker
{


    public class ProxyWorker : INotifyPropertyChanged
    {
        public static List<string> ProxyCodes { get; set; } = new List<string>("ru end".Split(' '));

        public ObservableCollection<Proxy> Proxy { get; set; } = new ObservableCollection<Proxy>();

        public ProxyWorker()
        {
            LoadProxy();
        }

        public bool ChangeProxyCountry()
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

        public Proxy GetProxyFromCollection()
        {
            var prx0 = Proxy.Where(x => x.CurrentStatus == ProxyStatus.NotUsed).FirstOrDefault();

            if (prx0 != null)
                return prx0;
            else
            {
                foreach (var item in Proxy.Where(x => x.CurrentStatus != ProxyStatus.Died))
                {
                    item.CurrentStatus = ProxyStatus.Good;
                }
                return Proxy.Where(x => x.CurrentStatus == ProxyStatus.Good).FirstOrDefault();
            }
        }

        public void AddProxyToCollection(Proxy proxy)
        {
            if (Proxy.Where(x => x.ID == proxy.ID).FirstOrDefault() == null)
            {
                Proxy.Add(proxy);
                SaveProxy();
            }
        }

        public void ChangeProxyStatus(string ID, ProxyStatus status)
        {
            Proxy.Where(x => x.ID == ID).FirstOrDefault().CurrentStatus = status;
            SaveProxy();
        }

        /// <summary>
        /// Получение IPv6 прокси
        /// </summary>
        /// <returns>Прокси</returns>
        public Proxy GetProxy()
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
                            prx = new Proxy(proxy.id, proxy.ip, proxy.host, proxy.pass);
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

        public Proxy GetProxy(string code)
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
                            prx = new Proxy(proxy.id, proxy.ip, proxy.host, proxy.pass);
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
        public bool DeleteProxy(Proxy proxy)
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
        public void LoadProxy()
        {
            try
            {
                using (StreamReader sr = new StreamReader("bin\\Proxy.dat"))
                {
                    foreach (var item in SerializeHelper.Desirialize<List<Proxy>>(sr.ReadToEnd()))
                    {
                        Proxy.Add(item);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Сохранение настроек в файл
        /// </summary>
        /// <returns></returns>
        public bool SaveProxy()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("bin\\Proxy.dat"))
                {
                    sw.Write(SerializeHelper.Serialize(Proxy.ToList()));
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
