using Bitclout.Exceptions;
using Bitclout.Model;
using Bitclout.Worker;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Bitclout
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ChromeWorker chromeWorker = new ChromeWorker();
        public ProxyWorker proxyWorker { get; set; } = new ProxyWorker();
        public static Settings settings { get; set; } = Settings.LoadSettings();

        bool bitcloutMain = false;
        bool stop = false;

        ObservableCollection<UserRegistrationInfo> _RegistrationInfo = new ObservableCollection<UserRegistrationInfo>(UserRegistrationInfo.LoadUsers());
        public ObservableCollection<string> SMSCountrys { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> BotWorkModes { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ProxyTypes { get; set; } = new ObservableCollection<string>();

        string _SelectedSMS = settings.SMSCountry.Name;
        public string SelectedSMS
        {
            get
            {
                return _SelectedSMS;
            }
            set
            {
                _SelectedSMS = value;
                switch (_SelectedSMS)
                {
                    case "Ru":
                        settings.SMSCountry = new SMSCountry(0, "iti-0__item-ru", "Ru");
                        break;
                    case "Ua":
                        settings.SMSCountry = new SMSCountry(1, "iti-0__item-ua", "Ua");
                        break;
                    case "Gb":
                        settings.SMSCountry = new SMSCountry(16, "iti-0__item-gb", "Gb");
                        break;
                    default:
                        break;
                }
                OnPropertyChanged("SelectedSMS");
            }
        }

        string _SelectedPrx = settings.ProxyType.Name;
        public string SelectedPrx
        {
            get
            {
                return _SelectedPrx;
            }
            set
            {
                _SelectedPrx = value;
                switch (_SelectedPrx)
                {
                    case "NotUsed":
                        settings.ProxyType = new ProxyType(PrxType.NotUsed, "NotUsed");
                        break;
                    case "Cycle":
                        settings.ProxyType = new ProxyType(PrxType.Cycle, "Cycle");
                        break;
                    case "OnlyFirst":
                        settings.ProxyType = new ProxyType(PrxType.OnlyFirst, "OnlyFirst");
                        break;
                    case "SOAX":
                        settings.ProxyType = new ProxyType(PrxType.SOAX, "SOAX");
                        break;
                    default:
                        break;
                }
                OnPropertyChanged("SelectedPrx");
            }
        }

        string _SelectedMode = settings.BotWorkMode.Name;
        public string SelectedMode
        {
            get
            {
                return _SelectedMode;
            }
            set
            {
                _SelectedMode = value;
                switch (_SelectedMode)
                {
                    case "OnlyReg":
                        settings.BotWorkMode = new BotWorkMode(WorkType.OnlyReg, "OnlyReg");
                        break;
                    case "SellMain":
                        settings.BotWorkMode = new BotWorkMode(WorkType.SellMain, "SellMain");
                        break;
                    case "SellReg":
                        settings.BotWorkMode = new BotWorkMode(WorkType.SellReg, "SellReg");
                        break;
                    case "RegAndSell":
                        settings.BotWorkMode = new BotWorkMode(WorkType.RegAndSell, "RegAndSell");
                        break;
                    case "OnlyBuyCoins":
                        settings.BotWorkMode = new BotWorkMode(WorkType.OnlyBuyCoins, "OnlyBuyCoins");
                        break;
                    case "BuyCoinsAndSellMain":
                        settings.BotWorkMode = new BotWorkMode(WorkType.BuyCoinsAndSellMain, "BuyCoinsAndSellMain");
                        break;
                    case "OnlyMerlin":
                        settings.BotWorkMode = new BotWorkMode(WorkType.OnlyMerlin, "OnlyMerlin");
                        break;
                    case "MerlinAndSellReg":
                        settings.BotWorkMode = new BotWorkMode(WorkType.MerlinAndSellReg, "MerlinAndSellReg");
                        break;
                    default:
                        break;
                }
                OnPropertyChanged("SelectedMode");
            }
        }

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

        public static ObservableCollection<UserInfo> RegistredUsers { get; set; } = new ObservableCollection<UserInfo>(UserInfo.LoadRegistredUsers());

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

        private RelayCommand _AddProxyCommand;
        public RelayCommand AddProxyCommand
        {
            get
            {
                return _AddProxyCommand ??
                    (_AddProxyCommand = new RelayCommand(obj =>
                    {
                        GetProxyFromFile();
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

            SMSCountrys.Add("Ru");
            SMSCountrys.Add("Ua");
            SMSCountrys.Add("Gb");

            ProxyTypes.Add("NotUsed");
            ProxyTypes.Add("Cycle");
            ProxyTypes.Add("OnlyFirst");
            ProxyTypes.Add("SOAX");

            BotWorkModes.Add("OnlyReg");
            BotWorkModes.Add("SellMain");
            BotWorkModes.Add("SellReg");
            BotWorkModes.Add("RegAndSell");
            BotWorkModes.Add("OnlyBuyCoins");
            BotWorkModes.Add("BuyCoinsAndSellMain");
            BotWorkModes.Add("OnlyMerlin");
            BotWorkModes.Add("MerlinAndSellReg");
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

        void GetProxyFromFile()
        {
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
                            proxyWorker.AddProxyToCollection(new Proxy(str[0], str[1], str[2]));
                        }
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

        void StartBot()
        {
            while (!stop)
            {
                bool iserr = true;
                PhoneNumber pn = null;
                try
                {
                    switch (settings.ProxyType.Type)
                    {
                        case PrxType.NotUsed:
                            settings.CurrentProxy = null;
                            break;
                        case PrxType.OnlyFirst:
                            settings.CurrentProxy = proxyWorker.Proxy.First();
                            break;
                        case PrxType.SOAX:
                        case PrxType.Cycle:
                            settings.CurrentProxy = proxyWorker.GetProxyFromCollection();
                            if (settings.CurrentProxy == null)
                                throw new OutOfProxyException("Закончились рабочие прокси");
                            break;
                        default:
                            break;
                    }

                    if (RegistrationInfo.Count == 0)
                        throw new OutOfRegistrationInfoException("Закончились аккаунты для регистрации");

                    NLog.LogManager.GetCurrentClassLogger().Info("Запуск драйвера для Bitclout ->");

                    if ((settings.SendBitlout || settings.BotWorkMode.Type == WorkType.OnlyBuyCoins || settings.BotWorkMode.Type == WorkType.BuyCoinsAndSellMain || settings.BotWorkMode.Type == WorkType.SellMain) && !bitcloutMain)
                    {
                        chromeWorker.BitcloutChromeDriver = chromeWorker.InitializeChromeDriver(@"\MainChrome");

                        chromeWorker.BitcloutChromeDriver.Manage().Window.Maximize();

                        chromeWorker.LoginToBitclout(chromeWorker.BitcloutChromeDriver, settings.BitcloutSeedPhrase);

                        bitcloutMain = true;
                    }

                    if (bitcloutMain && (settings.BotWorkMode.Type == WorkType.SellMain || settings.BotWorkMode.Type == WorkType.BuyCoinsAndSellMain))
                        Task.Run(() =>
                        {
                            if (settings.SellMoreThan != 0)
                            {
                                var usrtosell = chromeWorker.GetTopSellName(chromeWorker.BitcloutChromeDriver);
                                if (usrtosell != "")
                                    if (!chromeWorker.SellAllCreatorCoins(usrtosell, chromeWorker.BitcloutChromeDriver))
                                        NLog.LogManager.GetCurrentClassLogger().Info("Не удалось продать все коины");
                                NLog.LogManager.GetCurrentClassLogger().Info("Коины проданы");
                            }
                        });

                    if (settings.BotWorkMode.Type != WorkType.SellReg || settings.BotWorkMode.Type != WorkType.SellMain)
                    {
                        while (pn == null)//Получаем номер, пока не получим
                        {
                            pn = PhoneWorker.GetPhoneNumber(ServiceCodes.lt);
                            Thread.Sleep(settings.MainDelay);
                        }

                        chromeWorker.RegChromeDriver = chromeWorker.InitializeChromeDriver(@"\Chrome", settings.CurrentProxy);

                        chromeWorker.RegChromeDriver.Manage().Window.Maximize();

                        RegistredUsers.Add(chromeWorker.RegNewBitclout(RegistrationInfo[0], pn));

                        settings.CurrentProxy.CurrentStatus = ProxyStatus.Good;

                        UserInfo.SaveRegistredUsers(RegistredUsers.ToList());

                        PhoneWorker.NumberConformation(pn);

                        NLog.LogManager.GetCurrentClassLogger().Info($"Конец автоматической регистрации");
                    }
                }
                catch (NoBitcloutBalanceException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (BadSyncResponseException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (OutOfProxyException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    MessageBox.Show("Закончились прокси");
                    stop = true;
                    break;
                }
                catch (FailedInitializeBitcloutChromeDriver ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    MessageBox.Show(ex.Message);
                    stop = true;
                    break;
                }
                catch (OutOfRegistrationInfoException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    MessageBox.Show(ex.Message);
                    stop = true;
                    break;
                }
                catch (FailToStartBitcloutChromeDriverException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    MessageBox.Show(ex.Message);
                    stop = true;
                    break;
                }
                catch (NoPhoneBalanceException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    MessageBox.Show(ex.Message);
                    stop = true;
                    break;
                }
                catch (PhoneCodeNotSendException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (BadProxyException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    if (settings.CurrentProxy == null || settings.ProxyType.Type == PrxType.OnlyFirst) continue;
                    if (settings.ProxyType.Type == PrxType.SOAX)
                    {
                        settings.CurrentProxy.CurrentStatus = ProxyStatus.BadSoax;
                    }
                    else
                    {
                        settings.CurrentProxy.CurrentStatus = ProxyStatus.Died;
                    }
                }
                catch (NameAlreadyExistException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (FailSendBitcloutException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (FailPrepareToBuyCreatorCoinsException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (FailConfirmBuyException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (FailInitializeRegChromeDriverException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (FailedSaveProfileException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (PhoneNumberAlreadyUsedException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (OpenQA.Selenium.NoSuchElementException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (OpenQA.Selenium.WebDriverException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    iserr = true;
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    PhoneWorker.DeclinePhone(pn);
                }
                finally
                {
                    try
                    {
                        PhoneWorker.DeclinePhone(pn);
                    }
                    catch
                    {
                    }

                    if (RegistrationInfo.Count != 0)
                        Application.Current.Dispatcher.Invoke(() => { RegistrationInfo.RemoveAt(0); });

                    UserRegistrationInfo.SaveUsers(RegistrationInfo.ToList());
                    try
                    {
                        if (chromeWorker.RegChromeDriver != null)
                            try
                            {
                                chromeWorker.EndRegistration(chromeWorker.RegChromeDriver);
                            }
                            catch (Exception)
                            {
                                if (settings.ProxyType.Type == PrxType.SOAX)
                                    settings.CurrentProxy.CurrentStatus = ProxyStatus.BadSoax;
                                else
                                    settings.CurrentProxy.CurrentStatus = ProxyStatus.Died;
                                chromeWorker.RegChromeDriver.Quit();
                            }
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                        iserr = true;
                    }
                    try
                    {
                        if (iserr)
                        {
                            foreach (var item in Process.GetProcessesByName("chromedriver"))
                            {
                                KillProcessAndChildrens(item.Id);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    }
                    try
                    {
                        if (iserr)
                        {
                            foreach (var item in Process.GetProcessesByName("chrome"))
                            {
                                KillProcessAndChildrens(item.Id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    }
                   


                    if (settings.CurrentProxy != null)
                        proxyWorker.ChangeProxyStatus(settings.CurrentProxy.ID, settings.CurrentProxy.CurrentStatus);

                    settings.SaveSettings();

                    UserInfo.SaveRegistredUsers(RegistredUsers.ToList());
                }
            }
        }

        private static void KillProcessAndChildrens(int pid)
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();

            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }

            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                }
            }
        }

        void StartSell()
        {
            while (!stop && (settings.BotWorkMode.Type == WorkType.SellReg || settings.BotWorkMode.Type == WorkType.RegAndSell || settings.BotWorkMode.Type == WorkType.MerlinAndSellReg))
            {
                bool err = false;
                UserInfo usr = null;
                try
                {
                    var col = RegistredUsers.Where(x => !x.IsSell);
                    if (col.Count() != 0)
                    {
                        usr = col.First();

                        chromeWorker.SellChromeDriver = chromeWorker.InitializeChromeDriver(@"\SellChrome", isIncognito: true);
                        chromeWorker.SellChromeDriver.Manage().Window.Maximize();
                        chromeWorker.LoginToBitclout(chromeWorker.SellChromeDriver, usr.BitcloutSeedPhrase);

                        chromeWorker.SendAllBitclout(settings.PublicKey, chromeWorker.SellChromeDriver);
                    }
                }
                catch (FailToStartBitcloutChromeDriverException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    err = true;
                    break;
                }
                catch (FailSendBitcloutException ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                }
                finally
                {
                    if (usr != null)
                    {
                        if (!err)
                            RegistredUsers.Where(x => x.BitcloutSeedPhrase == usr.BitcloutSeedPhrase).FirstOrDefault().IsSell = true;
                        UserInfo.SaveRegistredUsers(RegistredUsers.ToList());
                        try
                        {
                            chromeWorker.EndRegistration(chromeWorker.SellChromeDriver);
                        }
                        catch (Exception)
                        {
                            chromeWorker.SellChromeDriver.Quit();
                        }
                    }
                    Thread.Sleep(7000);
                }
            }
        }

        void BotStart()
        {
            Task.Run(() => StartSell());
            Task.Run(() => StartBot());
        }
    }
}
