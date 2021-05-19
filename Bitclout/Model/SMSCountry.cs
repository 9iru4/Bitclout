using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bitclout.Model
{
    [Serializable]
    public class SMSCountry : INotifyPropertyChanged
    {
        int _SMSCode = 0;

        public int SMSCode
        {
            get
            {
                return _SMSCode;
            }
            set
            {
                _SMSCode = value;
                OnPropertyChanged("SMSCode");
            }
        }

        string _SMSHTMLCode = "iti-0__item-ru";

        public string SMSHTMLCode
        {
            get
            {
                return _SMSHTMLCode;
            }
            set
            {
                _SMSHTMLCode = value;
                OnPropertyChanged("SMSHTMLCode");
            }
        }

        string _Name = "Ru";

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public SMSCountry()
        {
        }

        public SMSCountry(int sMSCode, string sMSHTMLCode, string name)
        {
            SMSCode = sMSCode;
            SMSHTMLCode = sMSHTMLCode;
            Name = name;
        }
    }
}
