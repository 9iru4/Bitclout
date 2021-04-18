using Bitclout.Model;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bitclout
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ChromeWorker chromeWorker = new ChromeWorker();
        public static Settings settings { get; set; } = Settings.LoadSettings();

        bool bitclout = false;
        bool stop = false;
        bool selltop = false;
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

        bool _StartEnabled = true;
        int pncode = 0;
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

                            BotStart();
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
                                RegistrationInfo.Add(new UserRegistrationInfo(str[0], str[1]));
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

        void GetRefistredUsers()
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Получение пользователей из файла ->");
            try
            {
                using (StreamReader sr = new StreamReader(settings.PathToRegistredUsers, Encoding.Default))
                {
                    while (sr.Peek() >= 0)
                    {
                        var str = sr.ReadLine().Split(';');
                        if (RegistrationInfo.Where(x => x.Name == str[0]).Count() == 0)
                        {
                            RegistrationInfo.Add(new UserRegistrationInfo(str[0], str[2]));
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

        void BotStart()
        {
            while (!stop)
            {
                var usr = new UserRegistrationInfo();
                try
                {
                    NLog.LogManager.GetCurrentClassLogger().Info("Запуск драйвера для Bitclout ->");

                    GetRefistredUsers();


                    if (RegistrationInfo.Where(x => !x.IsRegistred).Count() != 0)
                    {
                        usr = RegistrationInfo.Where(x => !x.IsRegistred).FirstOrDefault();
                        chromeWorker.InitializeBitcloutChromeDriver();
                        chromeWorker.LoginToBitclout(usr.BitcloudPhrase);
                        var usrtosell = chromeWorker.GetTopSellName();
                        if (usrtosell != "")
                        {
                            chromeWorker.SellAllCreatorCoins(usrtosell);


                            chromeWorker.SendBitclout(settings.BitcloutPublicKey);
                        }
                        chromeWorker.EndRegistration();
                    }
                    NLog.LogManager.GetCurrentClassLogger().Info($"Конец автоматической регистрации");
                }
                catch (Exception ex)
                {
                    chromeWorker.EndRegistration();
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    continue;
                }
                finally
                {
                    chromeWorker.EndRegistration();
                    RegistrationInfo.Where(x => x.Name == usr.Name).FirstOrDefault().IsRegistred = true;
                    UserRegistrationInfo.SaveUsers(RegistrationInfo.ToList());
                    settings.SaveSettings();
                    Thread.Sleep(10000);
                }
            }
        }
    }
}
