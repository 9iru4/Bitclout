using Bitclout.Model;
using Bitclout.Worker;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bitclout
{
    public class ChromeWorker
    {
        /// <summary>
        /// Веб драйвер
        /// </summary>
        IWebDriver ChromeDriver { get; set; }


        public ChromeWorker()
        {

        }

        public void InitializeChromeDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--user-data-dir=" + Directory.GetCurrentDirectory() + @"\Chrome");
            options.AddArgument("--incognito");
            options.BinaryLocation = @"C:\Program Files\Google\Chrome\Application\Chrome.exe";
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeDriver = new ChromeDriver(service, options);
        }

        public void RegisterBitClout()
        {
            PhoneNumber pn = PhoneWorker.GetPhoneNumber(ServiceCodes.lt);
            InitializeChromeDriver();
            ChromeDriver.Navigate().GoToUrl("https://bitclout.com/sign-up");
            Thread.Sleep(5000);
            var seedPhrase = ChromeDriver.FindElement(By.XPath("//div[@class='p-15px ng-star-inserted']")).Text;

            ChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ng-star-inserted']")).Click();

            Thread.Sleep(5000);

            ChromeDriver.FindElement(By.XPath("//textarea[@class='form-control fs-15px ng-untouched ng-pristine ng-valid']")).SendKeys(seedPhrase);

            Thread.Sleep(5000);

            ChromeDriver.FindElement(By.XPath("//button[@class='btn btn-primary font-weight-bold fs-15px ml-10px ng-star-inserted']")).Click();

            Thread.Sleep(5000);

            ChromeDriver.FindElement(By.XPath("//div[@class='iti__selected-flag dropdown-toggle']")).Click();
            Thread.Sleep(5000);
            ChromeDriver.FindElement(By.Id("iti-0__item-ru")).Click();


            Thread.Sleep(5000);
            ChromeDriver.FindElement(By.Id("phone")).SendKeys(pn.Number);
            Thread.Sleep(5000);

            ChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();
            Thread.Sleep(10000);
            pn = PhoneWorker.GetCode(pn);

            ChromeDriver.FindElement(By.Name("verificationCode")).SendKeys(pn.Code);
            Thread.Sleep(5000);

            ChromeDriver.FindElement(By.XPath("//a[@class='btn btn-primary font-weight-bold fs-15px ml-10px']")).Click();
            Thread.Sleep(5000);

            ChromeDriver.FindElement(By.XPath("//button[@class='btn btn-outline-primary font-weight-bold fs-15px']")).Click();
            Thread.Sleep(5000);

            ChromeDriver.FindElement(By.XPath("//button[@class='btn btn-outline-primary font-weight-bold fs-15px mt-5px mr-15px mb-5px']")).Click();
            Thread.Sleep(5000);

        }
    }
}
