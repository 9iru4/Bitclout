using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Bitclout.Model
{
    /// <summary>
    /// Настройки программы
    /// </summary>
    [Serializable]
    public class Settings : INotifyPropertyChanged
    {
        string _SMSApiKey;
        /// <summary>
        /// Ключ апи для смс
        /// </summary>
        public string SMSApiKey
        {
            get
            {
                return _SMSApiKey;
            }
            set
            {
                _SMSApiKey = value;
                OnPropertyChanged("SMSApiKey");
            }
        }

        string _ProxyApiKey;
        /// <summary>
        /// Ключ апи для прокси
        /// </summary>
        public string ProxyApiKey
        {
            get
            {
                return _ProxyApiKey;
            }
            set
            {
                _ProxyApiKey = value;
                OnPropertyChanged("ProxyApiKey");
            }
        }

        string _ChromePath;
        /// <summary>
        /// Путь к хрому
        /// </summary>
        public string ChromePath
        {
            get
            {
                return _ChromePath;
            }
            set
            {
                _ChromePath = value;
                OnPropertyChanged("ChromePath");
            }
        }

        string _PhotosPath;
        /// <summary>
        /// Путь к папке фото
        /// </summary>
        public string PhotosPath
        {
            get
            {
                return _PhotosPath;
            }
            set
            {
                _PhotosPath = value;
                OnPropertyChanged("PhotosPath");
            }
        }

        string _BitcloutSeedPhrase;
        /// <summary>
        /// Логин Bitclout
        /// </summary>
        public string BitcloutSeedPhrase
        {
            get
            {
                return _BitcloutSeedPhrase;
            }
            set
            {
                _BitcloutSeedPhrase = value;
                OnPropertyChanged("BitcloutSeedPhrase");
            }
        }

        Proxy _CurrentProxy;
        /// <summary>
        /// Текущий прокси
        /// </summary>
        public Proxy CurrentProxy
        {
            get
            {
                return _CurrentProxy;
            }
            set
            {
                _CurrentProxy = value;
                OnPropertyChanged("CurrentProxy");
            }
        }

        Proxy _MainProxy;
        /// <summary>
        /// Текущий прокси
        /// </summary>
        public Proxy MainProxy
        {
            get
            {
                return _MainProxy;
            }
            set
            {
                _MainProxy = value;
                OnPropertyChanged("MainProxy");
            }
        }

        int _DelayTime = 5000;

        public int DelayTime
        {
            get
            {
                return _DelayTime;
            }
            set
            {
                _DelayTime = value;
                OnPropertyChanged("DelayTime");
            }
        }

        double _MinUSD = 3;

        public double MinUSD
        {
            get
            {
                return _MinUSD;
            }
            set
            {
                _MinUSD = value;
                OnPropertyChanged("MinUSD");
            }
        }

        double _MaxUSD = 7;

        public double MaxUSD
        {
            get
            {
                return _MaxUSD;
            }
            set
            {
                _MaxUSD = value;
                OnPropertyChanged("MaxUSD");
            }
        }

        double _SellAmount = 2.85;

        public double SellAmount
        {
            get
            {
                return _SellAmount;
            }
            set
            {
                _SellAmount = value;
                OnPropertyChanged("SellAmount");
            }

        }

        double _SellMoreThan = 10;

        public double SellMoreThan
        {
            get
            {
                return _SellMoreThan;
            }
            set
            {
                _SellMoreThan = value;
                OnPropertyChanged("SellMoreThan");
            }
        }

        bool _IsBuyCoins = false;

        public bool IsBuyCoins
        {
            get
            {
                return _IsBuyCoins;
            }
            set
            {
                _IsBuyCoins = value;
                OnPropertyChanged("IsBuyCoins");
            }
        }

        int _Comission = 0;

        public int Comission
        {
            get
            {
                return _Comission;
            }
            set
            {
                _Comission = value;
                OnPropertyChanged("Comission");
            }
        }

        public Settings()
        {
            SMSApiKey = "";
            ProxyApiKey = "";
            ChromePath = "";
            PhotosPath = "";
            BitcloutSeedPhrase = "";
            CurrentProxy = null;
        }

        /// <summary>
        /// Загрузка настроек из файла
        /// </summary>
        /// <returns>Загруженные настройки</returns>
        public static Settings LoadSettings()
        {
            try
            {
                using (StreamReader sr = new StreamReader("bin\\Settings.dat"))
                {
                    return SerializeHelper.Desirialize<Settings>(sr.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Не удалось загрузить настройки.");
                return new Settings();
            }
        }

        /// <summary>
        /// Сохранение настроек в файл
        /// </summary>
        /// <returns></returns>
        public bool SaveSettings()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("bin\\Settings.dat"))
                {
                    sw.Write(SerializeHelper.Serialize(this));
                    return true;
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Не удалось сохранить настройки.");
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
