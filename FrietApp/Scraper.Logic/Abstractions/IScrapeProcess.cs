namespace SnackApp.Logic.Abstractions;

public interface IScrapeProcess
{
    List<Models.Item> StartScrape(string baseUrl);
}