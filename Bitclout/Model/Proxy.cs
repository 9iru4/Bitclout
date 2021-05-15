using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bitclout.Model
{
    public enum ProxyStatus { NotUsed = 0, Good = 1, BadSoax = 2, Died = 3 }
    /// <summary>
    /// Класс прокси
    /// </summary>
    [Serializable]
    public class Proxy : INotifyPropertyChanged
    {
        string _ID;
        /// <summary>
        /// Ид прокси
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

        /// <summary>
        /// ИП прокси
        /// </summary>
        string _IP;
        public string IP
        {
            get
            {
                return _IP;
            }
            set
            {
                _IP = value;
                OnPropertyChanged("IP");
            }
        }


        string _Host;
        /// <summary>
        /// Адрес
        /// </summary>
        public string Host
        {
            get
            {
                return _Host;
            }
            set
            {
                _Host = value;
                OnPropertyChanged("Host");
            }
        }

        string _Port;
        /// <summary>
        /// Порт
        /// </summary>
        public string Port
        {
            get
            {
                return _Port;
            }
            set
            {
                _Port = value;
                OnPropertyChanged("Port");
            }
        }

        string _StatusCode;
        /// <summary>
        /// Пароль прокси
        /// </summary>
        public string StatusCode
        {
            get
            {
                return _StatusCode;
            }
            set
            {
                _StatusCode = value;
                OnPropertyChanged("StatusCode");
            }
        }


        ProxyStatus _CurrentStatus;
        /// <summary>
        /// Количество регистраций
        /// </summary>
        public ProxyStatus CurrentStatus
        {
            get
            {
                return _CurrentStatus;
            }
            set
            {
                _CurrentStatus = value;
                OnPropertyChanged("CurrentStatus");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public Proxy()
        {
            ID = "";
            _IP = "";
            Host = "";
            Port = "";
            CurrentStatus = ProxyStatus.NotUsed;
        }

        /// <summary>
        /// Инициализатор прокси
        /// </summary>
        /// <param name="iD">ИД</param>
        /// <param name="iP">ИП</param>
        /// <param name="host">Адрес</param>
        /// <param name="port">Порт</param>
        /// <param name="accountRegistred"></param>
        public Proxy(string iD, string iP, string host, string port, ProxyStatus prxStatus = ProxyStatus.NotUsed)
        {
            ID = iD;
            _IP = iP;
            Host = host;
            Port = port;
            CurrentStatus = prxStatus;
        }

        public Proxy(string iD, string host, string port, ProxyStatus prxStatus = ProxyStatus.NotUsed)
        {
            ID = iD;
            Host = host;
            Port = port;
            CurrentStatus = prxStatus;
        }

        /// <summary>
        /// Получение строки Адрес:Порт
        /// </summary>
        /// <returns>Строка</returns>
        public string GetAddress()
        {
            return $"{Host}:{Port}";
        }
    }
}
