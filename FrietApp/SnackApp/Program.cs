using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using SnackApp.Models;
using SnackApp.Models.ItemProperties;

const string baseDomain = "https://cafetariabienvenue.12waiter.eu";
var scrapingBrowser = new ScrapingBrowser();

//Get product links
var collectionLinks = GetCollectionLinks(baseDomain);
var productLinks = GetProductLinks(collectionLinks);

// Collect Items
var items = CollectItems(productLinks);

//Test
foreach (var item in items)
{
    Console.WriteLine(item.Name);
    Console.WriteLine(item.Description);
    Console.WriteLine(item.ImgUrl);
    Console.WriteLine(item.Price);
    foreach (var option in item.Options)
    {
        Console.WriteLine($"Option Group: {option.GroupId}");
        Console.WriteLine($"Option ID: {option.Id}");
        Console.WriteLine($"Option: {option.Name}");
    }
}

return;

//Methods
List<Item> CollectItems(List<string> productLinks)
{
    var items = new List<Item>();
    foreach (var link in productLinks)
    {
        var productUrl = $"{baseDomain}{link}";
        var html = GetHtml(productUrl);
        var documentNode = html.OwnerDocument.DocumentNode;
      
        items.Add(ConstructItem(documentNode));
    }
    return items;
}

List<ItemOption> CollectTotalOptions(HtmlNode documentNode)
{
    var totalOptions = new List<ItemOption>();

    var optionGroupNodes = documentNode.QuerySelectorAll(".product-option-group");
    foreach (var optionGroupNode in optionGroupNodes)
    {
        var options = CollectOptions(optionGroupNode);
        totalOptions.AddRange(options);
    }
            
    return totalOptions;
}


List<ItemOption> CollectOptions(HtmlNode optionGroupNode)
{
    var optionNodes = optionGroupNode.QuerySelectorAll(".product-option.form-check"); 
    
    var options = new List<ItemOption>();
    foreach (var optionNode in optionNodes)
    {
        var values = optionGroupNode.ChildAttributes("value").ToString();
        options.Add(new ItemOption
        {
            Id = optionNode.GetAttributeValue("value"),
            Name = optionNode.InnerText.Trim().Split('\n')[0],
            // Price = ,
            GroupId = values,
            // IsOptional = ,
            // IsMutuallyExclusive = ,
        });
    }

    var key = optionGroupNode.InnerText.Trim().Split('\n')[0];
    
    return options;
}

List<string> GetCollectionLinks(string url)
{
    var html = GetHtml(url);
    var links = html.CssSelect("a.collection-item[href^='/c/']");
    
    var pageCollectionLinks = new List<string>();
    foreach (var link in links)
    {
        pageCollectionLinks.Add(link.Attributes["href"].Value);
    }
    return pageCollectionLinks;
}

List<string> GetProductLinks(List<string> collectionUrls)
{
    var productLinks = new List<string>();
    foreach (var url in collectionUrls)
    {
        var productPage = $"{baseDomain}{url}";
        var html = GetHtml(productPage);
        var links = html.CssSelect("a.product-item[href^='/c/'][href*='/p/']");
        if (!links.Any())
        {
            var subcollectionUri = GetCollectionLinks(productPage);
            foreach (var subUri in subcollectionUri)
            {
                var subcollectionPage = $"{baseDomain}{subUri}";
                var subhtml = GetHtml(subcollectionPage);
                links = subhtml.CssSelect("a.product-item[href^='/c/'][href*='/p/']");
                foreach (var link in links)
                {
                    productLinks.Add(link.Attributes["href"].Value);
                }
            }
        }
        foreach (var link in links) 
        {
            productLinks.Add(link.Attributes["href"].Value);
        }
    }
    return productLinks;
}

Item ConstructItem(HtmlNode documentNode)
{
    return new Item
    {
        Name = documentNode.QuerySelector("h1").InnerText,
        Description = documentNode.QuerySelector("div.product-section.product-intro > p")?.InnerText ?? "N/A",
        ImgUrl = documentNode.QuerySelector("img").ChildAttributes("src").FirstOrDefault()?.Value ?? "N/A",
        Price = Convert.ToDouble(documentNode.QuerySelector(".product-price").InnerText.Split(' ')[1]),
        Options = CollectTotalOptions(documentNode),
        // Associations  = node.QuerySelector("").InnerText,
        // Size = node.QuerySelector("").InnerText,
        // Availability = true //node.SelectSingleNode("").InnerText;
    };
}

HtmlNode GetHtml(string url)
{
    var webPage = scrapingBrowser.NavigateToPage(new Uri(url));
    return webPage.Html;
}