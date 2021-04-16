using Bitclout.Exceptions;
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
                return true;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(ex, "Не удалось инициализировать драйвер Bitclout");
                throw new FailedInitializeBitcloutChromeDriver(ex.Message);
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
                    if (first.Item2 > MainWindowViewModel.settings.SellAmount)
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
