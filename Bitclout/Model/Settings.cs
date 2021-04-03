using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bitclout.Model
{
    public class Settings : INotifyPropertyChanged
    {
        string _SMSApiKey;
        public string SMSApiKey
        {
            get
            {
                return _SMSApiKey;
            }
            set
            {
                _SMSApiKey = value;
                OnPropertyChanged("SMSApiKey");
            }
        }
        string _TwitterApiKey;
        public string TwitterApiKey
        {
            get
            {
                return _TwitterApiKey;
            }
            set
            {
                _TwitterApiKey = value;
                OnPropertyChanged("TwitterApiKey    ");
            }
        }

        string _ChromePath;
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
        string _PhotosPath;
        public string PhotosPath
        {
            get
            {
                return _PhotosPath;
            }
            set
            {
                _PhotosPath = value;
                OnPropertyChanged("PhotosPath");
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
