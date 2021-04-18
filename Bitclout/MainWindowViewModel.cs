using Bitclout.Exceptions;
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

        public static ObservableCollection<UserInfo> RegistredUsers { get; set; } = new ObservableCollection<UserInfo>();

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
            PhoneWorker.ApiKey = settings.SMSApiKey;
        }

        void SaveRegistredUser()
        {
            foreach (var item in RegistredUsers.ToList())
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



        void BotStart()
        {
            while (!stop)
            {
                try
                {
                    NLog.LogManager.GetCurrentClassLogger().Info("Запуск драйвера для Bitclout ->");

                    if (!bitclout)
                    {
                        chromeWorker.InitializeBitcloutChromeDriver();
                        chromeWorker.LoginToBitclout();
                        bitclout = true;
                    }

                    if (RegistrationInfo.Count == 0)
                        throw new OutOfRegistrationInfoException("Закончились аккаунты для регистрации");

                    Task.Run(() =>
                    {
                        if (!selltop)
                        {
                            selltop = true;
                            var usrtosell = chromeWorker.GetTopSellName();
                            if (usrtosell != "")
                                chromeWorker.SellAllCreatorCoins(usrtosell);
                            selltop = false;
                        }
                    });

                    if (settings.IsBuyCoins)
                        RegistredUsers.Add(chromeWorker.RegisterNewBitсlout(RegistrationInfo[0]));
                    else
                        RegistredUsers.Add(chromeWorker.RegisterWithoutBuy(RegistrationInfo[0]));


                    pncode = 0;

                    NLog.LogManager.GetCurrentClassLogger().Info($"Конец автоматической регистрации");
                }
                catch (OutOfProxyException ex)
                {
                    if (ProxyWorker.ChangeProxyCountry())
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                        settings.CurrentProxy = null;
                        continue;
                    }
                    else
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                        MessageBox.Show(ex.Message);
                        break;
                    }
                }
                catch (FailedInitializeBitcloutChromeDriver ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    MessageBox.Show(ex.Message);
                    break;
                }
                catch (OutOfRegistrationInfoException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    MessageBox.Show(ex.Message);
                    break;
                }
                catch (FailToStartBitcloutChromeDriverException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    MessageBox.Show(ex.Message);
                    break;
                }
                catch (NoPhoneBalanceException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    MessageBox.Show(ex.Message);
                    break;
                }
                catch (PhoneCodeNotSendException ex)
                {
                    pncode++;
                    if (pncode > 4)
                    {
                        pncode = 0;
                        //Thread.Sleep(60000 * 5);
                    }
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    continue;
                }
                catch (BadProxyException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    settings.CurrentProxy.AccountsRegistred = 2;
                    continue;
                }
                catch (NameAlreadyExistException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    continue;
                }
                catch (FailSendBitcloutException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    continue;
                }
                catch (FailPrepareToBuyCreatorCoinsException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    continue;
                }
                catch (FailConfirmBuyException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    continue;
                }
                catch (FailInitializeRegChromeDriverException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    continue;
                }
                catch (FailedSaveProfileException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    continue;
                }
                catch (PhoneNumberAlreadyUsedException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    continue;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("ERR_PROXY_CONNECTION_FAILED"))
                    {
                        settings.CurrentProxy = null;
                    }
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    continue;
                }
                finally
                {

                    if (RegistrationInfo.Count != 0)
                        Application.Current.Dispatcher.Invoke(() => { RegistrationInfo.RemoveAt(0); });

                    UserRegistrationInfo.SaveUsers(RegistrationInfo.ToList());

                    settings.SaveSettings();

                    SaveRegistredUser();
                }
            }
        }
    }
}
