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

        string _TwitterApiKey;
        /// <summary>
        /// Ключ апи для твиттера
        /// </summary>
        public string TwitterApiKey
        {
            get
            {
                return _TwitterApiKey;
            }
            set
            {
                _TwitterApiKey = value;
                OnPropertyChanged("TwitterApiKey");
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

        string _TwitterUserName;
        /// <summary>
        /// Имя пользователя Твиттера
        /// </summary>
        public string TwitterUserName
        {
            get
            {
                return _TwitterUserName;
            }
            set
            {
                _TwitterUserName = value;
                OnPropertyChanged("TwitterUserName");
            }
        }

        string _TwitterPassword;
        /// <summary>
        /// Пароль пользователя Твиттера
        /// </summary>
        public string TwitterPassword
        {
            get
            {
                return _TwitterPassword;
            }
            set
            {
                _TwitterPassword = value;
                OnPropertyChanged("TwitterPassword");
            }
        }

        string _TwitterEmail;
        /// <summary>
        /// Twitter Email
        /// </summary>
        public string TwitterEmail
        {
            get
            {
                return _TwitterEmail;
            }
            set
            {
                _TwitterEmail = value;
                OnPropertyChanged("TwitterEmail");
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

        public Settings()
        {
            SMSApiKey = "";
            TwitterApiKey = "";
            ProxyApiKey = "";
            ChromePath = "";
            PhotosPath = "";
            TwitterUserName = "";
            TwitterPassword = "";
            TwitterEmail = "";
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
