using System.Data;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using SnackApp.Models;


//source: https://zenscrape.com/web-scraping-csharp/
var scrapingBrowser = new ScrapingBrowser();
const string baseDomain = "https://cafetariabienvenue.12waiter.eu";

//Get product links
var collectionLinks = GetCollectionLinks(baseDomain);
var productLinks = GetProductLinks(collectionLinks);

// Create Items
foreach (var link in productLinks)
{
    var productlink = $"{baseDomain}{link}";
    var html = GetHtml(productlink);
    var node = html.OwnerDocument.DocumentNode;
    
    
    //Construct options Dictionaries
    var totalOptions = new List<Dictionary<string, List<string>>>();
    
    var optionGroupNodes = node.QuerySelectorAll(".product-option-group");
    foreach (var optionGroupNode in optionGroupNodes)
    {
        var optionsDict = new Dictionary<string, List<string>>();
        var optionNodes = optionGroupNode.QuerySelectorAll("label.form-check-label"); //TODO css selector doesnt work?
        var options = new List<string>();
        foreach (var optionNode in optionNodes)   
        {
            options.Add(optionNode.InnerText);
        }
        optionsDict.Add(optionGroupNode.InnerText,options);
        totalOptions.Add(optionsDict);
    }
    
    
    
    var items = new List<Item>();
    
    try
    {
        items.Add(new Item
        {
            Name = node.QuerySelector("h1").InnerText,
            Description = node.QuerySelector("div.product-section.product-intro > p")?.InnerText ?? "N/A",
            ImgUrl = node.QuerySelector("img").ChildAttributes("src").FirstOrDefault()?.Value ?? "N/A",
            Price = Convert.ToDouble(node.QuerySelector(".product-price").InnerText.Split(' ')[1]),
            Options =  totalOptions,
            // Associations  = node.QuerySelector("").InnerText,
            // Size = node.QuerySelector("").InnerText,
            // Availability = true //node.SelectSingleNode("").InnerText;
        });
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }

    
    foreach (var item in items)
    {
        Console.WriteLine(item.Name);
        Console.WriteLine(item.Description);
        Console.WriteLine(item.ImgUrl);
        Console.WriteLine(item.Price);
        foreach (var options in item.Options)
        {
            foreach (var pair in options)
            {
                Console.WriteLine(pair.Key);
                var values = pair.Value;
                foreach (var value in values)
                {
                    Console.WriteLine(pair.Value);
                }
            }
        }
    }
}

//Methods
List<string> GetCollectionLinks(string url)
{
    var pageCollectionLinks = new List<string>();
    var html = GetHtml(url);
    var links = html.CssSelect("a.collection-item[href^='/c/']");

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

HtmlNode GetHtml(string url)
{
    var webPage = scrapingBrowser.NavigateToPage(new Uri(url));
    return webPage.Html;
}