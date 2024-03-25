using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Entities;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using SnackApp.Logic.Abstractions;
using SnackApp.Logic.ItemPropertyHandlers;
using SnackApp.Models;

namespace SnackApp.Logic;

public class ProductScrapeProcess : IScrapeProcess
{
    private readonly ScrapingBrowser _scrapingBrowser = new();
    private readonly ItemAssociationHandler _associationHandler = new();
    private readonly ItemDimensionHandler _itemDimensionHandler = new();
    private readonly ItemOptionHandler _itemOptionHandler = new();
    public List<Item> StartScrape(string baseUrl)
    {
        //Get product links
        var collectionLinks = GetCollectionLinks(baseUrl);
        var productLinks = GetProductLinks(baseUrl, collectionLinks);

        // Collect Items
        var items = new List<Item>();
        foreach (var link in productLinks)
        {
            var productLink = baseUrl + link;
            var node = GetHtml(productLink);
            var item = CollectItem(node);
            items.Add(item);
        }

        return items;
    }

    private Item CollectItem(HtmlNode node)
    {

        var imgUrl = node.QuerySelector("img").GetAttributeValue("src");
        var item = new Item
        {
            Slug = node.QuerySelector("input[id=Editor_Slug]").GetAttributeValue("value"),
            Name = node.QuerySelector("h1").InnerText,
            Description = node.QuerySelector(".product-description-short")?.InnerText ?? string.Empty,
            ImgUrl = node.QuerySelector("img").GetAttributeValue("src"),
            ItemUrl = node.QuerySelector("img").GetAttributeValue("src"), //TODO 
            Price = GetItemPrice(node),
            Options = _itemOptionHandler.CollectItemOptions(node),
            Associations = _associationHandler.CollectItemAssociations(node),
            Dimensions = _itemDimensionHandler.CollectItemDimensions(node),
            Availability = GetProductAvailability(node),
            DepositValue = GetItemDepositValue(node),
        };

        return item;
    }

    private bool GetProductAvailability(HtmlNode node)
    {
        var availability = false;
        var jsonNode = node.QuerySelector("head").QuerySelector("script");

        var jsonString = jsonNode.InnerText;

        if (string.IsNullOrEmpty(jsonString))
        {
            return availability;
        }

        var jsonObject = JsonNode.Parse(jsonString) ?? throw new Exception("Unable to parse jsonObject");

        var offersJson = jsonObject["offers"]?.ToJsonString();

        if (string.IsNullOrWhiteSpace(offersJson))
        {
            throw new InvalidOperationException("No order JsonInfo found");
        }

        var offerAvailability = JsonSerializer.Deserialize<Offer>(offersJson);
        availability = offerAvailability.Availability == "https://schema.org/InStock";

        return availability;
    }

    public struct Offer
    {
        [JsonPropertyName("@type")]
        public string Type;
        public string Url;
        public string Availability;
        public double Price;
        public string Pricecurrency;
    }


    internal Currency GetItemPrice(HtmlNode node)
    {
        var rawItemPrice = node.QuerySelector("div.product-price").InnerText;

        var itemPriceParts = rawItemPrice.Split(" ");

        if (itemPriceParts.Length != 2)
        {
            throw new Exception($"invalid item price format: {rawItemPrice}");
        }

        var currencySymbol = itemPriceParts[0];

        var itemPriceStr = itemPriceParts[1].Replace(".", ",");

        if (!double.TryParse(itemPriceStr, out var itemPrice))
        {
            throw new Exception("unable to parse item price");
        }

        return new Currency
        {
            CurrencySymbol = currencySymbol,
            Value = itemPrice,
        };
    }

    public Currency GetItemDepositValue(HtmlNode node)
    {
        //Deposit Value?
        var rawDepositValue = node.QuerySelector("div.product-deposit")?.InnerText ?? string.Empty;

        if (string.IsNullOrWhiteSpace(rawDepositValue))
        {
            return new Currency
            {
                CurrencySymbol = string.Empty,
                Value = 0,
            };
        }

        var depositValueParts = rawDepositValue.Split(" ");

        if (depositValueParts.Length != 4)
        {
            throw new Exception($"invalid deposit value format: {rawDepositValue}");
        }

        var currencySymbol = depositValueParts[1];

        var depositValueStr = depositValueParts[2].Replace(".", ",");

        if (!double.TryParse(depositValueStr, out var result))
        {
            throw new Exception("couldn't parse option price");
        }

        return new Currency
        {
            CurrencySymbol = currencySymbol,
            Value = result,
        };
    }
    List<string> GetProductLinks(string baseDomain, List<string> collectionUrls)
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

    HtmlNode GetHtml(string url)
    {
        var webPage = _scrapingBrowser.NavigateToPage(new Uri(url));
        return webPage.Html;
    }
}