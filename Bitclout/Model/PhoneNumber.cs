using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bitclout.Model
{
    public class PhoneNumber : INotifyPropertyChanged
    {
        string _ID;
        /// <summary>
        /// Ид
        /// </summary>
        public string ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
                OnPropertyChanged("ID");
            }
        }

        string _Number;
        /// <summary>
        /// Номер
        /// </summary>
        public string Number
        {
            get
            {
                return _Number;
            }
            set
            {
                _Number = value;
                OnPropertyChanged("Number");
            }
        }

        string _StatusCode;
        /// <summary>
        /// Статус код
        /// </summary>
        public string StatusCode
        {
            get
            {
                return _Number;
            }
            set
            {
                _StatusCode = value;
                OnPropertyChanged("StatusCode");
            }
        }

        string _Code;
        /// <summary>
        /// Код смс
        /// </summary>
        public string Code
        {
            get
            {
                return _Code;
            }
            set
            {
                _Code = value;
                OnPropertyChanged("Code");
            }
        }

        public bool UseAgain { get; set; }

        public PhoneNumber()
        {

        }

        public PhoneNumber(string iD, string number, string statusCode)
        {
            ID = iD;
            Number = number;
            StatusCode = statusCode;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
