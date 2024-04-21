using OpenQA.Selenium.Firefox;

namespace InsightsInn.Api.Scrapables.WebSites;

internal class Facebook
{
    public void Scrape()
    {
        var url = "//https://www.facebook.com/Fettah2024?mibextid=ZbWKwL";

        var options = new FirefoxOptions()
        {
            BinaryLocation = "/snap/bin/firefox"
        };
        options.AddArguments("headless", "disable-gpu");

        var browser = new FirefoxDriver(options);

        browser.Navigate().GoToUrl(url);

        var image = browser.GetFullPageScreenshot();
    }
}