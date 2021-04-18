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

        string _BitcloudPhrase;
        /// <summary>
        /// Описание пользователя
        /// </summary>
        public string BitcloudPhrase
        {
            get
            {
                return _BitcloudPhrase;
            }
            set
            {
                _BitcloudPhrase = value;
                OnPropertyChanged("BitcloudPhrase");
            }
        }

        bool _IsRegistred = false;

        public bool IsRegistred
        {
            get
            {
                return _IsRegistred;
            }
            set
            {
                _IsRegistred = value;
                OnPropertyChanged("IsRegistred");
            }
        }

        public UserRegistrationInfo()
        {

        }

        public UserRegistrationInfo(string name, string bitcloudPhrase)
        {
            Name = name;
            BitcloudPhrase = bitcloudPhrase;
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
