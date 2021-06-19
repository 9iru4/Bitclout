﻿using Bitclout.Exceptions;
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
        public ChromeDriver InitializeChromeDriver(string dirpath, Model.Proxy prx = null, bool isIncognito = false, bool isRandomUserAgent = false)
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
            if (isRandomUserAgent)
                options.AddArgument($"--user-agent=Mozilla / 5.0 (Macintosh; Intel Mac OS X 10_15_1) AppleWebKit / 537.36 (KHTML, как Gecko) Chrome / 80.0." + new Random().Next(1000, 9999).ToString() + ".161 Safari / 537.36"); ;//Установка useragent
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

            if (!MainWindowViewModel.settings.incognito)
            {
                driver.Navigate().GoToUrl($"https://bitclout.com/");
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));
                driver.Manage().Cookies.DeleteAllCookies();
                ClearChromeData(driver);
            }
            driver.Quit();

            NLog.LogManager.GetCurrentClassLogger().Info("Драйвер успешно закрыт");
        }

        public UserInfo RegNewBitclout(UserRegistrationInfo user, IWebDriver driver, bool second = false)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Регистрация нового пользователя {user.Name} ->");

            UserInfo userInfo = new UserInfo();
            userInfo.Name = user.Name;
            userInfo.Description = user.Description;

            NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу регистрации");
            driver.Navigate().GoToUrl($"https://bitclout.com/");//Страница реги
            Thread.Sleep(7000);

            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Жмем кнопку регистрация");
                driver.FindElement(By.XPath("//a[@class='btn btn-primary landing__sign-up']")).Click();//Кликаем дальше
                Thread.Sleep(7000);

                driver.SwitchTo().Window(driver.WindowHandles[1]);
            }
            catch (Exception)
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем аккаунт");
                driver.FindElement(By.XPath("//div[@class='d-flex align-items-center justify-content-between pl-10px pr-10px change-account-selector__hover']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

                NLog.LogManager.GetCurrentClassLogger().Info($"Жмем добавить новый");
                driver.FindElement(By.XPath("//div[@class='pl-10px pr-10px pt-10px font-weight-bold change-account-selector__hover']")).Click();//Кликаем дальше
                Thread.Sleep(7000);

                driver.SwitchTo().Window(driver.WindowHandles[1]);
            }

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем с помощью сидфразы");
            driver.FindElement(By.XPath("//button[@class='btn btn-default sign-up-seed']")).Click();//Кликаем дальше
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            userInfo.BitcloutSeedPhrase = driver.FindElement(By.XPath("//div[@class='p-15px border-bottom border-color-grey']")).Text;//Получаем фразу
            NLog.LogManager.GetCurrentClassLogger().Info($"Получаем фразу-логин {userInfo.BitcloutSeedPhrase}");

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
            driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px sign-up-btn']")).Click();//Кликаем дальше
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фразу-логин");
            driver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(userInfo.BitcloutSeedPhrase);//Вставляем фразу
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
            driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-10px sign-up-btn']")).Click();//Кликаем дальше
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            driver.SwitchTo().Window(driver.WindowHandles[0]);

            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем хоум");
            driver.FindElements(By.XPath("//a[@class='cursor-pointer fs-15px text-grey5']")).Where(x => x.Text.Contains("Home")).First().Click();//Кликаем дальше
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));
            if (!second)
                Thread.Sleep(40000 + new Random().Next(2000, 5000));

            NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу обновления профайла");
            driver.FindElements(By.XPath("//a[@class='cursor-pointer fs-15px text-grey5']")).Where(x => x.Text.Contains("Update Profile")).First().Click();//Кликаем дальше
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Getstartedbitclout");
            driver.FindElement(By.XPath("//a[@class='mt-15px btn btn-primary br-12px']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500)); // Кликаем дальше

            while (MainWindowViewModel.pn == null)//Получаем номер, пока не получим
            {
                MainWindowViewModel.pn = PhoneWorker.GetPhoneNumber(ServiceCodes.lt);
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));
            }


            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим номер {MainWindowViewModel.pn.Number}");
            driver.FindElement(By.Id("phone")).SendKeys("+");
            Thread.Sleep(300);
            if (MainWindowViewModel.settings.SMSCountry.Name == "Ru")
            {
                driver.FindElement(By.Id("phone")).SendKeys("7");
                Thread.Sleep(300);
            }
            for (int i = 0; i < MainWindowViewModel.pn.Number.Length; i++)
            {
                driver.FindElement(By.Id("phone")).SendKeys(MainWindowViewModel.pn.Number[i].ToString());
                Thread.Sleep(300 + new Random().Next(50, 150));
            }
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем отправить код");
            driver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем отправить код
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 3);

            var errors = driver.FindElements(By.XPath("//div[@class='mt-10px ng-star-inserted']"));

            if (errors.Count != 0)
            {
                foreach (var item in errors)
                {
                    if (item.Text.Contains("This phone number is being used"))
                        throw new PhoneNumberAlreadyUsedException("Телефон уже зарегистрирован");
                }
            }

            if (driver.FindElements(By.Name("verificationCode")).Count == 0)
            {

                try
                {
                    bool isok = false;
                    bool isblc = false;
                    for (int i = 0; i < 10; i++)
                    {
                        try
                        {
                            driver.SwitchTo().DefaultContent();
                            driver.SwitchTo().Frame(1);
                            try
                            {
                                var blc = driver.FindElement(By.XPath("//div[@class='captcha__human__title']")).Text;//Поиск капчасолвера и текста на нем
                                if (blc.Contains("заблоки"))
                                {
                                    isblc = true;
                                    break;
                                }
                            }
                            catch
                            {
                            }
                            driver.FindElement(By.XPath("//div[@class='captcha-solver']")).Click();//Кликаем на капчесолвер решить
                            isok = true;
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(3000);
                        }
                    }
                    if (isblc)
                        throw new BadProxyException("вы были заблокированы");
                    if (!isok)
                        throw new BadProxyException("Не прожимается некст");


                    Thread.Sleep(50000);//Время на решение капчи
                    int br = 0;
                    for (int i = 0; i < 30; i++)
                    {
                        try
                        {
                            try
                            {
                                driver.SwitchTo().DefaultContent();
                                driver.SwitchTo().Frame(1);
                                var txt = driver.FindElement(By.XPath("//div[@class='captcha-solver']")).Text;//Поиск капчасолвера и текста на нем
                                if (txt.Contains("решена"))
                                    br++;
                                if (txt.Contains("API"))
                                    throw new BadProxyException("Капча решена, но не решена");
                                if (txt.Contains("ERROR"))
                                    throw new BadProxyException("Капча решена, но не решена");
                            }
                            catch (BadProxyException ex)
                            {
                                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка в капчасолвере");
                                throw ex;
                            }
                            catch (Exception ex)
                            {
                                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка в поиске капчасолвере");
                            }

                            if (br == 2)
                                throw new BadProxyException("Капча решена, но не решена");
                            driver.SwitchTo().DefaultContent();
                            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));
                            if (driver.FindElements(By.Id("phone")).Count != 0)
                            {
                                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим номер {MainWindowViewModel.pn.Number}");
                                driver.FindElement(By.Id("phone")).SendKeys("+");
                                Thread.Sleep(300);
                                if (MainWindowViewModel.settings.SMSCountry.Name == "Ru")
                                {
                                    driver.FindElement(By.Id("phone")).SendKeys("7");
                                    Thread.Sleep(300);
                                }
                                for (int k = 0; k < MainWindowViewModel.pn.Number.Length; k++)
                                {
                                    driver.FindElement(By.Id("phone")).SendKeys(MainWindowViewModel.pn.Number[k].ToString());
                                    Thread.Sleep(300 + new Random().Next(50, 150));
                                }
                                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));
                                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем отправить код");
                                driver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем отправить код
                                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 3);
                                if (driver.FindElements(By.Name("verificationCode")).Count == 0)
                                    throw new BadProxyException("Не прожимается некст после капчи");

                                break;
                            }
                        }
                        catch (BadProxyException ex)
                        {
                            throw ex;
                        }
                        catch
                        {
                            Thread.Sleep(5000);
                        }
                    }
                }
                catch (BadProxyException ex)
                {
                    throw ex;
                }
                //NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу обновления профайла");
                //driver.Navigate().GoToUrl($"https://bitclout.com/update-profile");//Страница реги
                //Thread.Sleep(7000);

                //NLog.LogManager.GetCurrentClassLogger().Info($"Getstartedbitclout");
                //driver.FindElement(By.XPath("//a[@class='mt-15px btn btn-primary br-12px']")).Click();
                //Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500)); // Кликаем дальше

                //NLog.LogManager.GetCurrentClassLogger().Info($"Вводим номер {MainWindowViewModel.pn.Number}");
                //driver.FindElement(By.Id("phone")).SendKeys("+7" + MainWindowViewModel.pn.Number);//Вводим полученный номер
                //Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));
                //NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем отправить код");
                //driver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем отправить код
                //Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);
            }

            PhoneWorker.MessageSend(MainWindowViewModel.pn);

            Thread.Sleep(30000);

            for (int i = 0; i < 20; i++)//Ждем еще проверяя каждые 3 секунды
            {
                MainWindowViewModel.pn = PhoneWorker.GetCode(MainWindowViewModel.pn);
                if (MainWindowViewModel.pn.Code == "")
                    Thread.Sleep(2000);
                else break;
            }
            if (MainWindowViewModel.pn.Code == "")
                throw new PhoneCodeNotSendException("Не прислали код");

            PhoneWorker.NumberConformation(MainWindowViewModel.pn);//Подтверждаем номер

            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим полученный код {MainWindowViewModel.pn.Code}");
            driver.FindElement(By.Name("verificationCode")).SendKeys(MainWindowViewModel.pn.Code);//Вводим полученный код

            MainWindowViewModel.pn = null;

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
            else
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
            driver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем дальше
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            bool regok = false;
            for (int i = 0; i < 20; i++)
            {
                if (driver.FindElements(By.XPath("//a[@class='btn btn-outline-primary font-weight-bold fs-15px mt-5px mr-15px mb-5px']")).Count != 0)
                {
                    regok = true;
                    break;
                }
                Thread.Sleep(1000);
            }

            if (!regok)
                throw new BadProxyException("Не удалось подтвердить код");

            Thread.Sleep(2000);

            return userInfo;
        }


        public UserInfo UpdateProfile(UserInfo user, IWebDriver driver)
        {
            string filepath = UserRegistrationInfo.GeneratePhotoPath();

            driver.Navigate().GoToUrl($"https://bitclout.com/update-profile");//Страница профиля
            Thread.Sleep(7000);

            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим имя {user.Name}");
            driver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-18px p-10px ng-untouched ng-pristine ng-valid']")).SendKeys(user.Name);//Вводим имя пользователя из файла
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим описание {user.Description}");
            driver.FindElement(By.XPath("//textarea[@class='fs-15px p-10px w-100 ng-untouched ng-pristine ng-valid']")).SendKeys(user.Description);//Вводим описание из файла
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фотку");

            driver.FindElement(By.Id("file")).SendKeys(filepath);//Отправляем фотку 
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Изменяем комиссию");
            driver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-untouched ng-pristine ng-valid']")).Clear();//Очищаем ввод процента
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            if (MainWindowViewModel.settings.BotWorkMode.Type == WorkType.OnlyBuyCoins || MainWindowViewModel.settings.BotWorkMode.Type == WorkType.BuyCoinsAndSellMain)
                driver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys("99");
            else
                driver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys(MainWindowViewModel.settings.Comission.ToString());
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Копируем публичный код");
            user.PublicKey = driver.FindElement(By.XPath("//div[@class='mt-10px d-flex align-items-center update-profile__pub-key fc-muted fs-110px']")).Text;//Копируем публичный ключ

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
            driver.FindElement(By.XPath("//a[@class='btn btn-primary btn-lg font-weight-bold fs-15px mt-5px']")).Click();//Пробем сохранить

            if (MainWindowViewModel.settings.BotWorkMode.Type == WorkType.MerlinAndSellReg || MainWindowViewModel.settings.BotWorkMode.Type == WorkType.OnlyMerlin || MainWindowViewModel.settings.BotWorkMode.Type == WorkType.SellMerlin)
            {
                Thread.Sleep(MainWindowViewModel.settings.MerlinDelay);

                driver.Navigate().Refresh();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));
            }
            else
            {
                Thread.Sleep(7000);

                bool created = false;

                var err = driver.FindElements(By.XPath("//div[@class='swal2-html-container']"));
                if (err.Count != 0)//Если есть элемнт неудачного сохранения
                {
                    if (err[0].Text.Contains("already exists"))
                        throw new NameAlreadyExistException("Имя занято");
                    if (err[0].Text.Contains("fee"))
                        throw new FailedSaveProfileException("Ошибка сайта при обновлении профиля");

                    for (int i = 0; i < MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 8 / 100; i++)
                    {
                        if (driver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px']")).Count == 1 || driver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px ng-star-inserted']")).Count == 1)
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

            user.IsUpdate = true;

            return user;
        }

        public UserInfo BuyCoins(UserInfo user, IWebDriver driver)
        {
            user.USDBuy = BuyCreatorCoins(user.Name, BitcloutChromeDriver);//Покупаем коины пользователя

            if (user.USDBuy == -1)
                throw new FailPrepareToBuyCreatorCoinsException("Не удалось купить коины");

            NLog.LogManager.GetCurrentClassLogger().Info($"Обновляем страницу профиля");
            driver.Navigate().GoToUrl($"https://bitclout.com/update-profile");//Страница реги
            Thread.Sleep(7000);

            NLog.LogManager.GetCurrentClassLogger().Info($"Изменяем комиссию");
            driver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-untouched ng-pristine ng-valid']")).Clear();//Очищаем ввод процента
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Ставим комиссию {MainWindowViewModel.settings.Comission}");
            driver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys(MainWindowViewModel.settings.Comission.ToString());//Ставим 0
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Пробуем сохранить профиль");
            driver.FindElement(By.XPath("//a[@class='btn btn-primary btn-lg font-weight-bold fs-15px mt-5px']")).Click();//Пробем сохранить
            Thread.Sleep(MainWindowViewModel.settings.BuyDelay);

            bool buy = false;
            for (int i = 0; i < MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 3 / 100; i++)
            {
                if (driver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px']")).Count == 1 || RegChromeDriver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px ng-star-inserted']")).Count == 1)
                {
                    buy = ConfirmBuy(BitcloutChromeDriver);
                    break;
                }
                Thread.Sleep(100);
            }

            if (!buy)
                throw new FailConfirmBuyException("Не удалось Купить коины");

            return user;
        }

        public bool SendAllBitclout(string publicKey, IWebDriver driver)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем Bitclout ->");

                driver.Navigate().GoToUrl($"https://bitclout.com/wallet");
                Thread.Sleep(3000);

                foreach (var item in driver.FindElements(By.XPath("//a[@class='cursor-pointer fs-15px text-grey5']")))
                {
                    if (item.Text.Contains("Send BitClout"))
                    {
                        item.Click();
                        break;
                    }
                }

                Thread.Sleep(7000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим публичный ключ");

                try
                {
                    driver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px mt-5px ng-untouched ng-pristine ng-valid']")).SendKeys(publicKey);
                    Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);

                    driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-5px py-10px']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);

                    try
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                        driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px']")).Click();
                        Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);
                    }
                    catch (Exception)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                        driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px ng-star-inserted']")).Click();
                        Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);
                    }
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка в отправке");
                    try
                    {
                        bool isok = false;
                        for (int i = 0; i < 10; i++)
                        {
                            try
                            {
                                driver.SwitchTo().DefaultContent();
                                driver.SwitchTo().Frame(1);
                                driver.FindElement(By.XPath("//div[@class='captcha-solver']")).Click();//Кликаем на капчесолвер решить
                                isok = true;
                                break;
                            }
                            catch
                            {
                                Thread.Sleep(3000);
                            }
                        }
                        if (!isok)
                            throw new BadProxyException("Не прожимается некст");


                        Thread.Sleep(50000);//Время на решение капчи
                        int br = 0;
                        for (int i = 0; i < 30; i++)
                        {
                            try
                            {
                                try
                                {
                                    driver.SwitchTo().DefaultContent();
                                    driver.SwitchTo().Frame(1);
                                    try
                                    {
                                        var blc = driver.FindElement(By.XPath("//div[@class='captcha__human__title']")).Text;//Поиск капчасолвера и текста на нем
                                        if (blc.Contains("заблоки"))
                                            throw new Exception("block");
                                    }
                                    catch (Exception exx)
                                    {
                                        if (exx.Message.Contains("block"))
                                            throw new BadProxyException("вы были заблокированы");
                                    }
                                    var txt = driver.FindElement(By.XPath("//div[@class='captcha-solver']")).Text;//Поиск капчасолвера и текста на нем
                                    if (txt.Contains("решена"))
                                        br++;
                                    if (txt.Contains("API"))
                                        throw new BadProxyException("Капча решена, но не решена");
                                    if (txt.Contains("ERROR"))
                                        throw new BadProxyException("Капча решена, но не решена");
                                }
                                catch (BadProxyException exx)
                                {
                                    NLog.LogManager.GetCurrentClassLogger().Info(exx, $"Ошибка в капчасолвере");
                                    throw exx;
                                }
                                catch (Exception exx)
                                {
                                    NLog.LogManager.GetCurrentClassLogger().Info(exx, $"Ошибка в поиске капчасолвере");
                                }

                                if (br == 2)
                                    throw new BadProxyException("Капча решена, но не решена");

                                var txt1 = driver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px mt-5px ng-untouched ng-pristine ng-valid']"));
                                if (txt1 != null)
                                {
                                    NLog.LogManager.GetCurrentClassLogger().Info($"Вводим публичный ключ");
                                    Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);

                                    driver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px mt-5px ng-untouched ng-pristine ng-valid']")).SendKeys(publicKey);
                                    Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);

                                    driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-5px py-10px']")).Click();
                                    Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);

                                    try
                                    {
                                        NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                                        driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px']")).Click();
                                        Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);
                                    }
                                    catch (Exception)
                                    {
                                        NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                                        driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px ng-star-inserted']")).Click();
                                        Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);
                                    }
                                    break;
                                }
                            }
                            catch (BadProxyException exx)
                            {
                                throw exx;
                            }
                            catch
                            {
                                Thread.Sleep(5000);
                            }
                        }
                    }
                    catch (BadProxyException exx)
                    {
                        throw exx;
                    }
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
                driver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
                Thread.Sleep(14000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
                driver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка в отправки битклаут");
                return false;
            }
        }

        public bool LoginToBitclout(IWebDriver driver, string phrase)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу регистрации");
            driver.Navigate().GoToUrl($"https://bitclout.com/");//Страница реги
            Thread.Sleep(7000);

            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Жмем кнопку регистрация");
                driver.FindElement(By.XPath("//a[@class='landing__log-in d-none d-sm-block']")).Click();//Кликаем дальше
                Thread.Sleep(7000);

                driver.SwitchTo().Window(driver.WindowHandles[1]);
            }
            catch (Exception)
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем аккаунт");
                driver.FindElement(By.XPath("//div[@class='change-account-selector__ellipsis-restriction cursor-pointer']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

                NLog.LogManager.GetCurrentClassLogger().Info($"Жмем добавить новый");
                driver.FindElement(By.XPath("//div[@class='pl-10px pr-10px pt-10px font-weight-bold change-account-selector__hover']")).Click();//Кликаем дальше
                Thread.Sleep(7000);

                driver.SwitchTo().Window(driver.WindowHandles[1]);

                NLog.LogManager.GetCurrentClassLogger().Info($"Нажимаем добавить новый");
                driver.FindElement(By.CssSelector("[href*='/sign-up']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));
            }

            NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем с помощью сидфразы");
            driver.FindElement(By.XPath("//button[@class='btn btn-default log-in-seed']")).Click();//Кликаем дальше
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фразу");
            driver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(phrase);
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем аккаунт");
            driver.FindElement(By.XPath("//li[@class='list-group-item list-group-item-action cursor-pointer']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            driver.SwitchTo().Window(driver.WindowHandles[0]);
            Thread.Sleep(7000);
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
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим сумму");
                driver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px ng-untouched ng-pristine ng-valid']")).SendKeys(".01005");

                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);
                try
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                    driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);
                }
                catch (Exception)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                    driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px ng-star-inserted']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
                driver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
                Thread.Sleep(15000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
                driver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 2);
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

                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

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
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

                bool buy = false;
                for (int i = 0; i < MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 3 / 100; i++)
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
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            driver.FindElement(By.Name("amount")).SendKeys(MainWindowViewModel.settings.SellAmount.ToString().Replace(',', '.'));
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            driver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));


            driver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

            bool sell = false;
            for (int i = 0; i < MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 3 / 100; i++)
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
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

                driver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

                driver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500));

                bool sell = false;
                for (int i = 0; i < MainWindowViewModel.settings.MainDelay + new Random().Next(500, 1500) * 3 / 100; i++)
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