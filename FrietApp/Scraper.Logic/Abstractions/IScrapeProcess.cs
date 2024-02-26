namespace SnackApp.Logic.Scraper;

public interface IScrapeProcess
{
    List<Models.Item> StartScrape(string baseUrl);
}