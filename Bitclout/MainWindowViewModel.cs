using Bitclout.Exceptions;
using Bitclout.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Bitclout
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ChromeWorker chromeWorker = new ChromeWorker();
        public static Settings settings { get; set; } = Settings.LoadSettings();
        bool stop = false;
        bool bitclout = false;

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

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public MainWindowViewModel()
        {
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

                    chromeWorker.SellAllCreatorCoins(chromeWorker.GetTopSellName());

                    Thread.Sleep(settings.SellDelayTime);

                    NLog.LogManager.GetCurrentClassLogger().Info($"Конец автоматической продажи");
                }
                catch (FailedInitializeBitcloutChromeDriver ex)
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
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    continue;
                }
            }
        }
    }
}
