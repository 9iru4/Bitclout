using Bitclout.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitclout
{
    public class MainWindowViewModel
    {
        ChromeWorker chromeWorker = new ChromeWorker();


        private RelayCommand _TestCommand;
        public RelayCommand TestCommand
        {
            get
            {
                return _TestCommand ??
                    (_TestCommand = new RelayCommand(obj =>
                    {
                        chromeWorker.RegisterBitClout();
                    }));
            }
        }

        public MainWindowViewModel()
        {
            PhoneWorker.ApiKey = "15616Ua9cf34a219873ef904879716413f042e";
        }
    }
}
