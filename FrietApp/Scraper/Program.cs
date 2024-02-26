using System.Text;
using SnackApp.Logic.Scraper;

const string baseDomain = "https://cafetariabienvenue.12waiter.eu";
IScrapeProcess process = new ProductScrapeProcess();
var items = process.StartScrape(baseDomain);

//Test
foreach (var item in items)
{
    Console.WriteLine(item.Name);
    Console.WriteLine(item.Price);
    if (item.Dimensions is not null)
    {
        foreach (var dimension in item.Dimensions)
        {
            Console.WriteLine($"Dimension: {dimension.Name}");
            Console.WriteLine($"Dimension Price: {dimension.Price}");
        }
    }
}

return items.Count;

