using Bitclout.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

        bool stop = false;

        ObservableCollection<UserRegistrationInfo> _RegistrationInfo { get; set; } = new ObservableCollection<UserRegistrationInfo>(UserRegistrationInfo.LoadUsers());

        ObservableCollection<String> Posts { get; set; } = new ObservableCollection<string>(LoadPosts());

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
                        GetRegistredUsers();
                    }));
            }
        }

        private RelayCommand _AddPostsCommand;
        public RelayCommand AddPostsCommand
        {
            get
            {
                return _AddPostsCommand ??
                    (_AddPostsCommand = new RelayCommand(obj =>
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info("Добавление новых записей из файла ->");
                        GetPosts();
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

        void GetRegistredUsers()
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
                            var str = sr.ReadLine();
                            if (RegistrationInfo.Where(x => x.BitcloudPhrase == str).Count() == 0)
                            {
                                RegistrationInfo.Add(new UserRegistrationInfo(str));
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
            Task.Run(() =>
            {
                while (!stop)
                {

                    try
                    {
                        if (RegistrationInfo.Where(x => x.LastPostDate.AddHours(12) < DateTime.Now).Count() != 0)
                        {
                            var info = RegistrationInfo.Where(x => x.LastPostDate.AddHours(12) < DateTime.Now).First();
                            if (chromeWorker.InitializeChromeDriver("PostChrome", chromeWorker.PostChromeDriver))
                                if (chromeWorker.LoginToBitclout(info.BitcloudPhrase, chromeWorker.PostChromeDriver))
                                    if (Posts.Count != 0)
                                        if (chromeWorker.MakePost(Posts.First(), chromeWorker.PostChromeDriver))
                                        {
                                            NLog.LogManager.GetCurrentClassLogger().Info($"Пост сделан успешно");
                                            RegistrationInfo.Where(x => x.BitcloudPhrase == info.BitcloudPhrase).First().LastPostDate = DateTime.Now;
                                            if (info.LastSendDimondDate.Year == 0)
                                                RegistrationInfo.Where(x => x.BitcloudPhrase == info.BitcloudPhrase).First().LastSendDimondDate = DateTime.Now.AddMinutes(15);
                                            UserRegistrationInfo.SaveUsers(RegistrationInfo.ToList());
                                            Posts.RemoveAt(0);
                                            SavePosts(Posts.ToList());
                                        }
                        }
                    }
                    catch (Exception ex)
                    {
                        chromeWorker.CloseDdriver(chromeWorker.PostChromeDriver);
                        NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    }
                    finally
                    {
                        Thread.Sleep(5000);
                    }
                }
            });

            Task.Run(() =>
            {
                while (!stop)
                {
                    try
                    {
                        if (RegistrationInfo.Where(x => x.LastSendDimondDate.AddHours(12) < DateTime.Now).Count() != 0)
                        {
                            var info = RegistrationInfo.Where(x => x.LastPostDate.AddHours(12) < DateTime.Now && x.LastPostDate.Year != 0).First();
                            if (chromeWorker.InitializeChromeDriver("DiamondChrome", chromeWorker.DiamondChromeDriver))
                                if (chromeWorker.LoginToBitclout(info.BitcloudPhrase, chromeWorker.DiamondChromeDriver))
                                    if (chromeWorker.SendDiamond(settings.BitcloutPublicKey, chromeWorker.DiamondChromeDriver))
                                    {
                                        NLog.LogManager.GetCurrentClassLogger().Info($"Токены отправлены");
                                        RegistrationInfo.Where(x => x.BitcloudPhrase == info.BitcloudPhrase).First().LastSendDimondDate = DateTime.Now;
                                        UserRegistrationInfo.SaveUsers(RegistrationInfo.ToList());
                                    }
                        }
                    }
                    catch (Exception ex)
                    {
                        chromeWorker.CloseDdriver(chromeWorker.PostChromeDriver);
                        NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                    }
                    finally
                    {
                        Thread.Sleep(5000);
                    }
                }
            });
        }

        void GetPosts()
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
                            var str = sr.ReadLine();
                            if (RegistrationInfo.Where(x => x.BitcloudPhrase == str).Count() == 0)
                            {
                                Posts.Add(str);
                                NLog.LogManager.GetCurrentClassLogger().Info($"Данные для {str[0]} успешно считаны");
                            }
                        }
                        SavePosts(Posts.ToList());
                        NLog.LogManager.GetCurrentClassLogger().Info($"Все посты из файла получены");
                    }
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, "Произошла ошибка при попытке получения данных для постов из файла");
                }
            }
            else
                NLog.LogManager.GetCurrentClassLogger().Info("Диологовое окно выбора файла пользователей закрыто");
        }

        public static List<string> LoadPosts()
        {
            try
            {
                using (StreamReader sr = new StreamReader("bin\\Posts.dat"))
                {
                    return SerializeHelper.Desirialize<List<string>>(sr.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Не удалось загрузить посты");
                return new List<string>();
            }
        }

        /// <summary>
        /// Сохранение настроек в файл
        /// </summary>
        /// <returns></returns>
        public bool SavePosts(List<string> posts)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("bin\\Posts.dat"))
                {
                    sw.Write(SerializeHelper.Serialize(posts));
                    return true;
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Не удалось сохранить посты.");
                return false;
            }
        }
    }
}
