using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Bitclout.Model
{
    /// <summary>
    /// Класс описывающий созданный аккаунт
    /// </summary>
    [Serializable]
    public class UserInfo : INotifyPropertyChanged
    {
        string _Name;
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
        }

        string _Description;
        /// <summary>
        /// Описание
        /// </summary>
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
                OnPropertyChanged("Description");
            }
        }

        string _BitcloutSeedPhrase;
        /// <summary>
        /// Фраза bitclout
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

        double _USDBuy;
        /// <summary>
        /// Долларов портачено
        /// </summary>
        public double USDBuy
        {
            get
            {
                return _USDBuy;
            }
            set
            {
                _USDBuy = value;
                OnPropertyChanged("USDBuy");
            }
        }

        double _USDSell;
        /// <summary>
        /// Долларов получено
        /// </summary>
        public double USDSell
        {
            get
            {
                return _USDSell;
            }
            set
            {
                _USDSell = value;
                OnPropertyChanged("USDSell");
            }
        }

        string _PublicKey;
        /// <summary>
        /// Публичный ключ
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

        string _UserPhotoPath;
        /// <summary>
        /// Путь к фото профиля
        /// </summary>
        public string UserPhotoPath
        {
            get
            {
                return _UserPhotoPath;
            }
            set
            {
                _UserPhotoPath = value;
                OnPropertyChanged("UserPhotoPath");
            }
        }

        string _BitcloutSreenPath;
        /// <summary>
        /// Путь к Скриншоту профиля bitclout
        /// </summary>
        public string BitcloutSreenPath
        {
            get
            {
                return _BitcloutSreenPath;
            }
            set
            {
                _BitcloutSreenPath = value;
                OnPropertyChanged("BitcloutSreenPath");
            }
        }

        bool _IsSell = false;

        public bool IsSell
        {
            get
            {
                return _IsSell;
            }
            set
            {
                _IsSell = value;
                OnPropertyChanged("IsSell");
            }
        }

        bool _IsUpdate = false;

        public bool IsUpdate
        {
            get
            {
                return _IsUpdate;
            }
            set
            {
                _IsUpdate = value;
                OnPropertyChanged("IsUpdate");
            }
        }

        bool _IsError = false;

        public bool IsError
        {
            get
            {
                return _IsError;
            }
            set
            {
                _IsError = value;
                OnPropertyChanged("IsError");
            }
        }

        public UserInfo()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public static bool SaveRegistredUsers(List<UserInfo> users)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("bin\\RegistredUsers.dat"))
                {
                    sw.Write(SerializeHelper.Serialize(users));
                    return true;
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Не удалось сохранить зарегистрированных пользователей.");
                return false;
            }
        }

        public static List<UserInfo> LoadRegistredUsers()
        {
            try
            {
                using (StreamReader sr = new StreamReader("bin\\RegistredUsers.dat"))
                {
                    return SerializeHelper.Desirialize<List<UserInfo>>(sr.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Не удалось загрузить зарегистрированных пользователей.");
                return new List<UserInfo>();
            }
        }

    }
}