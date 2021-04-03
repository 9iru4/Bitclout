using Bitclout.Model;
using Bitclout.Worker;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Bitclout
{
    public class MainWindowViewModel
    {
        ChromeWorker chromeWorker = new ChromeWorker();
        public static Settings settings { get; set; } = Settings.LoadSettings();

        public ObservableCollection<UserRegistrationInfo> RegistrationInfo { get; set; } = new ObservableCollection<UserRegistrationInfo>(UserRegistrationInfo.LoadUsers());

        public static ObservableCollection<UserInfo> RegistredUsers { get; set; } = new ObservableCollection<UserInfo>();

        private RelayCommand _TestCommand;
        public RelayCommand TestCommand
        {
            get
            {
                return _TestCommand ??
                    (_TestCommand = new RelayCommand(obj =>
                    {
                        var user = RegistrationInfo[0];
                        RegistrationInfo.RemoveAt(0);
                        UserRegistrationInfo.SaveUsers(RegistrationInfo.ToList());
                        chromeWorker.RegisterBitClout(user);
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
                        GetUsersFromFile();
                    }));
            }
        }

        public MainWindowViewModel()
        {
            PhoneWorker.ApiKey = settings.SMSApiKey;
        }

        void GetUsersFromFile()
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
                            if (RegistrationInfo.Where(x => x.Name == str[0]).FirstOrDefault() == null)
                                RegistrationInfo.Add(new UserRegistrationInfo(str[0], str[1], str[2]));
                        }
                        UserRegistrationInfo.SaveUsers(RegistrationInfo.ToList());
                    }
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
