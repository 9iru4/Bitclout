﻿using Bitclout.Exceptions;
using Bitclout.Model;
using Bitclout.Worker;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Threading;

namespace Bitclout
{
    public class ChromeWorker
    {
        /// <summary>
        /// Веб драйвер
        /// </summary>
        public IWebDriver RegChromeDriver { get; set; }
        /// <summary>
        /// Веб драйвер
        /// </summary>
        public IWebDriver BitcloutChromeDriver { get; set; }

        public ChromeWorker()
        {
        }

        /// <summary>
        /// Инициализация хрома
        /// </summary>
        public bool InitializeRegChromeDriver()
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info("Инициализация драйвера для регистрации ->");
                ChromeOptions options = new ChromeOptions();
                if (MainWindowViewModel.settings.CurrentProxy == null || MainWindowViewModel.settings.CurrentProxy.AccountsRegistred > 1)//если нету или активаций больше двух
                {
                    if (MainWindowViewModel.settings.CurrentProxy != null && MainWindowViewModel.settings.CurrentProxy.AccountsRegistred > 1)//Удаляем прокси с сайта
                    {
                        if (ProxyWorker.DeleteProxy(MainWindowViewModel.settings.CurrentProxy))//если удалили занулляем
                            MainWindowViewModel.settings.CurrentProxy = null;
                    }
                    MainWindowViewModel.settings.CurrentProxy = ProxyWorker.GetProxy();//получаем новый прокси
                    if (MainWindowViewModel.settings.CurrentProxy == null) return false;
                    MainWindowViewModel.settings.SaveSettings();
                }

