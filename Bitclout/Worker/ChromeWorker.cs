using Bitclout.Model;
using Bitclout.Worker;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Threading;

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

        /// <summary>
        /// Инициализация хрома
        /// </summary>
        /// <param name="isProxyUsed">Использовать прокси</param>
        public void InitializeChromeDriver(bool isProxyUsed)
        {
            ChromeOptions options = new ChromeOptions();
            if (isProxyUsed)
            {
                if (MainWindowViewModel.settings.CurrentProxy == null || MainWindowViewModel.settings.CurrentProxy.AccountsRegistred > 2)//если нету или активаций больше двух
                {
                    if (MainWindowViewModel.settings.CurrentProxy.AccountsRegistred > 2)//Удаляем прокси с сайта
                    {
                        if (ProxyWorker.DeleteProxy(MainWindowViewModel.settings.CurrentProxy))//если удалили занулляем
                            MainWindowViewModel.settings.CurrentProxy = null;
                    }
                    MainWindowViewModel.settings.CurrentProxy = ProxyWorker.GetProxy();//получаем новый прокси
                    MainWindowViewModel.settings.SaveSettings();
                    ProxyWorker.UpdateProxyExtension();//Обновляем расширение для прокси
                }

                options.AddArguments("--proxy-server=http://" + MainWindowViewModel.settings.CurrentProxy.GetAddress());
                options.AddExtension("proxy.zip");
            }
            options.AddArgument("--user-data-dir=" + Directory.GetCurrentDirectory() + @"\Chrome");
            options.BinaryLocation = MainWindowViewModel.settings.ChromePath;
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeDriver = new ChromeDriver(service, options);
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="user">Данные пользователя для регистрации</param>
        public void RegisterBitClout(UserRegistrationInfo user)
        {
            PhoneNumber pn = PhoneWorker.GetPhoneNumber(ServiceCodes.lt);
            InitializeChromeDriver(true);
            ChromeDriver.Navigate().GoToUrl($"https://bitclout.com/sign-up");
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

            var uploadPhoto = ChromeDriver.FindElement(By.Id("file"));
            uploadPhoto.SendKeys(user.PhotoPath);
            uploadPhoto.Submit();
            Thread.Sleep(5000);
        }
    }
}
