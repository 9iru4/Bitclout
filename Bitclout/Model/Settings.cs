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

        string _PublicKey;
        /// <summary>
        /// Логин Bitclout
        /// </summary>
        public string PublicKey
        {
            get
            {
                return _PublicKey;
            }
            set
            {
                _PublicKey = value;
                OnPropertyChanged("PublicKey");
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

        string _VerifySyncAddress = "http://46.243.186.18:10090/api/Default/GetAll?ID=";
        /// <summary>
        /// Логин Bitclout
        /// </summary>
        public string VerifySyncAddress
        {
            get
            {
                return _VerifySyncAddress;
            }
            set
            {
                _VerifySyncAddress = value;
                OnPropertyChanged("VerifySyncAddress");
            }
        }

        string _UpdateSyncAddress = "http://46.243.186.18:10099/api/Default/GetAll?ID=";
        /// <summary>
        /// Логин Bitclout
        /// </summary>
        public string UpdateSyncAddress
        {
            get
            {
                return _UpdateSyncAddress;
            }
            set
            {
                _UpdateSyncAddress = value;
                OnPropertyChanged("UpdateSyncAddress");
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

        int _BuyDelay = 10000;

        public int BuyDelay
        {
            get
            {
                return _BuyDelay;
            }
            set
            {
                _BuyDelay = value;
                OnPropertyChanged("BuyDelay");
            }
        }

        int _MainDelay = 5000;

        public int MainDelay
        {
            get
            {
                return _MainDelay;
            }
            set
            {
                _MainDelay = value;
                OnPropertyChanged("MainDelay");
            }
        }

        int _MerlinDelay = 1000;

        public int MerlinDelay
        {
            get
            {
                return _MerlinDelay;
            }
            set
            {
                _MerlinDelay = value;
                OnPropertyChanged("MerlinDelay");
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

        SMSCountry _SMSCountry = new SMSCountry(0, "iti-0__item-ru", "Ru");

        public SMSCountry SMSCountry
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

        ProxyType _ProxyType = new ProxyType(PrxType.NotUsed, "NotUsed");

        public ProxyType ProxyType
        {
            get
            {
                return _ProxyType;
            }
            set
            {
                _ProxyType = value;
                OnPropertyChanged("ProxyType");
            }
        }

        BotWorkMode _BotWorkMode = new BotWorkMode(WorkType.OnlyReg, "OnlyReg");

        public BotWorkMode BotWorkMode
        {
            get
            {
                return _BotWorkMode;
            }
            set
            {
                _BotWorkMode = value;
                OnPropertyChanged("BotWorkMode");
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
