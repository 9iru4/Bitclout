using System;

namespace Bitclout.Model
{
    /// <summary>
    /// Класс прокси
    /// </summary>
    [Serializable]
    public class Proxy
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
            }
        }

        string _UserName;
        /// <summary>
        /// Логин прокси
        /// </summary>
        public string UserName
        {
            get
            {
                return _UserName;
            }
            set
            {
                _UserName = value;
            }
        }

        string _Pass;
        /// <summary>
        /// Пароль прокси
        /// </summary>
        public string Pass
        {
            get
            {
                return _Pass;
            }
            set
            {
                _Pass = value;
            }
        }

        uint _AccountsRegistred;
        /// <summary>
        /// Количество регистраций
        /// </summary>
        public uint AccountsRegistred
        {
            get
            {
                return _AccountsRegistred;
            }
            set
            {
                _AccountsRegistred = value;
            }
        }

        public Proxy()
        {
            ID = "";
            _IP = "";
            Host = "";
            Port = "";
            UserName = "";
            Pass = "";
            AccountsRegistred = 0;
        }

        /// <summary>
        /// Инициализатор прокси
        /// </summary>
        /// <param name="iD">ИД</param>
        /// <param name="iP">ИП</param>
        /// <param name="host">Адрес</param>
        /// <param name="port">Порт</param>
        /// <param name="user">Логин</param>
        /// <param name="pass">Пароль</param>
        /// <param name="accountRegistred"></param>
        public Proxy(string iD, string iP, string host, string port, string user, string pass, uint accountRegistred = 0)
        {
            ID = iD;
            _IP = iP;
            Host = host;
            Port = port;
            UserName = user;
            Pass = pass;
            AccountsRegistred = accountRegistred;
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
