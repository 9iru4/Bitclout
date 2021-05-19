using Bitclout.Exceptions;
using Bitclout.Model;
using Bitclout.Worker;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        public IWebDriver SellChromeDriver { get; set; }

        public ChromeWorker()
        {
        }

        /// <summary>
        /// Инициализация драйвера хрома
        /// </summary>
        public ChromeDriver InitializeChromeDriver(string dirpath, Model.Proxy prx = null, bool isIncognito = false)
        {
            NLog.LogManager.GetCurrentClassLogger().Info("Инициализация драйвера для регистрации ->");

            ChromeOptions options = new ChromeOptions();

            if (prx != null)
            {
                options.AddArguments("--proxy-server=http://" + MainWindowViewModel.settings.CurrentProxy.GetAddress());
                NLog.LogManager.GetCurrentClassLogger().Info($"Получен прокси {MainWindowViewModel.settings.CurrentProxy.GetAddress()}");
            }

            if (isIncognito)
                options.AddArgument("--incognito");
            options.AddArgument("--disable-blink-features=AutomationControlled");//отключение флага webdriver
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36");//Установка useragent
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalCapability("useAutomationExtension", false);

            options.AddArgument("--ignore-certificate-errors-spki-list");//Хз чо это, игнор ошибок сертификата
            options.AddArgument("--user-data-dir=" + Directory.GetCurrentDirectory() + dirpath);
            options.BinaryLocation = MainWindowViewModel.settings.ChromePath;
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            NLog.LogManager.GetCurrentClassLogger().Info("Драйвер успешно инициализирован");
            return new ChromeDriver(service, options);
        }
        public void ClearChromeData(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("chrome://settings/clearBrowserData");
            Thread.Sleep(2000);
            driver.FindElement(By.TagName("body")).SendKeys(Keys.Tab);
            Thread.Sleep(200);
            driver.FindElement(By.TagName("body")).SendKeys(Keys.Tab);
            Thread.Sleep(200);
            driver.FindElement(By.TagName("body")).SendKeys(Keys.Tab);
            Thread.Sleep(200);
            driver.FindElement(By.TagName("body")).SendKeys(Keys.Tab);
            Thread.Sleep(200);
            driver.FindElement(By.TagName("body")).SendKeys(Keys.Tab);
            Thread.Sleep(200);
            driver.FindElement(By.TagName("body")).SendKeys(Keys.Tab);
            Thread.Sleep(200);
            driver.FindElement(By.TagName("body")).SendKeys(Keys.Tab);
            Thread.Sleep(200);
            driver.SwitchTo().ActiveElement().SendKeys(Keys.Enter);
            Thread.Sleep(7000);
        }
        /// <summary>
        /// Очистка куки и закрытие драйвера
        /// </summary>
        public void EndRegistration(IWebDriver driver)
        {
            NLog.LogManager.GetCurrentClassLogger().Info("Очистка куки ->");

            driver.Navigate().GoToUrl($"https://bitclout.com/");
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);
            driver.Manage().Cookies.DeleteAllCookies();
            ClearChromeData(driver);
            driver.Quit();

            NLog.LogManager.GetCurrentClassLogger().Info("Драйвер успешно закрыт");
        }

        public UserInfo RegNewBitclout(UserRegistrationInfo user, PhoneNumber pn)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Регистрация нового пользователя {user.Name} ->");

            UserInfo userInfo = new UserInfo();
            userInfo.Name = user.Name;
            userInfo.Description = user.Description;
            string filepath = UserRegistrationInfo.GeneratePhotoPath();

            NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу регистрации");
            RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/");//Страница реги
            Thread.Sleep(7000);

            if (RegChromeDriver.FindElements(By.XPath("//h1[@class='inline-block md:block mr-2 md:mb-2 font-light text-60 md:text-3xl text-black-dark leading-tight']")).Count != 0)
                throw new OutOfProxyException("Ошибка с сервером cloudfire");

            NLog.LogManager.GetCurrentClassLogger().Info($"Жмем кнопку регистрация");
            RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary landing__sign-up']")).Click();//Кликаем дальше
            Thread.Sleep(7000);

            RegChromeDriver.SwitchTo().Window(RegChromeDriver.WindowHandles[1]);

            userInfo.BitcloutSeedPhrase = RegChromeDriver.FindElement(By.XPath("//div[@class='p-15px']")).Text;//Получаем фразу
            NLog.LogManager.GetCurrentClassLogger().Info($"Получаем фразу-логин {userInfo.BitcloutSeedPhrase}");

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
            RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();//Кликаем дальше
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фразу-логин");
            RegChromeDriver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(userInfo.BitcloutSeedPhrase);//Вставляем фразу
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
            RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем дальше
            Thread.Sleep(7000);

            RegChromeDriver.SwitchTo().Window(RegChromeDriver.WindowHandles[0]);

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем на выбор кода страны");
            RegChromeDriver.FindElement(By.XPath("//div[@class='iti__selected-flag dropdown-toggle']")).Click();//кликаем на выбор кода страны
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем Россию");
            RegChromeDriver.FindElement(By.Id(MainWindowViewModel.settings.SMSCountry.SMSHTMLCode)).Click();//Кликаем на россию
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим номер {pn.Number}");
            RegChromeDriver.FindElement(By.Id("phone")).SendKeys(pn.Number);//Вводим полученный номер

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем отправить код");
            RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем отправить код

            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            var errors = RegChromeDriver.FindElements(By.XPath("//div[@class='mt-10px ng-star-inserted']"));

            if (errors.Count != 0)
            {
                foreach (var item in errors)
                {
                    if (item.Text.Contains("This phone number is being used"))
                        throw new PhoneNumberAlreadyUsedException("Телефон уже зарегистрирован");
                }
            }
            //Продажа в инкогнито и добавить ошибку некст номер
            if (RegChromeDriver.FindElements(By.XPath("//div[@class='mt-15px mb-15px fs-24px font-weight-bold']")).Count == 0)
            {
                Thread.Sleep(15000);

                try
                {
                    try
                    {
                        RegChromeDriver.SwitchTo().Frame(1);
                        RegChromeDriver.FindElement(By.XPath("//div[@class='captcha-solver']")).Click();
                        RegChromeDriver.SwitchTo().Frame(0);
                    }
                    catch
                    {
                        throw new BadProxyException("Не прожимается некст");
                    }


                    Thread.Sleep(50000);
                    int br = 0;
                    for (int i = 0; i < 30; i++)
                    {
                        try
                        {
                            try
                            {
                                RegChromeDriver.SwitchTo().Frame(1);
                                var txt = RegChromeDriver.FindElement(By.XPath("//div[@class='captcha-solver-info']")).Text;
                                RegChromeDriver.SwitchTo().Frame(0);
                                if (txt.Contains("Капча решена!"))
                                    br++;
                            }
                            catch
                            {
                            }

                            if (br == 3)
                                throw new BadProxyException("Captcha sosat'");

                            var txt1 = RegChromeDriver.FindElement(By.XPath("//div[@class='mt-15px fs-24px font-weight-bold']")).Text;
                            if (txt1.Contains("Get Starter BitClout"))
                            {
                                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем на выбор кода страны");
                                RegChromeDriver.FindElement(By.XPath("//div[@class='iti__selected-flag dropdown-toggle']")).Click();//кликаем на выбор кода страны
                                Thread.Sleep(MainWindowViewModel.settings.MainDelay);

                                NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем Россию");
                                RegChromeDriver.FindElement(By.Id(MainWindowViewModel.settings.SMSCountry.SMSHTMLCode)).Click();//Кликаем на россию
                                Thread.Sleep(MainWindowViewModel.settings.MainDelay);

                                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим номер {pn.Number}");
                                RegChromeDriver.FindElement(By.Id("phone")).SendKeys(pn.Number);//Вводим полученный номер
                                Thread.Sleep(MainWindowViewModel.settings.MainDelay);
                                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем отправить код");
                                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем отправить код
                                Thread.Sleep(MainWindowViewModel.settings.MainDelay * 2);
                                break;
                            }
                        }
                        catch (BadProxyException ex)
                        {
                            throw ex;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(5000);
                        }
                    }

                    if (RegChromeDriver.FindElements(By.XPath("//div[@class='mt-15px mb-15px fs-24px font-weight-bold']")).Count == 0)
                        throw new BadProxyException("Captcha sosat'");
                }
                catch (BadProxyException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {

                }
            }

            PhoneWorker.MessageSend(pn);

            Thread.Sleep(5000);
            RegChromeDriver.FindElements(By.LinkText("Resend"))[0].Click();

            Thread.Sleep(20000);

            for (int i = 0; i < 10; i++)//Ждем еще проверяя каждые 3 секунды
            {
                pn = PhoneWorker.GetCode(pn);
                if (pn.Code == "")
                    Thread.Sleep(2000);
                else break;
            }

            if (pn.Code == "")
                throw new PhoneCodeNotSendException("Истекло время ожидания кода или код не пришел");

            PhoneWorker.NumberConformation(pn);//Подтверждаем номер

            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим полученный код {pn.Code}");
            RegChromeDriver.FindElement(By.Name("verificationCode")).SendKeys(pn.Code);//Вводим полученный код

            if (MainWindowViewModel.settings.IsSyncBots)
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 5, 0);

                    var responseString = client.GetStringAsync(MainWindowViewModel.settings.VerifySyncAddress + MainWindowViewModel.settings.BotID).Result;

                    if (!responseString.Contains("GO!"))
                        throw new BadSyncResponseException("Ошибка в синхронизации");
                }
            }

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
            RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем дальше
            Thread.Sleep(7000);

            if (RegChromeDriver.Url != "https://bitclout.com/sign-up?stepNum=4")
                throw new BadProxyException("Не удалось подтвердить код");

            RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/browse?feedTab=Global");//Страница реги
            Thread.Sleep(7000);

            if (RegChromeDriver.FindElement(By.XPath("//div[@class='d-flex align-items-center justify-content-end flex-wrap']")).Text.Contains("$0.00 USD"))
                if (!MainWindowViewModel.settings.SendBitlout)
                    throw new NoBitcloutBalanceException("Мерлин не заслал бабок");
                else
                {
                    SendBitclout(userInfo.PublicKey, BitcloutChromeDriver);
                }

            if (MainWindowViewModel.settings.BotWorkMode.Type == WorkType.OnlyMerlin || MainWindowViewModel.settings.BotWorkMode.Type == WorkType.MerlinAndSellReg)
            {
                ChromeWorker chrome = new ChromeWorker();
                chrome.EndRegistration(RegChromeDriver);

                RegChromeDriver = chrome.InitializeChromeDriver(@"\Chrome");
                RegChromeDriver.Manage().Window.Maximize();

                if (!chrome.LoginToBitclout(RegChromeDriver, userInfo.BitcloutSeedPhrase))
                    throw new FailInitializeRegChromeDriverException("Не удалось инициализировать драйвер для повторного использования");
            }

            RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/update-profile");//Страница профиля
            Thread.Sleep(7000);

            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим имя {user.Name}");
            RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-18px p-10px ng-untouched ng-pristine ng-valid']")).SendKeys(user.Name);//Вводим имя пользователя из файла
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим описание {user.Description}");
            RegChromeDriver.FindElement(By.XPath("//textarea[@class='fs-15px p-10px w-100 ng-untouched ng-pristine ng-valid']")).SendKeys(user.Description);//Вводим описание из файла
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фотку");

            RegChromeDriver.FindElement(By.Id("file")).SendKeys(filepath);//Отправляем фотку 
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            NLog.LogManager.GetCurrentClassLogger().Info($"Изменяем комиссию");
            RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-untouched ng-pristine ng-valid']")).Clear();//Очищаем ввод процента
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            if (MainWindowViewModel.settings.BotWorkMode.Type == WorkType.OnlyBuyCoins || MainWindowViewModel.settings.BotWorkMode.Type == WorkType.BuyCoinsAndSellMain)
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys("99");
            else
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys(MainWindowViewModel.settings.Comission.ToString());
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            NLog.LogManager.GetCurrentClassLogger().Info($"Копируем публичный код");
            userInfo.PublicKey = RegChromeDriver.FindElement(By.XPath("//div[@class='mt-10px d-flex align-items-center update-profile__pub-key fc-muted fs-110px']")).Text;//Копируем публичный ключ

            if (MainWindowViewModel.settings.IsSyncBots)
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 5, 0);

                    var responseString = client.GetStringAsync(MainWindowViewModel.settings.UpdateSyncAddress + MainWindowViewModel.settings.BotID).Result;

                    if (!responseString.Contains("GO!"))
                        throw new BadSyncResponseException("Ошибка в синхронизации");
                }
            }

            NLog.LogManager.GetCurrentClassLogger().Info($"Пробуем сохранить профиль");
            RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary btn-lg font-weight-bold fs-15px mt-5px']")).Click();//Пробем сохранить

            if (MainWindowViewModel.settings.BotWorkMode.Type == WorkType.MerlinAndSellReg || MainWindowViewModel.settings.BotWorkMode.Type == WorkType.OnlyMerlin)
            {
                Thread.Sleep(MainWindowViewModel.settings.MerlinDelay);

                RegChromeDriver.Navigate().Refresh();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay);
            }
            else
            {
                Thread.Sleep(7000);

                bool created = false;

                var err = RegChromeDriver.FindElements(By.XPath("//div[@class='swal2-html-container']"));
                if (err.Count != 0)//Если есть элемнт неудачного сохранения
                {
                    if (err[0].Text.Contains("already exists"))
                        throw new NameAlreadyExistException("Имя занято");
                    if (err[0].Text.Contains("fee"))
                        throw new FailedSaveProfileException("Ошибка сайта при обновлении профиля");

                    for (int i = 0; i < MainWindowViewModel.settings.MainDelay * 8 / 100; i++)
                    {
                        if (RegChromeDriver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px']")).Count == 1 || RegChromeDriver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px ng-star-inserted']")).Count == 1)
                        {
                            created = true;
                            break;
                        }
                        Thread.Sleep(100);
                    }
                }
                if (!created)
                    throw new FailedSaveProfileException("Не удалось сохранить профиль");
            }

            if (MainWindowViewModel.settings.BotWorkMode.Type == WorkType.OnlyBuyCoins || MainWindowViewModel.settings.BotWorkMode.Type == WorkType.BuyCoinsAndSellMain)
            {
                userInfo.USDBuy = BuyCreatorCoins(userInfo.Name, BitcloutChromeDriver);//Покупаем коины пользователя

                if (userInfo.USDBuy == -1)
                    throw new FailPrepareToBuyCreatorCoinsException("Не удалось купить коины");

                NLog.LogManager.GetCurrentClassLogger().Info($"Обновляем страницу профиля");
                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/update-profile");//Страница реги
                Thread.Sleep(7000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Изменяем комиссию");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-untouched ng-pristine ng-valid']")).Clear();//Очищаем ввод процента
                Thread.Sleep(MainWindowViewModel.settings.MainDelay);

                NLog.LogManager.GetCurrentClassLogger().Info($"Ставим комиссию {MainWindowViewModel.settings.Comission}");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys(MainWindowViewModel.settings.Comission.ToString());//Ставим 0
                Thread.Sleep(MainWindowViewModel.settings.MainDelay);

                NLog.LogManager.GetCurrentClassLogger().Info($"Пробуем сохранить профиль");
                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary btn-lg font-weight-bold fs-15px mt-5px']")).Click();//Пробем сохранить
                Thread.Sleep(MainWindowViewModel.settings.BuyDelay);

                bool buy = false;
                for (int i = 0; i < MainWindowViewModel.settings.MainDelay * 3 / 100; i++)
                {
                    if (RegChromeDriver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px']")).Count == 1 || RegChromeDriver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px ng-star-inserted']")).Count == 1)
                    {
                        buy = ConfirmBuy(BitcloutChromeDriver);
                        break;
                    }
                    Thread.Sleep(100);
                }

                if (!buy) throw new FailConfirmBuyException("Не удалось Купить коины");
                else
                    if (MainWindowViewModel.settings.SellAmount != 0)
                    if (!SellCreatorCoins(userInfo.Name, BitcloutChromeDriver))
                        throw new FailedSellCreatorCoinsException("Не удалось продать CreatorCoins");
            }

            return userInfo;
        }

        public void SendAllBitclout(string publicKey, IWebDriver driver)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем Bitclout ->");

            driver.Navigate().GoToUrl($"https://bitclout.com/send-bitclout");
            Thread.Sleep(7000);

            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим публичный ключ");

            driver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px mt-5px ng-untouched ng-pristine ng-valid']")).SendKeys(publicKey);
            Thread.Sleep(MainWindowViewModel.settings.MainDelay * 2);

            driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-5px py-10px']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay * 2);

            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay * 2);
            }
            catch (Exception)
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px ng-star-inserted']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay * 2);
            }

            NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
            driver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
            Thread.Sleep(14000);

            NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
            driver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay * 2);
        }

        public bool LoginToBitclout(IWebDriver driver, string phrase)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу регистрации");
            driver.Navigate().GoToUrl($"https://bitclout.com/");//Страница реги
            Thread.Sleep(7000);

            NLog.LogManager.GetCurrentClassLogger().Info($"Жмем кнопку регистрация");
            driver.FindElement(By.XPath("//a[@class='landing__log-in d-none d-md-block']")).Click();//Кликаем дальше
            Thread.Sleep(7000);

            driver.SwitchTo().Window(driver.WindowHandles[1]);

            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фразу");
            driver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(phrase);
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем аккаунт");
            driver.FindElement(By.XPath("//li[@class='list-group-item list-group-item-action cursor-pointer active']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();
            Thread.Sleep(7000);

            driver.SwitchTo().Window(driver.WindowHandles[0]);

            if (driver.Url.Contains("https://bitclout.com/browse"))
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Вход выполнен успешно");
                return true;
            }
            else throw new FailToStartBitcloutChromeDriverException("Не авторизоваться в Bitclout");
        }

        public void SendBitclout(string publicKey, IWebDriver driver)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем Bitclout ->");

                driver.Navigate().GoToUrl($"https://bitclout.com/send-bitclout");
                Thread.Sleep(7000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим публичный ключ");

                driver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px mt-5px ng-untouched ng-pristine ng-valid']")).SendKeys(publicKey);
                Thread.Sleep(MainWindowViewModel.settings.MainDelay * 2);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим сумму");
                driver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px ng-untouched ng-pristine ng-valid']")).SendKeys(".00005");

                Thread.Sleep(MainWindowViewModel.settings.MainDelay * 2);
                try
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                    driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.MainDelay * 2);
                }
                catch (Exception)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                    driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px ng-star-inserted']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.MainDelay * 2);
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
                driver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
                Thread.Sleep(15000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
                driver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay * 2);
            }
            catch (Exception ex)
            {
                throw new FailSendBitcloutException(ex.Message);
            }
        }

        public double BuyCreatorCoins(string userName, IWebDriver driver)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Покупаем Creator Coins {userName} ->");

                driver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName + @"/buy");
                Thread.Sleep(7000);

                double send = 0;

                if (MainWindowViewModel.settings.MinUSD == MainWindowViewModel.settings.MaxUSD)
                    send = MainWindowViewModel.settings.MinUSD;
                else
                {
                    send = new Random().NextDouble() * (MainWindowViewModel.settings.MaxUSD - MainWindowViewModel.settings.MinUSD) + MainWindowViewModel.settings.MinUSD;
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Сгенерировали число {send}");
                driver.FindElement(By.Name("creatorCoinTrade.amount")).SendKeys(send.ToString().Replace(',', '.'));

                Thread.Sleep(MainWindowViewModel.settings.MainDelay);

                NLog.LogManager.GetCurrentClassLogger().Info($"Покупаем");
                driver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                return send;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка при покупке Creator Coins");
                return -1;
            }
        }

        public bool ConfirmBuy(IWebDriver driver)
        {
            try
            {
                driver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay);

                bool buy = false;
                for (int i = 0; i < MainWindowViewModel.settings.MainDelay * 3 / 100; i++)
                {
                    var btn = driver.FindElements(By.XPath("//button[@class='w-100 btn btn-primary fs-18px']"));
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

        public bool SellCreatorCoins(string userName, IWebDriver driver)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Продаем Creator Coins {userName} ->");

            driver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName + @"/sell");
            Thread.Sleep(7000);

            driver.FindElement(By.Name("amount")).Clear();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            driver.FindElement(By.Name("amount")).SendKeys(MainWindowViewModel.settings.SellAmount.ToString().Replace(',', '.'));
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            driver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);


            driver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay);

            bool sell = false;
            for (int i = 0; i < MainWindowViewModel.settings.MainDelay * 3 / 100; i++)
            {
                var btn = driver.FindElements(By.XPath("//button[@class='w-100 btn btn-primary fs-18px']"));
                if (btn.Count == 1)
                {
                    sell = true;
                    break;
                }
                Thread.Sleep(100);
            }

            return sell;
        }

        public string GetTopSellName(IWebDriver driver)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Находим первую запись для продажи ->");

                driver.Navigate().GoToUrl($"https://bitclout.com/wallet");
                Thread.Sleep(7000);

                var allcoins = driver.FindElements(By.XPath("//div[@class='row no-gutters fc-default px-15px']"));

                List<(string, double)> AllCoins = new List<(string, double)>();

                if (allcoins.Count != 0)
                {
                    foreach (var item in allcoins)
                    {
                        var text = item.Text.Split(Environment.NewLine.ToCharArray());
                        var name = text[0];
                        var coins = double.Parse(text[4].Remove(0, 3).Replace('.', ','));
                        AllCoins.Add((name, coins));
                    }
                    var first = AllCoins.OrderByDescending(x => x.Item2).FirstOrDefault();
                    if (first.Item2 > MainWindowViewModel.settings.SellMoreThan)
                        return first.Item1;
                }
                return "";
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в продаже");
                return "";
            }
        }

        public bool SellAllCreatorCoins(string userName, IWebDriver driver)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Продаем Creator Coins {userName} ->");

                driver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName + @"/sell");
                Thread.Sleep(7000);

                driver.FindElement(By.XPath("//a[@class='text-grey7']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay);

                driver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay);

                driver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay);

                bool sell = false;
                for (int i = 0; i < MainWindowViewModel.settings.MainDelay * 3 / 100; i++)
                {
                    var btn = driver.FindElements(By.XPath("//button[@class='w-100 btn btn-primary fs-18px']"));
                    if (btn.Count == 1)
                    {
                        sell = true;
                        break;
                    }
                    Thread.Sleep(100);
                }

                return sell;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка в продаже");
                return false;
            }
        }
    }
}
