using System.Text.Json;
using SnackApp.Logic;
using SnackApp.Logic.Scraper;

const string baseDomain = "https://cafetariabienvenue.12waiter.eu";
const string outputPath = @"C:\repositories\SnackApp\FrietApp\Output";
IScrapeProcess process = new ProductScrapeProcess();
var items = process.StartScrape(baseDomain);

var jsonData = JsonSerializer.Serialize(items);
if (string.IsNullOrWhiteSpace(jsonData))
{
    throw new Exception("Error encoding JSON");
}

if (!Path.Exists(outputPath))
{
    Directory.CreateDirectory(outputPath);
}

await using (var outputFile = new StreamWriter(Path.Combine(outputPath, "menu.json")))
{
    await outputFile.WriteAsync(jsonData);
}



//Test
foreach (var item in items)
{
    Console.WriteLine(item.Name);
    Console.WriteLine(item.Price.CurrencyString);
    if (item.Dimensions is not null)
    {
        foreach (var dimension in item.Dimensions)
        {
            Console.WriteLine($"Dimension: {dimension.Name}");
            Console.WriteLine($"Dimension Price: {dimension.Price.CurrencyString}");
        }
    }
}

return items.Count;

