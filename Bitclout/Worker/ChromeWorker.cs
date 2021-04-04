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
        IWebDriver RegChromeDriver { get; set; }
        /// <summary>
        /// Веб драйвер
        /// </summary>
        IWebDriver MainChromeDriver { get; set; }
        /// <summary>
        /// Веб драйвер
        /// </summary>
        IWebDriver TwitterChromeDriver { get; set; }

        public ChromeWorker()
        {
        }

        /// <summary>
        /// Инициализация хрома
        /// </summary>
        public void InitializeRegChromeDriver()
        {
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
                //ProxyWorker.UpdateProxyExtension();//Обновляем расширение для прокси
            }

            options.AddArguments("--proxy-server=http://" + MainWindowViewModel.settings.CurrentProxy.GetAddress());
            //options.AddExtension("proxy.zip");
            options.AddArguments("--incognito");
            options.AddArgument("--user-data-dir=" + Directory.GetCurrentDirectory() + @"\Chrome");
            options.BinaryLocation = MainWindowViewModel.settings.ChromePath;
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            RegChromeDriver = new ChromeDriver(service, options);
        }

        /// <summary>
        /// Инициализация главного драйвера
        /// </summary>
        public void InitializeMainChromeDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--incognito");
            options.AddArgument("--user-data-dir=" + Directory.GetCurrentDirectory() + @"\MainChrome");
            options.BinaryLocation = MainWindowViewModel.settings.ChromePath;
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            MainChromeDriver = new ChromeDriver(service, options);
        }

        public void EndRegistration()
        {
            RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/");
            Thread.Sleep(5000);
            RegChromeDriver.Manage().Cookies.DeleteAllCookies();
            RegChromeDriver.Quit();
            MainWindowViewModel.settings.CurrentProxy.AccountsRegistred++;
            MainWindowViewModel.settings.SaveSettings();
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="user">Данные пользователя для регистрации</param>
        public UserInfo RegisterNewBitClout(UserRegistrationInfo user)
        {
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
                    Thread.Sleep(3000);
                }

                InitializeRegChromeDriver();

                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/sign-up");//Страница реги
                Thread.Sleep(5000);

                userInfo.BitcloutSeedPhrase = RegChromeDriver.FindElement(By.XPath("//div[@class='p-15px ng-star-inserted']")).Text;//Получаем фразу

                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ng-star-inserted']")).Click();//Кликаем дальше
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(userInfo.BitcloutSeedPhrase);//Вставляем фразу
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-10px ng-star-inserted']")).Click();//Кликаем дальше
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.XPath("//div[@class='iti__selected-flag dropdown-toggle']")).Click();//кликаем на выбор кода страны
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.Id("iti-0__item-ru")).Click();//Кликаем на россию
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.Id("phone")).SendKeys(pn.Number);//Вводим полученный номер
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем отправить код
                Thread.Sleep(20000);

                for (int i = 0; i < 4; i++)//Ждем еще 20 секунд, проверяя каждые 5
                {
                    pn = PhoneWorker.GetCode(pn);
                    if (pn.Code == "")
                    {
                        Thread.Sleep(5000);
                    }
                    else break;
                }

                if (pn.Code == "")
                    throw new Exception("Не удалось получить код");

                RegChromeDriver.FindElement(By.Name("verificationCode")).SendKeys(pn.Code);//Вводим полученный код
                PhoneWorker.NumberConformation(pn);//Подтверждаем номер

                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем дальше
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-outline-primary font-weight-bold fs-15px']")).Click();//Кликаем скип
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-outline-primary font-weight-bold fs-15px mt-5px mr-15px mb-5px']")).Click();//Кликаем изменить профиль
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-18px p-10px ng-untouched ng-pristine ng-valid']")).SendKeys(user.Name);//Вводим имя пользователя из файла
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.XPath("//textarea[@class='fs-15px p-10px w-100 ng-untouched ng-pristine ng-valid']")).SendKeys(user.Description);//Вводим описание из файла
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.Id("file")).SendKeys(user.PhotoPath);//Отправляем фотку 
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-untouched ng-pristine ng-valid']")).Clear();//Очищаем ввод процента
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys("0");//Ставим 0
                Thread.Sleep(5000);

                userInfo.PublicKey = RegChromeDriver.FindElement(By.XPath("//div[@class='mt-10px d-flex align-items-center update-profile__pub-key fc-muted fs-110px']")).Text;//Копируем публичный ключ
                Thread.Sleep(5000);

                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary btn-lg font-weight-bold fs-15px mt-5px']")).Click();//Пробем сохранить
                Thread.Sleep(5000);

                try
                {
                    if (RegChromeDriver.FindElement(By.XPath("//div[@class='swal2-html-container']")) != null)//Если есть элемнт неудачного сохранения
                    {
                        if (SendBitCloud(userInfo.PublicKey))//Переводим бабло
                        {
                            RegChromeDriver.FindElement(By.XPath("//button[@class='swal2-cancel btn btn-light no swal2-styled']")).Click();//Закрываем окно с сообщением
                            Thread.Sleep(5000);

                            RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary btn-lg font-weight-bold fs-15px mt-5px']")).Click();//Пробем сохранить еще раз
                            Thread.Sleep(5000);
                        }
                        else
                            throw new Exception("Не удалось отправить bitclout");
                    }
                }
                catch (Exception ex)
                {
                }


                if (MakeScreenshot(userInfo.Name))//Пробуем сделать скриншот
                    userInfo.BitcloutSreenPath = Directory.GetCurrentDirectory() + $"\\screenshots\\{userInfo.Name}.png";
                else throw new Exception("Не удалось получить скриншот");

                throw new Exception();
                userInfo.USDBuy = BuyCreatorCoins(userInfo.Name);//Покупаем коины пользователя
                if (userInfo.USDBuy != 0)
                {
                    if (ChangeTwitterAccount(user))//Меняем данные твиттера
                    {
                        if (SendTweet())//Шлем твит
                        {
                            if (WaitUntilPriceChanged())//Ждем какое то время
                            {
                                userInfo.USDSell = SellCreatorCoins(userInfo.Name);//Продаем коины
                                if (userInfo.USDSell != 0)
                                {
                                    //успешное выполнение
                                }
                                else
                                    throw new Exception("Не удалось продать коины");
                            }
                            else
                                throw new Exception("За время ожидания цена не увеличилась");
                        }
                        else
                            throw new Exception("Не удалось отправить твит");
                    }
                    else
                        throw new Exception("Не удалось поменять твиттер");
                }
                else
                    throw new Exception("Не удалось купить коины");

                EndRegistration();
                return userInfo;
            }
            catch (Exception ex)
            {
                EndRegistration();
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
                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName);
                Thread.Sleep(5000);
                ((ITakesScreenshot)RegChromeDriver).GetScreenshot().SaveAsFile(Directory.GetCurrentDirectory() + $"\\screenshots\\{userName}.png", ScreenshotImageFormat.Png);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool StartMainChromeDriver()
        {
            try
            {
                InitializeMainChromeDriver();
                MainChromeDriver.Navigate().GoToUrl($"https://bitclout.com/log-in");
                Thread.Sleep(5000);

                MainChromeDriver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(MainWindowViewModel.settings.BitcloutSeedPhrase);
                Thread.Sleep(5000);

                MainChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();
                Thread.Sleep(5000);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SendBitCloud(string publicKey)
        {
            try
            {
                MainChromeDriver.Navigate().GoToUrl($"https://bitclout.com/send-bitclout");
                Thread.Sleep(5000);

                MainChromeDriver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px mt-5px ng-untouched ng-pristine ng-valid']")).SendKeys(publicKey);
                Thread.Sleep(5000);

                MainChromeDriver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px ng-untouched ng-pristine ng-valid']")).SendKeys(".00005");
                Thread.Sleep(5000);

                MainChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px ng-star-inserted']")).Click();
                Thread.Sleep(10000);

                MainChromeDriver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
                Thread.Sleep(10000);

                MainChromeDriver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
                Thread.Sleep(10000);

                return true;
            }
            catch (Exception ex)
            {
                //логировние
                return false;
            }
        }

        public int BuyCreatorCoins(string userName)
        {
            try
            {
                MainChromeDriver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName + @"/buy");
                Thread.Sleep(5000);

                var send = new Random().Next(3, 7);
                MainChromeDriver.FindElement(By.XPath("//input[@class='form-control w-50 fs-15px text-right d-inline-block ng-untouched ng-pristine ng-invalid']")).SendKeys(send.ToString());
                Thread.Sleep(5000);

                MainChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                Thread.Sleep(5000);

                MainChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
                Thread.Sleep(10000);

                if (MainChromeDriver.FindElement(By.XPath("//span[@class='ml-10px text-primary']")).Text.Contains("Success!"))
                    return send;
                else throw new Exception("Не успешная покупка");
            }
            catch (Exception ex)
            {
                //logs
                return 0;
            }
        }

        public int SellCreatorCoins(string userName)
        {
            return 0;
        }

        public bool WaitUntilPriceChanged()
        {
            return true;
        }

        public bool ChangeTwitterAccount(UserRegistrationInfo user)
        {
            return true;
        }

        public bool SendTweet()
        {
            return true;
        }
    }
}
