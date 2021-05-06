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
        public IWebDriver DiamondChromeDriver { get; set; }
        /// <summary>
        /// Веб драйвер
        /// </summary>
        public IWebDriver PostChromeDriver { get; set; }

        public ChromeWorker()
        {
        }

        /// <summary>
        /// Инициализация главного драйвера
        /// </summary>
        public bool InitializeChromeDriver(ChromeDriver driver, string folderPath)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info("Инициализация драйвера для Bitclout ->");
                ChromeOptions options = new ChromeOptions();
                options.AddArguments("--incognito");
                options.AddArgument("--user-data-dir=" + Directory.GetCurrentDirectory() + "\\" + folderPath);
                options.BinaryLocation = MainWindowViewModel.settings.ChromePath;
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                driver = new ChromeDriver(service, options);
                NLog.LogManager.GetCurrentClassLogger().Info("Драйвер Bitclout Успешно инициализирован");
                driver.Manage().Window.Maximize();
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, "Не удалось инициализировать драйвер Bitclout");
                throw new Exception(ex.Message);
            }
        }

        public void CloseDdriver(ChromeDriver driver)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info("Очистка куки ->");
                driver.Navigate().GoToUrl($"https://bitclout.com/");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
                driver.Manage().Cookies.DeleteAllCookies();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, ex.Message);
            }
            finally
            {
                driver.Quit();
                NLog.LogManager.GetCurrentClassLogger().Info("Драйвер регистрации закрыт, количество использований прокси увеличено на 1");
            }
        }

        public bool SendDiamond(string publicKey, ChromeDriver driver)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу трансфера");
                driver.Navigate().GoToUrl($"https://bitclout.com/u/BeActive/transfer");//Страница реги
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                driver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px mt-5px ng-untouched ng-pristine ng-valid']")).SendKeys(publicKey);
                Thread.Sleep(4000);

                driver.FindElement(By.XPath("//a[@class='text-grey7']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                driver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                driver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                bool sell = false;
                for (int i = 0; i < MainWindowViewModel.settings.DelayTime * 4 / 100; i++)
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
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в отправке поста");
                return false;
            }
        }

        public bool MakePost(string post, ChromeDriver driver)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Вводим текст для поста");
                driver.FindElement(By.XPath("//textarea[@class='cdk-textarea-autosize form-control fs-18px m-5px p-0 border-0 feed-create-post__textarea ng-untouched ng-pristine ng-valid']")).SendKeys(post);
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold ml-15px fs-14px br-12px']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в отправке поста");
                return false;
            }
        }

        public bool LoginToBitclout(string phrase, ChromeDriver driver)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Переходим на страницу регистрации");
            driver.Navigate().GoToUrl($"https://bitclout.com/");//Страница реги
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            NLog.LogManager.GetCurrentClassLogger().Info($"Жмем кнопку регистрация");
            driver.FindElement(By.XPath("//a[@class='landing__log-in d-none d-md-block']")).Click();//Кликаем дальше
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            driver.SwitchTo().Window(driver.WindowHandles[1]);

            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем фразу");
            driver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(phrase);
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            NLog.LogManager.GetCurrentClassLogger().Info($"Выбираем аккаунт");
            driver.FindElement(By.XPath("//li[@class='list-group-item list-group-item-action cursor-pointer active']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            driver.SwitchTo().Window(driver.WindowHandles[0]);

            if (driver.Url.Contains("https://bitclout.com/browse"))
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Вход выполнен успешно");
                return true;
            }
            else throw new Exception("Не авторизоваться в Bitclout");
        }

        public bool SendBitclout(string publicKey, ChromeDriver driver)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"Отправляем Bitclout ->");

            driver.Navigate().GoToUrl($"https://bitclout.com/send-bitclout");
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);

            NLog.LogManager.GetCurrentClassLogger().Info($"Вводим публичный ключ");

            driver.FindElement(By.XPath("//input[@class='form-control w-100 fs-15px lh-15px mt-5px ng-untouched ng-pristine ng-valid']")).SendKeys(publicKey);
            Thread.Sleep(2000);

            try
            {
                driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-5px py-10px']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            }
            catch (Exception)
            {

            }

            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            }
            catch (Exception)
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Подтверждаем");
                driver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-15px py-10px mt-5px ng-star-inserted']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            }

            NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
            driver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime * 2);

            NLog.LogManager.GetCurrentClassLogger().Info($"Подтвержаем");
            driver.FindElement(By.XPath("//button[@class='swal2-confirm btn btn-light swal2-styled']")).Click();
            Thread.Sleep(MainWindowViewModel.settings.DelayTime);
            return true;
        }

        public string GetTopSellName(ChromeDriver driver)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Находим первую запись для продажи ->");

                driver.Navigate().GoToUrl($"https://bitclout.com/wallet");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                var allcoins = driver.FindElements(By.XPath("//div[@class='row no-gutters fc-default pt-15px pl-15px']"));

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

        public bool SellAllCreatorCoins(string userName, ChromeDriver driver)
        {
            try
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"Продаем Creator Coins {userName} ->");

                driver.Navigate().GoToUrl($"https://bitclout.com/u/" + userName + @"/sell");
                Thread.Sleep(MainWindowViewModel.settings.DelayTime * 2);

                driver.FindElement(By.XPath("//button[@class='swal2-deny swal2-styled']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                driver.FindElement(By.XPath("//a[@class='text-grey7']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                driver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold w-60']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                driver.FindElement(By.XPath("//button[@class='btn btn-primary w-100 h-100']")).Click();
                Thread.Sleep(MainWindowViewModel.settings.DelayTime);

                bool sell = false;
                for (int i = 0; i < MainWindowViewModel.settings.DelayTime * 4 / 100; i++)
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
                NLog.LogManager.GetCurrentClassLogger().Info(ex, $"Ошшибка в продаже");
                return false;
            }
        }
    }
}
