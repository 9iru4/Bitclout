using Bitclout.Model;
using System;
using System.IO;
using System.Net;

namespace Bitclout.Worker
{
    public enum ServiceCodes { lt, none }

    public static class PhoneWorker
    {
        static string _ApiKey;
        /// <summary>
        /// Ключ апи
        /// </summary>
        public static string ApiKey
        {
            get
            {
                return _ApiKey;
            }
            set
            {
                _ApiKey = value;
            }
        }

        /// <summary>
        /// Получение телефона для отправки смс
        /// </summary>
        public static PhoneNumber GetPhoneNumber(ServiceCodes serviceCode)
        {
            //WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=getNumber&service=fx&operator=any&country=0");//get number
            WebRequest request = WebRequest.Create("https://smshub.org/stubs/handler_api.php?api_key=" + ApiKey + "&action=getNumber&service=" + serviceCode.ToString() + "&operator=any&country=0");
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    if (result.Contains("ACCESS_NUMBER"))
                    {
                        var num = result.Split(':');
                        if (num[2][0] == '7')
                        {
                            num[2] = num[2].Remove(0, 1);
                        }
                        return new PhoneNumber(num[1], num[2], num[0]);
                    }
                    else
                    {
                        throw new Exception("Не удалось получить номер");
                    }
                }
            }
        }

        /// <summary>
        /// Отправка уведомления об отправленном смс
        /// </summary>
        public static PhoneNumber MessageSend(PhoneNumber phoneNumber)
        {
            //WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=1&id=" + Id);//activate number
            WebRequest request = WebRequest.Create("https://smshub.org/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=1&id=" + phoneNumber.ID);

            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    phoneNumber.StatusCode = result;
                    return phoneNumber;
                }
            }
        }

        /// <summary>
        /// Получаем смс код
        /// </summary>
        public static PhoneNumber GetCode(PhoneNumber phoneNumber)
        {
            //WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=getStatus&id=" + Id);//get message
            WebRequest request = WebRequest.Create("https://smshub.org/stubs/handler_api.php?api_key=" + ApiKey + "&action=getStatus&id=" + phoneNumber.ID);//get message

            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    if (result.Contains("STATUS_OK"))
                    {
                        var res = result.Split(':');
                        phoneNumber.StatusCode = res[0];
                        phoneNumber.Code = res[1];
                    }
                    else
                    {
                        phoneNumber.StatusCode = result;
                    }
                    return phoneNumber;
                }
            }
        }

        ///// <summary>
        ///// Повторная попытка получения кода
        ///// </summary>
        ///// <returns>Получен ли код</returns>
        //public bool RetryCode()
        //{
        //    //WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=3&id=" + Id);//activate number
        //    WebRequest request = WebRequest.Create("https://smshub.org/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=3&id=" + Id);
        //    WebResponse response = request.GetResponse();
        //    using (Stream stream = response.GetResponseStream())
        //    {
        //        using (StreamReader reader = new StreamReader(stream))
        //        {

        //            var result = reader.ReadToEnd();
        //            StatusCode = result;
        //            if (result.Contains("ACCESS_RETRY_GET"))
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Сообщаем об успешном использовании номера
        /// </summary>
        public static PhoneNumber NumberConformation(PhoneNumber phoneNumber)
        {
            //WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=6&id=" + Id);//activate number
            WebRequest request = WebRequest.Create("https://smshub.org/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=6&id=" + phoneNumber.ID);

            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    phoneNumber.StatusCode = result;
                    return phoneNumber;
                }
            }
        }

        /// <summary>
        /// Сообщаем об отмене использования номера
        /// </summary>
        public static PhoneNumber DeclinePhone(PhoneNumber phoneNumber)
        {
            //WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=-1&id=" + Id);//activate number
            WebRequest request = WebRequest.Create("https://smshub.org/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=8&id=" + phoneNumber.ID);
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    phoneNumber.StatusCode = result;
                    return phoneNumber;
                }
            }
        }
    }
}
