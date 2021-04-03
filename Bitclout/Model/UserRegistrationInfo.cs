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

        public UserRegistrationInfo()
        {

        }

        public UserRegistrationInfo(string name, string description, string photoPath)
        {
            Name = name;
            Description = description;
            PhotoPath = photoPath;
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
                //сделать логирование
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
                //Сделать логирование
                return false;
            }
        }
    }
}
