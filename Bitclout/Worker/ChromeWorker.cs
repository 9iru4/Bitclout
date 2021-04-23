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
                BitcloutChromeDriver.Manage().Window.Maximize();
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, "Не удалось инициализировать драйвер Bitclout");
                throw new Exception(ex.Message);
            }
        }

        public void EndRegistration()
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info("Очистка куки ->");
                BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                BitcloutChromeDriver.Manage().Cookies.DeleteAllCookies();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
            }
            finally
            {
                BitcloutChromeDriver.Quit();
                NLog.LogManager.GetCurrentClassLogger().Info("Драйвер регистрации закрыт, количество использований прокси увеличено на 1");
            }
        }


        public bool LoginToBitclout(string phrase)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу регистрации");
            BitcloutChromeDriver.Navigate().GoToUrl($"https://bitclout.com/");//Страница реги
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            NLog.LogManager.GetCurrentClassLogger().Info($"Жмем кнопку регистрация");
            BitcloutChromeDriver.FindElement(By.XPath("//a[@class='landing__log-in d-none d-md-block']")).Click();//Кликаем дальше
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            BitcloutChromeDriver.SwitchTo().Window(BitcloutChromeDriver.WindowHandles[1]);

            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фразу");
            BitcloutChromeDriver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(phrase);
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем аккаунт");
            BitcloutChromeDriver.FindElement(By.XPath("//div[@class='d-flex justify-content-between w-100']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            BitcloutChromeDriver.SwitchTo().Window(BitcloutChromeDriver.WindowHandles[0]);

            if (BitcloutChromeDriver.Url.Contains("https://bitclout.com/browse"))
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Вход выполнен успешно");
                return true;
            }
            else throw new Exception("Не авторизоваться в Bitclout");
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
                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-5px py-10px']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
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
                    if (first.Item2 > 0)
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

                BitcloutChromeDriver.FindElement(By.XPath("//button[@class='swal2-deny swal2-styled']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

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
