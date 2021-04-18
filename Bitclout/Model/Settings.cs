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
        string _BitcloutPublicKey;
        /// <summary>
        /// Логин Bitclout
        /// </summary>
        public string BitcloutPublicKey
        {
            get
            {
                return _BitcloutPublicKey;
            }
            set
            {
                _BitcloutPublicKey = value;
                OnPropertyChanged("BitcloutPublicKey");
            }
        }

        string _PathToRegistredUsers;
        /// <summary>
        /// Логин Bitclout
        /// </summary>
        public string PathToRegistredUsers
        {
            get
            {
                return _PathToRegistredUsers;
            }
            set
            {
                _PathToRegistredUsers = value;
                OnPropertyChanged("PathToRegistredUsers");
            }
        }

        string _ChromePath;
        /// <summary>
        /// Логин Bitclout
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
