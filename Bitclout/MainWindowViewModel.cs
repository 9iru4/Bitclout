using Bitclout.Model;
using Bitclout.Worker;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bitclout
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ChromeWorker chromeWorker = new ChromeWorker();
        public static Settings settings { get; set; } = Settings.LoadSettings();

        bool bitclout = false;
        bool stop = false;

        ObservableCollection<UserRegistrationInfo> _RegistrationInfo = new ObservableCollection<UserRegistrationInfo>(UserRegistrationInfo.LoadUsers());

        public ObservableCollection<UserRegistrationInfo> RegistrationInfo
        {
            get
            {
                return _RegistrationInfo;
            }
            set
            {
                _RegistrationInfo = value;
                if (value != null)
                    UserRegistrationInfo.SaveUsers(RegistrationInfo.ToList());
            }
        }

        public static ObservableCollection<UserInfo> RegistredUsers { get; set; } = new ObservableCollection<UserInfo>();

        bool _StartEnabled = true;

        public bool StartEnabled
        {
            get
            {
                return _StartEnabled;
            }
            set
            {
                _StartEnabled = value;
                OnPropertyChanged("StartEnabled");
            }
        }

        private RelayCommand _StartBotCommand;
        public RelayCommand StartBotCommand
        {
            get
            {
                return _StartBotCommand ??
                    (_StartBotCommand = new RelayCommand(obj =>
                    {
                        Task.Run(() =>
                        {
                            StartEnabled = false;
                            stop = false;
                            while (!stop)
                            {
                                if (bitclout)
                                {
                                    NLog.LogManager.GetCurrentClassLogger().Info("Запуск автоматической регистрации ->");
                                    BotStart();
                                }
                                else
                                {
                                    NLog.LogManager.GetCurrentClassLogger().Info($"Не запущен Bitclout {bitclout}");
                                    MessageBox.Show("Не запущен твитер или битклоут");
                                    stop = true;
                                    StartEnabled = true;
                                }
                            }
                        });

                    }));
            }
        }

        private RelayCommand _StopBotCommand;
        public RelayCommand StopBotCommand
        {
            get
            {
                return _StopBotCommand ??
                    (_StopBotCommand = new RelayCommand(obj =>
                    {
                        Task.Run(() =>
                        {
                            stop = true;
                            StartEnabled = true;
                        });

                    }));
            }
        }

        private RelayCommand _StartBitcloutCommand;
        public RelayCommand StartBitcloutCommand
        {
            get
            {
                return _StartBitcloutCommand ??
                    (_StartBitcloutCommand = new RelayCommand(obj =>
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info("Запуск драйвера для Bitclout ->");
                        bitclout = chromeWorker.StartBitcloutChromeDriver();
                    }));
            }
        }

        private RelayCommand _SaveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return _SaveCommand ??
                    (_SaveCommand = new RelayCommand(obj =>
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info("Сохранение настроек ->");
                        settings.SaveSettings();
                    }));
            }
        }

        private RelayCommand _AddUsersCommand;
        public RelayCommand AddUsersCommand
        {
            get
            {
                return _AddUsersCommand ??
                    (_AddUsersCommand = new RelayCommand(obj =>
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info("Добавление новых записей из файла ->");
                        GetUsersFromFile();
                    }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public MainWindowViewModel()
        {
            PhoneWorker.ApiKey = settings.SMSApiKey;
        }

        void SaveRegistredUser()
        {
            foreach (var item in RegistredUsers)
            {
                using (StreamWriter sw = new StreamWriter(@"bin\RegistredUsers.dat", true))
                {
                    sw.WriteLine(item.ToLogFile());
                    NLog.LogManager.GetCurrentClassLogger().Info($"Зарегистрированный пользователь {item.Name} успешно сохранен.");
                }
            }
            RegistredUsers.Clear();
        }

        void GetUsersFromFile()
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Получение пользователей из файла ->");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(openFileDialog.FileName, Encoding.Default))
                    {
                        while (sr.Peek() >= 0)
                        {
                            var str = sr.ReadLine().Split('|');
                            if (RegistrationInfo.Where(x => x.Name == str[0]).FirstOrDefault() == null)
                            {
                                RegistrationInfo.Add(new UserRegistrationInfo(str[0], str[1], str[2], str[3], str[4]));
                                NLog.LogManager.GetCurrentClassLogger().Info($"Данные для {str[0]} успешно считаны");
                            }
                        }
                        UserRegistrationInfo.SaveUsers(RegistrationInfo.ToList());
                        NLog.LogManager.GetCurrentClassLogger().Info($"Все пользователи из файла получены");
                    }
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, "Произошла ошибка при попытке получения данных для регистрации из файла");
                }
            }
            else
                NLog.LogManager.GetCurrentClassLogger().Info("Диологовое окно выбора файла пользователей закрыто");
        }

        void BotStart()
        {
            try
            {
                var user = RegistrationInfo[0];

                NLog.LogManager.GetCurrentClassLogger().Info($"Используются следующие данные для регистрации {user.Name}");



                var usr = chromeWorker.RegisterNewBitсlout(user);
                NLog.LogManager.GetCurrentClassLogger().Info($"При регистрации получены данные для пользователя {usr.Name}");

                RegistredUsers.Add(usr);

                RegistrationInfo.RemoveAt(0);

                UserRegistrationInfo.SaveUsers(RegistrationInfo.ToList());

                SaveRegistredUser();

                NLog.LogManager.GetCurrentClassLogger().Info($"Конец автоматической регистрации");
            }
            catch (Exception ex)
            {
                if (RegistrationInfo.Count == 0)
                {
                    stop = true;
                    StartEnabled = true;
                }
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Произошла ошибка при регистрации");
            }

        }
    }
}
