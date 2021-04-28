using Bitclout.Exceptions;
using Bitclout.Model;
using Bitclout.Worker;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public bool InitializeRegChromeDriver(bool isuseproxy)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info("Инициализация драйвера для регистрации ->");
                ChromeOptions options = new ChromeOptions();
                if (isuseproxy)
                {
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
                }
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
                if (ex.Message == "Закончилсь доступные прокси для выбранной страны")
                    if (ProxyWorker.ChangeProxyCountry())
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
                        MainWindowViewModel.settings.CurrentProxy = null;
                    }
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
            string filepath = UserRegistrationInfo.GeneratePhotoPath();
            PhoneNumber pn = null;
            try
            {
                while (pn == null)//Получаем номер, пока не получим
                {
                    pn = PhoneWorker.GetPhoneNumber(ServiceCodes.lt);
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                }

                if (!InitializeRegChromeDriver(false))
                    throw new BadProxyException("Не удалось получить прокси");

                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу регистрации");
                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/");//Страница реги
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                if (RegChromeDriver.FindElements(By.XPath("//h1[@class='inline-block md:block mr-2 md:mb-2 font-light text-60 md:text-3xl text-black-dark leading-tight']")).Count != 0)
                    throw new OutOfProxyException("Ошибка с сервером cloudfire");

                NLog.LogManager.GetCurrentClassLogger().Info($"Жмем кнопку регистрация");
                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary landing__sign-up']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                RegChromeDriver.SwitchTo().Window(RegChromeDriver.WindowHandles[1]);

                userInfo.BitcloutSeedPhrase = RegChromeDriver.FindElement(By.XPath("//div[@class='p-15px']")).Text;//Получаем фразу
                NLog.LogManager.GetCurrentClassLogger().Info($"Получаем фразу-логин {userInfo.BitcloutSeedPhrase}");

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фразу-логин");
                RegChromeDriver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(userInfo.BitcloutSeedPhrase);//Вставляем фразу
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                RegChromeDriver.SwitchTo().Window(RegChromeDriver.WindowHandles[0]);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем на выбор кода страны");
                RegChromeDriver.FindElement(By.XPath("//div[@class='iti__selected-flag dropdown-toggle']")).Click();//кликаем на выбор кода страны
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем Россию");
                RegChromeDriver.FindElement(By.Id("iti-0__item-gb")).Click();//Кликаем на россию
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

                try
                {
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                    RegChromeDriver.FindElements(By.LinkText("Resend"))[0].Click();

                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                    RegChromeDriver.FindElements(By.LinkText("Resend"))[0].Click();

                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                    RegChromeDriver.FindElements(By.LinkText("Resend"))[0].Click();

                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                }
                catch (Exception)
                {

                }

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

                //MainWindowViewModel.settings.CurrentProxy.AccountsRegistred++;
                //MainWindowViewModel.settings.SaveSettings();

                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/update-profile");//Страница профиля
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим имя {user.Name}");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-18px p-10px ng-untouched ng-pristine ng-valid']")).SendKeys(user.Name);//Вводим имя пользователя из файла
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим описание {user.Description}");
                RegChromeDriver.FindElement(By.XPath("//textarea[@class='fs-15px p-10px w-100 ng-untouched ng-pristine ng-valid']")).SendKeys(user.Description);//Вводим описание из файла
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фотку");

                RegChromeDriver.FindElement(By.Id("file")).SendKeys(filepath);//Отправляем фотку 
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
                        if (err[0].Text.Contains("Creating a profile requires BitClout"))
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
                Thread.Sleep(MainWindowViewModel.settings.SellSleep);

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
                else
                {
                    if (MainWindowViewModel.settings.SellAmount != 0)
                        SellCreatorCoins(userInfo.Name);
                }

                return userInfo;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ERR_TUNNEL_CONNECTION_FAILED"))
                    throw new BadProxyException("ERR_PROXY_CONNECTION_FAILED");
                if (ex.Message.Contains("The HTTP request to the remote WebDriver server for URL"))
                    throw new BadProxyException(ex.Message);
                throw;
            }
            finally
            {
                if (pn != null || pn.Number == pn.Code)
                    PhoneWorker.DeclinePhone(pn);
                File.Delete(filepath);
                EndRegistration();
            }
        }


        public UserInfo RegisterWithoutBuy(UserRegistrationInfo user)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Регистрация нового пользователя {user.Name} ->");
            UserInfo userInfo = new UserInfo();
            userInfo.Name = user.Name;
            userInfo.Description = user.Description;
            string filepath = UserRegistrationInfo.GeneratePhotoPath();
            PhoneNumber pn = null;
            try
            {
                while (pn == null)//Получаем номер, пока не получим
                {
                    pn = PhoneWorker.GetPhoneNumber(ServiceCodes.lt);
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                }

                if (!InitializeRegChromeDriver(false))
                    throw new BadProxyException("Не удалось получить прокси");

                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу регистрации");
                RegChromeDriver.Navigate().GoToUrl($"https://bitclout.com/");//Страница реги
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                if (RegChromeDriver.FindElements(By.XPath("//h1[@class='inline-block md:block mr-2 md:mb-2 font-light text-60 md:text-3xl text-black-dark leading-tight']")).Count != 0)
                    throw new OutOfProxyException("Ошибка с сервером cloudfire");

                NLog.LogManager.GetCurrentClassLogger().Info($"Жмем кнопку регистрация");
                RegChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary landing__sign-up']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                RegChromeDriver.SwitchTo().Window(RegChromeDriver.WindowHandles[1]);

                userInfo.BitcloutSeedPhrase = RegChromeDriver.FindElement(By.XPath("//div[@class='p-15px']")).Text;//Получаем фразу
                NLog.LogManager.GetCurrentClassLogger().Info($"Получаем фразу-логин {userInfo.BitcloutSeedPhrase}");

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фразу-логин");
                RegChromeDriver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(userInfo.BitcloutSeedPhrase);//Вставляем фразу
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем дальше");
                RegChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();//Кликаем дальше
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                RegChromeDriver.SwitchTo().Window(RegChromeDriver.WindowHandles[0]);

                NLog.LogManager.GetCurrentClassLogger().Info($"Кликаем на выбор кода страны");
                RegChromeDriver.FindElement(By.XPath("//div[@class='iti__selected-flag dropdown-toggle']")).Click();//кликаем на выбор кода страны
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем Россию");
                RegChromeDriver.FindElement(By.Id("iti-0__item-gb")).Click();//Кликаем на россию
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


                try
                {
                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                    RegChromeDriver.FindElements(By.LinkText("Resend"))[0].Click();

                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                    RegChromeDriver.FindElements(By.LinkText("Resend"))[0].Click();

                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                    RegChromeDriver.FindElements(By.LinkText("Resend"))[0].Click();

                    Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                }
                catch (Exception)
                {

                }


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

                //MainWindowViewModel.settings.CurrentProxy.AccountsRegistred++;
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
                RegChromeDriver.FindElement(By.Id("file")).SendKeys(filepath);//Отправляем фотку 
                Thread.Sleep(2000);

                NLog.LogManager.GetCurrentClassLogger().Info($"Изменяем комиссию");
                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-untouched ng-pristine ng-valid']")).Clear();//Очищаем ввод процента
                Thread.Sleep(2000);

                RegChromeDriver.FindElement(By.XPath("//input[@class='form-control fs-15px lh-15px p-10px w-25 text-right ng-pristine ng-valid ng-touched']")).SendKeys(MainWindowViewModel.settings.Comission.ToString());//Ставим 0
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
                        if (err[0].Text.Contains("Creating a profile requires BitClout"))
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

                return userInfo;

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ERR_TUNNEL_CONNECTION_FAILED"))
                    throw new BadProxyException("ERR_PROXY_CONNECTION_FAILED");
                if (ex.Message.Contains("The HTTP request to the remote WebDriver server for URL"))
                    throw new BadProxyException(ex.Message);
                throw;
            }
            finally
            {
                if (pn != null || pn.Number == pn.Code)
                    PhoneWorker.DeclinePhone(pn);
                File.Delete(filepath);
                EndRegistration();
            }
        }

        public bool LoginToBitclout()
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу регистрации");
            BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/");//Страница реги
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            NLog.LogManager.GetCurrentClassLogger().Info($"Жмем кнопку регистрация");
            BitcloutChromeDriver.FindElement(By.XPath("//a[@class='landing__log-in d-none d-md-block']")).Click();//Кликаем дальше
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            BitcloutChromeDriver.SwitchTo().Window(BitcloutChromeDriver.WindowHandles[1]);

            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фразу");
            BitcloutChromeDriver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(MainWindowViewModel.settings.BitcloutSeedPhrase);
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем аккаунт");
            BitcloutChromeDriver.FindElement(By.XPath("//li[@class='list-group-item list-group-item-action cursor-pointer active']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            BitcloutChromeDriver.SwitchTo().Window(BitcloutChromeDriver.WindowHandles[0]);

            if (BitcloutChromeDriver.Url.Contains("https://bitclout.com/browse"))
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

            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим сумму");
                BitcloutChromeDriver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px ng-untouched ng-pristine ng-valid']")).SendKeys(".00005");
                Thread.Sleep(2000);
            }
            catch (Exception)
            {

            }

            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            }
            catch (Exception)
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px ng-star-inserted']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            }

            NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
            BitcloutChromeDriver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime * 2);

            NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
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

                double send = 0;
                if (MainWindowViewModel.settings.MinUSD == MainWindowViewModel.settings.MaxUSD)
                    send = MainWindowViewModel.settings.MinUSD;
                else
                {
                    send = new Random().NextDouble() * (MainWindowViewModel.settings.MaxUSD - MainWindowViewModel.settings.MinUSD) + MainWindowViewModel.settings.MinUSD;
                }

                NLog.LogManager.GetCurrentClassLogger().Info($"Сгенерировали число {send}");
                BitcloutChromeDriver.FindElement(By.XPath("//input[@class='form-control w-50 fs-15px text-right d-inline-block ng-untouched ng-pristine ng-invalid']")).SendKeys(send.ToString().Replace(',', '.'));

                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                NLog.LogManager.GetCurrentClassLogger().Info($"Покупаем");
                BitcloutChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                return 1;
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
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 2);

                BitcloutChromeDriver.FindElement(By.Name("amount")).Clear();
                Thread.Sleep(2000);

                BitcloutChromeDriver.FindElement(By.Name("amount")).SendKeys(MainWindowViewModel.settings.SellAmount.ToString().Replace(',', '.'));
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                BitcloutChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);


                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                bool sell = false;
                for (int i = 0; i < MainWindowViewModel.settings.DelayTime * 4 / 100; i++)
                {
                    var btn = BitcloutChromeDriver.FindElements(By.XPath("//button[@class='w-100 btn btn-primary fs-18px']"));
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
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в продаже");
                return false;
            }
        }

        public string GetTopSellName()
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Находим первую запись для продажи ->");

                BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/wallet");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                var allcoins = BitcloutChromeDriver.FindElements(By.XPath("//div[@class='row no-gutters fc-default px-15px']"));

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

        public bool SellAllCreatorCoins(string userName)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Продаем Creator Coins {userName} ->");

                BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName + @"/sell");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 2);

                BitcloutChromeDriver.FindElement(By.XPath("//a[@class='text-grey7']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                BitcloutChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                bool sell = false;
                for (int i = 0; i < MainWindowViewModel.settings.DelayTime * 4 / 100; i++)
                {
                    var btn = BitcloutChromeDriver.FindElements(By.XPath("//button[@class='w-100 btn btn-primary fs-18px']"));
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
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в продаже");
                return false;
            }
        }
    }
}
