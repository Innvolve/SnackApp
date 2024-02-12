using HtmlAgilityPack;
using ScrapySharp;
using ScrapySharp.Extensions;
using ScrapySharp.Network;


//source: https://zenscrape.com/web-scraping-csharp/
var scrapingBrowser = new ScrapingBrowser();
var baseDomain = "https://cafetariabienvenue.12waiter.eu";


var collectionLinks = GetCollectionLinks(baseDomain);
var productLinks = GetProductLinks(collectionLinks);
foreach (var link in productLinks)
{
    Console.WriteLine(link);
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