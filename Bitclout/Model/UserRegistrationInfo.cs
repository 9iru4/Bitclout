using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Bitclout.Model
{
    /// <summary>
    /// Данные пользователя для регисрации
    /// </summary>
    [Serializable]
    public class UserRegistrationInfo : INotifyPropertyChanged
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
        string _TwitterName;

        /// <summary>
        /// Имя пользователя Твиттера
        /// </summary>
        public string TwitterName
        {
            get
            {
                return _TwitterName;
            }
            set
            {
                _TwitterName = value;
                OnPropertyChanged("TwitterName");
            }
        }

        string _Description;
        /// <summary>
        /// Описание пользователя
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

        string _PhotoPath;
        /// <summary>
        /// Путь к фото пользователя
        /// </summary>
        public string PhotoPath
        {
            get
            {
                return _PhotoPath;
            }
            set
            {
                _PhotoPath = value;
                OnPropertyChanged("PhotoPath");
            }
        }

        string _TweetMessage;
        /// <summary>
        /// Сообщение для твита
        /// </summary>
        public string TweetMessage
        {
            get
            {
                return _TweetMessage;
            }
            set
            {
                _TweetMessage = value;
                OnPropertyChanged("TweetMessage");
            }
        }

        public UserRegistrationInfo()
        {

        }

        public UserRegistrationInfo(string name, string description, string photoPath, string twitterName, string tweetMessage)
        {
            Name = name;
            Description = description;
            PhotoPath = photoPath;
            TwitterName = twitterName;
            TweetMessage = tweetMessage;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Загрука пользователей из файла
        /// </summary>
        /// <returns>Загруженные пользователи</returns>
        public static List<UserRegistrationInfo> LoadUsers()
        {
            try
            {
                using (StreamReader sr = new StreamReader("bin\\Users.dat"))
                {
                    return SerializeHelper.Desirialize<List<UserRegistrationInfo>>(sr.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, "Не удалось загрузить пользователей.");
                return new List<UserRegistrationInfo>();
            }
        }

        /// <summary>
        /// Сохранние пользователей в файл
        /// </summary>
        /// <param name="users">Пользователи для сохранения</param>
        /// <returns>Успешно ли сохранение</returns>
        public static bool SaveUsers(List<UserRegistrationInfo> users)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("bin\\Users.dat"))
                {
                    sw.Write(SerializeHelper.Serialize(users));
                    return true;
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, "Не удалось сохранить пользователей.");
                return false;
            }
        }
    }
}
