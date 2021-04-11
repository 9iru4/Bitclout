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
        /// <summary>
        /// Веб драйвер
        /// </summary>
        public IWebDriver TwitterChromeDriver { get; set; }

        bool IsTweetSend = false;

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
                    if (MainWindowViewModel.settings.CurrentProxy.AccountsRegistred > 1)//Удаляем прокси с сайта
                    {
                        if (ProxyWorker.DeleteProxy(MainWindowViewModel.settings.CurrentProxy))//если удалили занулляем
                            MainWindowViewModel.settings.CurrentProxy = null;
                    }
                    MainWindowViewModel.settings.CurrentProxy = ProxyWorker.GetProxy();//получаем новый прокси
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
                return false;
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
                return false;
            }
        }

        /// <summary>
        /// Инициализация главного драйвера
        /// </summary>
        public bool InitializeTwitterChromeDriver()
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info("Инициализация драйвера для Twitter ->");
                ChromeOptions options = new ChromeOptions();
                options.AddArguments("--incognito");
                options.AddArgument("--user-data-dir=" + Directory.GetCurrentDirectory() + @"\TwitterChrome");
                options.BinaryLocation = MainWindowViewModel.settings.ChromePath;
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                TwitterChromeDriver = new ChromeDriver(service, options);
                NLog.LogManager.GetCurrentClassLogger().Info("Драйвер Twitter Успешно инициализирован");
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, "Не удалось инициализировать драйвер Twitter");
                return false;
            }
        }

        public void EndRegistration(string TwitterName)
        {
            NLog.LogManager.GetCurrentClassLogger().Info("Очистка куки ->");
            RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/");
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            RegChromeDriver.Manage().Cookies.DeleteAllCookies();
            RegChromeDriver.Quit();

            if (IsTweetSend)
            {
                if (!DeleteTweet(TwitterName))
                    DeleteTweet(TwitterName);
                NLog.LogManager.GetCurrentClassLogger().Info("Последний твит удален");
            }

            MainWindowViewModel.settings.CurrentProxy.AccountsRegistred++;
            MainWindowViewModel.settings.SaveSettings();
            NLog.LogManager.GetCurrentClassLogger().Info("Драйвер регистрации закрыт, количество использований прокси увеличено на 1");
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
            userInfo.UserPhotoPath = user.PhotoPath;

            try
            {
                PhoneNumber pn = null;
                while (pn == null)//Получаем номер, пока не получим
                {
                    pn = PhoneWorker.GetPhoneNumber(ServiceCodes.lt);
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                }

                if (!InitializeRegChromeDriver())
                    throw new Exception("Не удалось инициализировать драйвер регистрации");

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
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-10px ng-star-inserted']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем на выбор кода страны");
                RegChromeDriver.FindElement(By.XPath("//div[@class='iti__selected-flag dropdown-toggle']")).Click();//кликаем на выбор кода страны
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем Россию");
                RegChromeDriver.FindElement(By.Id("iti-0__item-ru")).Click();//Кликаем на россию
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим номер {pn.Number}");
                RegChromeDriver.FindElement(By.Id("phone")).SendKeys(pn.Number);//Вводим полученный номер
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем отправить код");
                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем отправить код
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 5);

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
                    throw new Exception("Истекло время ожидания кода или код не пришел"); ;
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим полученный код {pn.Code}");
                RegChromeDriver.FindElement(By.Name("verificationCode")).SendKeys(pn.Code);//Вводим полученный код
                PhoneWorker.NumberConformation(pn);//Подтверждаем номер
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 2);

                try
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем скип");
                    RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-outline-primary font-weight-bold fs-15px']")).Click();//Кликаем скип
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                }
                catch (Exception ex)
                {
                    MainWindowViewModel.settings.CurrentProxy.AccountsRegistred = 2;
                    throw ex;
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем изменить профиль");
                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-outline-primary font-weight-bold fs-15px mt-5px mr-15px mb-5px']")).Click();//Кликаем изменить профиль
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим имя {user.Name}");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-18px p-10px ng-untouched ng-pristine ng-valid']")).SendKeys(user.Name);//Вводим имя пользователя из файла
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим описание {user.Description}");
                RegChromeDriver.FindElement(By.XPath("//textarea[@class='fs-15px p-10px w-100 ng-untouched ng-pristine ng-valid']")).SendKeys(user.Description);//Вводим описание из файла
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фотку");
                RegChromeDriver.FindElement(By.Id("file")).SendKeys(user.PhotoPath);//Отправляем фотку 
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Изменяем комиссию");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-untouched ng-pristine ng-valid']")).Clear();//Очищаем ввод процента
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys("99");//Ставим 0
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Копируем публичный код");
                userInfo.PublicKey = RegChromeDriver.FindElement(By.XPath("//div[@class='mt-10px d-flex align-items-center update-profile__pub-key fc-muted fs-110px']")).Text;//Копируем публичный ключ
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Пробуем сохранить профиль");
                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary btn-lg font-weight-bold fs-15px mt-5px']")).Click();//Пробем сохранить
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 2);

                try
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($"Пробуем найти окно с ошибкой");
                    if (RegChromeDriver.FindElements(By.XPath("//div[@class='swal2-html-container']")).Count != 0)//Если есть элемнт неудачного сохранения
                    {
                        throw new Exception("Не прислал мерлин");
                    }
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Произошла ошбка в отправке Bitclout на аккаунт");
                    throw new Exception("Не удалось отправить Bitclout");
                }

                userInfo.USDBuy = BuyCreatorCoins(userInfo.Name);//Покупаем коины пользователя

                NLog.LogManager.GetCurrentClassLogger().Info($"Обновляем страницу профиля");
                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/update-profile");//Страница реги
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Изменяем комиссию");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-untouched ng-pristine ng-valid']")).Clear();//Очищаем ввод процента
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Ставим комиссию 0");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys("0");//Ставим 0
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Пробуем сохранить профиль");
                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary btn-lg font-weight-bold fs-15px mt-5px']")).Click();//Пробем сохранить

                bool buy = false;
                for (int i = 0; i < MainWindowViewModel.settings.DelayTime * 2 / 1000; i++)
                {
                    if (RegChromeDriver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px']")).Count == 1 || RegChromeDriver.FindElements(By.XPath("//i[@class='far fa-check-circle fa-lg fc-blue ml-10px ng-star-inserted']")).Count == 1)
                    {
                        buy = ConfirmBuy();
                        break;
                    }
                    Thread.Sleep(1000);
                }

                if (!buy) throw new Exception("Не удалось продать");

                if (!SellCreatorCoins(userInfo.Name))
                    SellCreatorCoins(userInfo.Name);
                EndRegistration(user.Name);
                return userInfo;
            }
            catch (Exception ex)
            {
                EndRegistration(user.Name);
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка на этапе регистрации аккаунта");
                return userInfo;
            }
        }

        /// <summary>
        /// Сделать сриншот и сохранить с именем пользователя
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <returns>Сделан ли скриншот</returns>
        public bool MakeScreenshot(string userName)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Делаем скриншот ->");
                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName);
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                ((ITakesScreenshot)RegChromeDriver).GetScreenshot().SaveAsFile(Directory.GetCurrentDirectory() + $"\\screenshots\\{userName}.png", ScreenshotImageFormat.Png);
                NLog.LogManager.GetCurrentClassLogger().Info("Скриншот успешно сделан");
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Не удалось сделать скриншот");
                return false;
            }
        }

        public bool StartBitcloutChromeDriver()
        {
            try
            {
                if (!InitializeBitcloutChromeDriver())
                    throw new Exception("Не удалось инициализировать драйвер Bitclout");

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
                else throw new Exception("Не удалось инициализировать драйвер Bitclout");
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка при входе в аккаунт Bitclout");
                return false;
            }
        }


        public bool SendBitCloud(string publicKey)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем Bitclout ->");

                BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/send-bitclout");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим публичный ключ");

                BitcloutChromeDriver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px mt-5px ng-untouched ng-pristine ng-valid']")).SendKeys(publicKey);
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим сумму");
                BitcloutChromeDriver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px ng-untouched ng-pristine ng-valid']")).SendKeys(".00005");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");

                try
                {
                    BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px ng-star-inserted']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime * 4);
                }
                catch (Exception)
                {
                    BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime * 4);
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 4);

                NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 4);

                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка в отправке Bitclout");
                return false;
            }
        }

        public int BuyCreatorCoins(string userName)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Покупаем Creator Coins {userName} ->");

                BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName + @"/buy");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 2);

                var send = new Random().Next(MainWindowViewModel.settings.MinUSD, MainWindowViewModel.settings.MaxUSD);
                NLog.LogManager.GetCurrentClassLogger().Info($"Сгенерировали число {send}");
                BitcloutChromeDriver.FindElement(By.XPath("//input[@class='form-control w-50 fs-15px text-right d-inline-block ng-untouched ng-pristine ng-invalid']")).SendKeys(send.ToString());
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Покупаем");
                BitcloutChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
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
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 4);

                if (BitcloutChromeDriver.FindElement(By.XPath("//span[@class='ml-10px text-primary']")).Text.Contains("Success!"))
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($"Покупка успешна");
                    return true;
                }
                else throw new Exception("Не успешная покупка");
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
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 2);

                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 5);

                try
                {
                    var text = BitcloutChromeDriver.FindElement(By.XPath("//span[@class='ml-10px text-primary']")).Text;
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                    if (text.Contains("Sucess"))
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info($"Успешная продажа");
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в продаже");
                    return false;
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в продаже");
                return false;
            }
        }

        public bool WaitUntilPriceChanged(UserRegistrationInfo user, UserInfo userInfo)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Ждем изменения цены Creator Coins {user.Name} ->");

                BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/u/" + user.Name + @"/sell");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                int usd = 0;

                for (int i = 0; i < 24; i++)
                {
                    BitcloutChromeDriver.FindElement(By.XPath("//a[@class='text-grey7']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                    usd = int.Parse(BitcloutChromeDriver.FindElement(By.XPath("//div[@class='w-100 bg-light text-grey6']")).Text.Split('$')[1].Replace("USD", "").Split('.')[0].Trim());
                    if (usd - userInfo.USDBuy > 4)
                    {
                        userInfo.USDSell = usd;
                        NLog.LogManager.GetCurrentClassLogger().Info($"Цена изменилась и разница больше 10$ разница {usd}");
                        return true;
                    }
                }
                if (!DeleteTweet(user.Name))
                {
                    DeleteTweet(user.Name);
                }
                SendTweet(user.TweetMessage, userInfo.BitcloutSreenPath);

                for (int i = 0; i < 24; i++)
                {
                    BitcloutChromeDriver.FindElement(By.XPath("//a[@class='text-grey7']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                    usd = int.Parse(BitcloutChromeDriver.FindElement(By.XPath("//div[@class='w-100 bg-light text-grey6']")).Text.Split('$')[1].Replace("USD", "").Split('.')[0].Trim());
                    if (usd - userInfo.USDBuy > 4)
                    {
                        userInfo.USDSell = usd;
                        NLog.LogManager.GetCurrentClassLogger().Info($"Цена изменилась и разница больше 10$ разница {usd}");
                        return true;
                    }
                }
                userInfo.USDSell = usd;
                NLog.LogManager.GetCurrentClassLogger().Info($"Цена не изменилась или меньше 10$ разнца {usd}");
                return false;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка в ожидании изменения цены");

                BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/u/" + user.Name + @"/sell");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                BitcloutChromeDriver.FindElement(By.XPath("//a[@class='text-grey7']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                SellCreatorCoins(user.Name);

                return false;
            }
        }

        public bool StartTwitterDriver()
        {
            try
            {
                InitializeTwitterChromeDriver();

                NLog.LogManager.GetCurrentClassLogger().Info($"Авторизация Twitter ->");
                TwitterChromeDriver.Navigate().GoToUrl($"https://twitter.com/login");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим логин");
                TwitterChromeDriver.FindElement(By.Name("session[username_or_email]")).SendKeys(MainWindowViewModel.settings.TwitterUserName);
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим пароль");
                TwitterChromeDriver.FindElement(By.Name("session[password]")).SendKeys(MainWindowViewModel.settings.TwitterPassword);
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Входим");
                TwitterChromeDriver.FindElement(By.XPath("//div[@class='css-18t94o4 css-1dbjc4n r-urgr8i r-42olwf r-sdzlij r-1phboty r-rs99b7 r-1w2pmg r-1fz3rvf r-usiww2 r-1pl7oy7 r-snto4y r-1ny4l3l r-1dye5f7 r-o7ynqc r-6416eg r-lrvibr']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                try
                {
                    if (TwitterChromeDriver.FindElement(By.XPath("//div[@class='TopNav-title u-pullLeft']")).Text == "Подтвердите свою личность")
                    {
                        TwitterChromeDriver.FindElement(By.Name("challenge_response")).SendKeys(MainWindowViewModel.settings.TwitterEmail);
                        Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                        TwitterChromeDriver.FindElement(By.Id("email_challenge_submit")).Click();
                        Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                        NLog.LogManager.GetCurrentClassLogger().Info($"Личность успешно подтверждена");
                    }
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Подтверждение личности неудалось");
                }

                if (TwitterChromeDriver.Url == "https://twitter.com/home")
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($"Вход в твиттер успешен");
                    return true;
                }
                else throw new Exception("Не удалось войти в твиттер");
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка при авторизцаии Twitter");
                return false;
            }
        }

        public bool ChangeTwitterAccount(UserRegistrationInfo user)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Изменение данных аккаунта Twitter ->");

                TwitterChromeDriver.Navigate().GoToUrl($"https://twitter.com/settings/your_twitter_data/account");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Пробуем подтвердить аккаунт паролем");
                try
                {
                    TwitterChromeDriver.FindElement(By.Name("current_password")).SendKeys(MainWindowViewModel.settings.TwitterPassword);
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                    TwitterChromeDriver.FindElement(By.XPath("//div[@class='css-18t94o4 css-1dbjc4n r-urgr8i r-42olwf r-sdzlij r-1phboty r-rs99b7 r-1w2pmg r-1ydw1k6 r-r0h9e2 r-ero68b r-1gg2371 r-1ny4l3l r-1fneopy r-o7ynqc r-6416eg r-lrvibr']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                    NLog.LogManager.GetCurrentClassLogger().Info($"Аккаунт подтвержден");
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошибка в блоке подтверждения пароля для Twitter или его не нужно вводить");
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу смены");
                TwitterChromeDriver.Navigate().GoToUrl($"https://twitter.com/settings/screen_name");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                var name = TwitterChromeDriver.FindElement(By.Name("typedScreenName"));
                NLog.LogManager.GetCurrentClassLogger().Info($"Меняем Username для Twitter");

                name.Clear();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                name.SendKeys(user.Name);
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                TwitterChromeDriver.FindElement(By.XPath("//div[@class='css-18t94o4 css-1dbjc4n r-urgr8i r-42olwf r-sdzlij r-1phboty r-rs99b7 r-1w2pmg r-1fz3rvf r-r0h9e2 r-ero68b r-1gg2371 r-1ny4l3l r-1fneopy r-o7ynqc r-6416eg r-lrvibr']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                MainWindowViewModel.settings.TwitterUserName = user.Name;
                MainWindowViewModel.settings.SaveSettings();

                NLog.LogManager.GetCurrentClassLogger().Info($"Смена Username для Twitter успешна");
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в блоке изменения данных аккаунта Twitter");
                return false;
            }
        }

        public bool ChangeTwitterProfile(UserRegistrationInfo user)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Изменение данных профиля Twitter ->");

                TwitterChromeDriver.Navigate().GoToUrl($"https://twitter.com/settings/profile");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фотографию");
                TwitterChromeDriver.FindElements(By.XPath("//input[@class='r-8akbif r-orgf3d r-1udh08x r-u8s1d r-xjis5s r-1wyyakw']"))[1].SendKeys(user.PhotoPath);
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем фотографию");
                TwitterChromeDriver.FindElements(By.XPath("//div[@class='css-18t94o4 css-1dbjc4n r-urgr8i r-42olwf r-sdzlij r-1phboty r-rs99b7 r-1w2pmg r-15ysp7h r-gafmid r-1ny4l3l r-1fneopy r-o7ynqc r-6416eg r-lrvibr']"))[1].Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Меняем имя");
                var name = TwitterChromeDriver.FindElement(By.Name("displayName"));
                name.Clear();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                name.SendKeys(user.TwitterName);
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                try
                {
                    TwitterChromeDriver.FindElement(By.XPath("//div[@class='css-18t94o4 css-1dbjc4n r-1q3imqu r-42olwf r-sdzlij r-1phboty r-rs99b7 r-1w2pmg r-15ysp7h r-gafmid r-1ny4l3l r-1fneopy r-o7ynqc r-6416eg r-lrvibr']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                }
                catch (Exception)
                {
                    TwitterChromeDriver.FindElement(By.XPath("//div[@class='css-18t94o4 css-1dbjc4n r-urgr8i r-42olwf r-sdzlij r-1phboty r-rs99b7 r-1w2pmg r-15ysp7h r-gafmid r-1ny4l3l r-1fneopy r-o7ynqc r-6416eg r-lrvibr']")).Click();
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Данные профиля успешно изменены");
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в изменении данных профиля твиттера");
                return false;
            }
        }

        public bool SendTweet(string textMessage, string screenPath)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Отправка Tweet ->");

                TwitterChromeDriver.Navigate().GoToUrl($"https://twitter.com/home");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим сообщение");
                TwitterChromeDriver.FindElement(By.XPath("//div[@class='public-DraftStyleDefault-block public-DraftStyleDefault-ltr']")).SendKeys(textMessage);
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вставляем скриншот");
                TwitterChromeDriver.FindElement(By.XPath("//input[@class='r-8akbif r-orgf3d r-1udh08x r-u8s1d r-xjis5s r-1wyyakw']")).SendKeys(screenPath);
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                TwitterChromeDriver.FindElement(By.XPath("//div[@class='css-18t94o4 css-1dbjc4n r-urgr8i r-42olwf r-sdzlij r-1phboty r-rs99b7 r-1w2pmg r-19u6a5r r-ero68b r-1gg2371 r-1ny4l3l r-1fneopy r-o7ynqc r-6416eg r-lrvibr']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Tweet успешно отправлен");
                IsTweetSend = true;
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в отправке Tweet");
                return false;
            }
        }

        public bool DeleteTweet(string userName)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Удаление последнего Tweet ->");

                TwitterChromeDriver.Navigate().GoToUrl($"https://twitter.com/{userName}");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем 3 точки");
                TwitterChromeDriver.FindElement(By.XPath("//div[@class='css-18t94o4 css-1dbjc4n r-1777fci r-bt1l66 r-1ny4l3l r-bztko3 r-lrvibr']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Жмем удалить");

                bool deleted = false;
                try
                {
                    var elements = TwitterChromeDriver.FindElements(By.XPath("//div[@class='css-1dbjc4n r-1panhkp r-1loqt21 r-18u37iz r-1ny4l3l r-ymttw5 r-1yzf0co r-o7ynqc r-6416eg r-13qz1uu']"));
                    foreach (var item in elements)
                    {
                        if (item.Text.Contains("Удалить") || item.Text.Contains("Delete"))
                        {
                            item.Click();
                            deleted = true;
                            break;
                        }
                    }
                    if (!deleted) throw new Exception("Твит не удален.");
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                }
                catch (Exception)
                {
                    //var elements = TwitterChromeDriver.FindElements(By.XPath("//div[@class='css-1dbjc4n r-1panhkp r-1loqt21 r-18u37iz r-1ny4l3l r-ymttw5 r-1yzf0co r-o7ynqc r-6416eg r-13qz1uu']"));
                    var elements = TwitterChromeDriver.FindElements(By.XPath("//div[@class='css-1dbjc4n r-1loqt21 r-18u37iz r-1ny4l3l r-ymttw5 r-1yzf0co r-o7ynqc r-6416eg r-13qz1uu']"));
                    foreach (var item in elements)
                    {
                        if (item.Text.Contains("Удалить") || item.Text.Contains("Delete"))
                        {
                            item.Click();
                            deleted = true;
                            break;
                        }
                    }
                    if (!deleted) throw new Exception("Твит не удален.");
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");

                try
                {
                    TwitterChromeDriver.FindElement(By.XPath("//div[@class='css-18t94o4 css-1dbjc4n r-1dgebii r-42olwf r-sdzlij r-1phboty r-rs99b7 r-16y2uox r-1w2pmg r-ero68b r-1gg2371 r-1ny4l3l r-1fneopy r-o7ynqc r-6416eg r-lrvibr']")).Click();
                }
                catch (Exception)
                {
                    TwitterChromeDriver.FindElement(By.XPath("//div[@class='css-18t94o4 css-1dbjc4n r-1ucxkr8 r-42olwf r-sdzlij r-1phboty r-rs99b7 r-16y2uox r-1w2pmg r-ero68b r-1gg2371 r-1ny4l3l r-1fneopy r-o7ynqc r-6416eg r-lrvibr']")).Click();
                }

                IsTweetSend = false;
                NLog.LogManager.GetCurrentClassLogger().Info($"Tweet Успешно удален");
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в удалении Tweet");
                return false;
            }
        }
    }
}