                options.AddArguments("--proxy-server=http://" + MainWindowViewModel.settings.CurrentProxy.GetAddress());
                //options.AddExtension("proxy.zip");
                options.AddArguments("--incognito");
                options.AddArgument("--user-data-dir=" + Directory.GetCurrentDirectory() + @"\Chrome");
                options.BinaryLocation = MainWindowViewModel.settings.ChromePath;
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                RegChromeDriver = new ChromeDriver(service, options);
                NLog.LogManager.GetCurrentClassLogger().Info("Драйвер регистрации успешно инициализирован");
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, "Не удалось инициализировать драйвер регистрации");
                throw new FailInitializeRegChromeDriverException(ex.Message);
            }
        }

        /// <summary>
        /// Инициализация главного драйвера
        /// </summary>
        public bool InitializeBitcloutChromeDriver()
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info("Инициализация драйвера для Bitclout ->");
                ChromeOptions options = new ChromeOptions();
                options.AddArguments("--incognito");
                options.AddArgument("--user-data-dir=" + Directory.GetCurrentDirectory() + @"\MainChrome");
                options.BinaryLocation = MainWindowViewModel.settings.ChromePath;
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                BitcloutChromeDriver = new ChromeDriver(service, options);
                NLog.LogManager.GetCurrentClassLogger().Info("Драйвер Bitclout Успешно инициализирован");
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, "Не удалось инициализировать драйвер Bitclout");
                throw new FailedInitializeBitcloutChromeDriver(ex.Message);
            }
        }

        public void EndRegistration()
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info("Очистка куки ->");
                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                RegChromeDriver.Manage().Cookies.DeleteAllCookies();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
            }
            finally
            {
                RegChromeDriver.Quit();
                NLog.LogManager.GetCurrentClassLogger().Info("Драйвер регистрации закрыт, количество использований прокси увеличено на 1");
            }
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="user">Данные пользователя для регистрации</param>
        public UserInfo RegisterNewBitсlout(UserRegistrationInfo user)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Регистрация нового пользователя {user.Name} ->");
            UserInfo userInfo = new UserInfo();
            userInfo.Name = user.Name;
            userInfo.Description = user.Description;

            PhoneNumber pn = null;
            try
            {

                while (pn == null)//Получаем номер, пока не получим
                {
                    pn = PhoneWorker.GetPhoneNumber(ServiceCodes.lt);
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                }

                if (!InitializeRegChromeDriver())
                    throw new BadProxyException("Не удалось получить прокси");

                NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу регистрации");
                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/sign-up");//Страница реги
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                userInfo.BitcloutSeedPhrase = RegChromeDriver.FindElement(By.XPath("//div[@class='p-15px ng-star-inserted']")).Text;//Получаем фразу
                NLog.LogManager.GetCurrentClassLogger().Info($"Получаем фразу-логин {userInfo.BitcloutSeedPhrase}");

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ng-star-inserted']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фразу-логин");
                RegChromeDriver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(userInfo.BitcloutSeedPhrase);//Вставляем фразу
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-10px ng-star-inserted']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем на выбор кода страны");
                RegChromeDriver.FindElement(By.XPath("//div[@class='iti__selected-flag dropdown-toggle']")).Click();//кликаем на выбор кода страны
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем Россию");
                RegChromeDriver.FindElement(By.Id("iti-0__item-ru")).Click();//Кликаем на россию
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим номер {pn.Number}");
                RegChromeDriver.FindElement(By.Id("phone")).SendKeys(pn.Number);//Вводим полученный номер
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем отправить код");
                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем отправить код
                PhoneWorker.MessageSend(pn);

                var errors = RegChromeDriver.FindElements(By.XPath("//div[@class='mt-10px ng-star-inserted']"));

                if (errors.Count != 0)
                {
                    foreach (var item in errors)
                    {
                        if (item.Text.Contains("This phone number is being used by another account"))
                            throw new PhoneNumberAlreadyUsedException("Телефон уже зарегистрирован");
                    }
                }

                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 4);

                for (int i = 0; i < 6; i++)//Ждем еще 30 секунд, проверяя каждые 5
                {
                    pn = PhoneWorker.GetCode(pn);
                    if (pn.Code == "")
                    {
                        Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                    }
                    else break;
                }

                if (pn.Code == "")
                {
                    throw new PhoneCodeNotSendException("Истекло время ожидания кода или код не пришел");
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим полученный код {pn.Code}");
                RegChromeDriver.FindElement(By.Name("verificationCode")).SendKeys(pn.Code);//Вводим полученный код
                PhoneWorker.NumberConformation(pn);//Подтверждаем номер
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                if (RegChromeDriver.Url != "https://bitclout.com/sign-up?stepNum=4")
                    throw new BadProxyException("Не удалось подтвердить код");

                MainWindowViewModel.settings.CurrentProxy.AccountsRegistred++;
                MainWindowViewModel.settings.SaveSettings();

                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/update-profile");//Страница профиля
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим имя {user.Name}");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-18px p-10px ng-untouched ng-pristine ng-valid']")).SendKeys(user.Name);//Вводим имя пользователя из файла
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим описание {user.Description}");
                RegChromeDriver.FindElement(By.XPath("//textarea[@class='fs-15px p-10px w-100 ng-untouched ng-pristine ng-valid']")).SendKeys(user.Description);//Вводим описание из файла
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фотку");
                RegChromeDriver.FindElement(By.Id("file")).SendKeys(UserRegistrationInfo.GeneratePhotoPath());//Отправляем фотку 
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Изменяем комиссию");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-untouched ng-pristine ng-valid']")).Clear();//Очищаем ввод процента
                Thread.Sleep(2000);

                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys("99");//Ставим 0
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Копируем публичный код");
                userInfo.PublicKey = RegChromeDriver.FindElement(By.XPath("//div[@class='mt-10px d-flex align-items-center update-profile__pub-key fc-muted fs-110px']")).Text;//Копируем публичный ключ
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Пробуем сохранить профиль");
                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary btn-lg font-weight-bold fs-15px mt-5px']")).Click();//Пробем сохранить
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);


                bool created = false;
                try
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($"Пробуем найти окно с ошибкой");
                    var err = RegChromeDriver.FindElements(By.XPath("//div[@class='swal2-html-container']"));
                    if (err.Count != 0)//Если есть элемнт неудачного сохранения
                    {
                        if (err[0].Text.Contains("already exists"))
                            throw new NameAlreadyExistException("Имя занято");
                        if (err[0].Text.Contains("Your balance"))
                        {
                            SendBitclout(userInfo.PublicKey);//Переводим бабло

                            NLog.LogManager.GetCurrentClassLogger().Info($"Закрываем окно с сообщением об ошибке");
                            RegChromeDriver.FindElement(By.XPath("//button[@class='swal2-cancel btn btn-light no swal2-styled']")).Click();//Закрываем окно с сообщением
                            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                            NLog.LogManager.GetCurrentClassLogger().Info($"Сохраняем профиль");
                            RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary btn-lg font-weight-bold fs-15px mt-5px']")).Click();//Пробем сохранить еще раз
                        }
                    }
                    for (int i = 0; i < MainWindowViewModel.settings.DelayTime * 4 / 100; i++)
                    {
                        if (RegChromeDriver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px']")).Count == 1 || RegChromeDriver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px ng-star-inserted']")).Count == 1)
                        {
                            created = true;
                            break;
                        }
                        Thread.Sleep(100);
                    }
                }
                catch (Exception)
                {
                    throw new FailSendBitcloutException("Не удалось отправить Bitclout");
                }
                if (!created)
                    throw new FailedSaveProfileException("Не удалось сохранить профиль");

                userInfo.USDBuy = BuyCreatorCoins(userInfo.Name);//Покупаем коины пользователя

                if (userInfo.USDBuy == 0)
                    throw new FailPrepareToBuyCreatorCoinsException("Не удалось купить коины");

                NLog.LogManager.GetCurrentClassLogger().Info($"Обновляем страницу профиля");
                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/update-profile");//Страница реги
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Изменяем комиссию");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-untouched ng-pristine ng-valid']")).Clear();//Очищаем ввод процента
                Thread.Sleep(3000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Ставим комиссию 0");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys("0");//Ставим 0
                Thread.Sleep(3000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Пробуем сохранить профиль");
                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary btn-lg font-weight-bold fs-15px mt-5px']")).Click();//Пробем сохранить

                bool buy = false;
                for (int i = 0; i < MainWindowViewModel.settings.DelayTime * 3 / 100; i++)
                {
                    if (RegChromeDriver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px']")).Count == 1 || RegChromeDriver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px ng-star-inserted']")).Count == 1)
                    {
                        buy = ConfirmBuy();
                        break;
                    }
                    Thread.Sleep(100);
                }

                if (!buy) throw new FailConfirmBuyException("Не удалось Купить коины");

                return userInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (pn != null || pn.Number == pn.Code)
                    PhoneWorker.DeclinePhone(pn);
                EndRegistration();
            }
        }

        public bool LoginToBitclout()
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Вход в аккаунт Bitclout ->");
            BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/log-in");
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фразу");
            BitcloutChromeDriver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(MainWindowViewModel.settings.BitcloutSeedPhrase);
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);


            BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            if (BitcloutChromeDriver.Url == "https://bitclout.com/browse")
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Вход выполнен успешно");
                return true;
            }
            else throw new FailToStartBitcloutChromeDriverException("Не авторизоваться в Bitclout");
        }

        public bool SendBitclout(string publicKey)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем Bitclout ->");

            BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/send-bitclout");
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим публичный ключ");

            BitcloutChromeDriver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px mt-5px ng-untouched ng-pristine ng-valid']")).SendKeys(publicKey);
            Thread.Sleep(2000);

            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим сумму");
            BitcloutChromeDriver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px ng-untouched ng-pristine ng-valid']")).SendKeys(".00005");
            Thread.Sleep(2000);

            NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");

            try
            {
                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px ng-star-inserted']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            }
            catch (Exception)
            {
                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            }

            NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
            BitcloutChromeDriver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime * 2);

            NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
            BitcloutChromeDriver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            return true;
        }

        public int BuyCreatorCoins(string userName)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Покупаем Creator Coins {userName} ->");

                BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName + @"/buy");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName + @"/buy");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 2);

                var send = new Random().Next(MainWindowViewModel.settings.MinUSD, MainWindowViewModel.settings.MaxUSD);
                NLog.LogManager.GetCurrentClassLogger().Info($"Сгенерировали число {send}");
                BitcloutChromeDriver.FindElement(By.XPath("//input[@class='form-control w-50 fs-15px text-right d-inline-block ng-untouched ng-pristine ng-invalid']")).SendKeys(send.ToString());

                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Покупаем");
                BitcloutChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                return send;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка при покупке Creator Coins");
                return 0;
            }
        }

        public bool ConfirmBuy()
        {
            try
            {
                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                bool buy = false;
                for (int i = 0; i < MainWindowViewModel.settings.DelayTime * 3 / 100; i++)
                {
                    var btn = BitcloutChromeDriver.FindElements(By.XPath("//button[@class='w-100 btn btn-primary fs-18px']"));
                    if (btn.Count == 1)
                    {
                        buy = true;
                        break;
                    }
                    Thread.Sleep(100);
                }
                if (buy)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($"Покупка успешна");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка при покупке Creator Coins");
                return false;
            }
        }

        public bool SellCreatorCoins(string userName)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Продаем Creator Coins {userName} ->");

                BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName + @"/sell");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                BitcloutChromeDriver.FindElement(By.XPath("//a[@class='text-grey7']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                BitcloutChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                return true;
                //try
                //{
                //    var text = BitcloutChromeDriver.FindElement(By.XPath("//span[@class='ml-10px text-primary']")).Text;
                //    if (text.Contains("Sucess"))
                //    {
                //        NLog.LogManager.GetCurrentClassLogger().Info($"Успешная продажа");
                //        return true;
                //    }
                //    return false;
                //}
                //catch (Exception ex)
                //{
                //    NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в продаже");
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в продаже");
                return false;
            }
        }
    }
}
