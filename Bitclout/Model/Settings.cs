using Bitclout.Worker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        string _BotID = "MaksLoh";
        /// <summary>
        /// Логин Bitclout
        /// </summary>
        public string BotID
        {
            get
            {
                return _BotID;
            }
            set
            {
                _BotID = value;
                OnPropertyChanged("BotID");
            }
        }

        string _SyncAddress = "http://46.243.186.18:10090/api/Default/GetAll?ID=";
        /// <summary>
        /// Логин Bitclout
        /// </summary>
        public string SyncAddress
        {
            get
            {
                return _SyncAddress;
            }
            set
            {
                _SyncAddress = value;
                OnPropertyChanged("SyncAddress");
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

        int _SellSleep = 10000;

        public int SellSleep
        {
            get
            {
                return _SellSleep;
            }
            set
            {
                _SellSleep = value;
                OnPropertyChanged("SellSleep");
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

        bool _SOAX = false;

        public bool SOAX
        {
            get
            {
                return _SOAX;
            }
            set
            {
                _SOAX = value;
                OnPropertyChanged("SOAX");
            }
        }

        bool _SendBitlout = false;

        public bool SendBitlout
        {
            get
            {
                return _SendBitlout;
            }
            set
            {
                _SendBitlout = value;
                OnPropertyChanged("SendBitlout");
            }
        }

        bool _IsSyncBots = false;

        public bool IsSyncBots
        {
            get
            {
                return _IsSyncBots;
            }
            set
            {
                _IsSyncBots = value;
                OnPropertyChanged("IsSyncBots");
            }
        }

        bool _IsDeletePhoto = false;

        public bool IsDeletePhoto
        {
            get
            {
                return _IsDeletePhoto;
            }
            set
            {
                _IsDeletePhoto = value;
                OnPropertyChanged("IsDeletePhoto");
            }
        }

        bool _IsMerlin = false;

        public bool IsMerlin
        {
            get
            {
                return _IsMerlin;
            }
            set
            {
                _IsMerlin = value;
                OnPropertyChanged("IsMerlin");
            }
        }

        bool _IsUsingProxy = false;

        public bool IsUsingProxy
        {
            get
            {
                return _IsUsingProxy;
            }
            set
            {
                _IsUsingProxy = value;
                OnPropertyChanged("IsUsingProxy");
            }
        }

        int _SMSCountry = 0;

        public int SMSCountry
        {
            get
            {
                return _SMSCountry;
            }
            set
            {
                _SMSCountry = value;
                OnPropertyChanged("SMSCountry");
            }
        }

        string _CountryCode = "iti-0__item-gb";

        public string CountryCode
        {
            get
            {
                return _CountryCode;
            }
            set
            {
                _CountryCode = value;
                OnPropertyChanged("CountryCode");
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

        int _MerlinTime = 1000;

        public int MerlinTime
        {
            get
            {
                return _MerlinTime;
            }
            set
            {
                _MerlinTime = value;
                OnPropertyChanged("MerlinTime");
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
