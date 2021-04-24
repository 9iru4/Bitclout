using Bitclout.Exceptions;
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
            NLog.LogManager.GetCurrentClassLogger().Info($"Получаем номер телефона для сервиса {serviceCode} ->");
            WebRequest request = WebRequest.Create("https://smshub.org/stubs/handler_api.php?api_key=" + ApiKey + "&action=getNumber&service=" + serviceCode.ToString() + "&operator=any&country=1");
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
                        NLog.LogManager.GetCurrentClassLogger().Info($"Номер {num[2]} получен с результатом {num[0]}");
                        return new PhoneNumber(num[1], num[2], num[0]);
                    }
                    else
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info($"Не удалось получить номер с результатом {result}");
                        if (result.Contains("NO_BALANCE"))
                            throw new NoPhoneBalanceException("Недостаточно денег на телефоне.");
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Отправка уведомления об отправленном смс
        /// </summary>
        public static PhoneNumber MessageSend(PhoneNumber phoneNumber)
        {
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
            NLog.LogManager.GetCurrentClassLogger().Info($"Получаем код от сервиса для телефона {phoneNumber.Number} ->");
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
                        NLog.LogManager.GetCurrentClassLogger().Info($"Для номера {phoneNumber.Number} успешно получен код {phoneNumber.Code} результатом {phoneNumber.StatusCode}");
                    }
                    else
                    {
                        phoneNumber.StatusCode = result;
                        phoneNumber.Code = "";
                        NLog.LogManager.GetCurrentClassLogger().Info($"Для номера {phoneNumber.Number} не удалось получить код с результатом {phoneNumber.StatusCode}");
                    }
                    return phoneNumber;
                }
            }
        }

        /// <summary>
        /// Сообщаем об успешном использовании номера
        /// </summary>
        public static PhoneNumber NumberConformation(PhoneNumber phoneNumber)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем телефон {phoneNumber.Number} ->");

                WebRequest request = WebRequest.Create("https://smshub.org/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=6&id=" + phoneNumber.ID);

                WebResponse response = request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var result = reader.ReadToEnd();
                        phoneNumber.StatusCode = result;
                        NLog.LogManager.GetCurrentClassLogger().Info($"Телефон {phoneNumber.Number} подтвержден со статусом {phoneNumber.StatusCode} ->");
                        return phoneNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Произошла ошибка при подтверждении использования номера");
                return null;
            }

        }

        /// <summary>
        /// Сообщаем об отмене использования номера
        /// </summary>
        public static PhoneNumber DeclinePhone(PhoneNumber phoneNumber)
        {
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
