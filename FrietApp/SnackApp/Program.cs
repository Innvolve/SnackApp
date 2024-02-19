using System.Runtime.InteropServices.ComTypes;
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
      
        items.Add(CollectTotalOptions(documentNode));
    }
    return items;
}

List<ItemOption> CollectTotalOptions(HtmlNode documentNode)
{
    var totalOptions = new List<ItemOption>();

    var optionGroupNodes = documentNode.QuerySelectorAll(".product-option-group");
    
    foreach (var optionGroupNode in optionGroupNodes)
    {
        //collect options
        var isOptional = false;

        var optionalSpan = optionGroupNode.QuerySelectorAll("legend span.badge.control-optional");
        if (optionalSpan != null && optionalSpan.Count > 0)
        {
            isOptional = true;
        }
        
        var options = HandleOptionsGroup(totalOptions, optionGroupNode, currencySymbol, isOptional);
        totalOptions.AddRange(options);
    }
            
    return totalOptions;
}


List<ItemOption> HandleOptionsGroup(List<ItemOption> options, HtmlNode optionGroupNode, string currencySymbol, bool isOptional)
{
    var optionElements = optionGroupNode.QuerySelectorAll(".product-option.form-check");
    var optionGroupIdInputs = optionGroupNode.QuerySelectorAll(".input[id=__id],[type=hidden]");
    if (optionGroupIdInputs is null)
    {
        throw new Exception("cannot find option group identifier element");
    }

    var groupId = optionGroupIdInputs.FirstOrDefault().GetAttributeValue("value");
    if (groupId is null)
    {
        throw new Exception("option group identifier is empty");
    }

    var itemOptions = new List<ItemOption>();

    foreach (var element in optionElements)
    {
        itemOptions.Add(ConstructItemOption(groupId, isOptional, currencySymbol, element));
    }

    return itemOptions;
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

ItemOption ConstructItemOption(string groupId, bool isOptional, string currencySymbol, HtmlNode optionSelection) //TODO: currencySymbol
{
    var isMutuallyExclusive = true;

    var checkBoxInput = optionSelection.QuerySelectorAll("input[type=checkbox]:not([type=hidden])");

    if (checkBoxInput is not null && checkBoxInput.Count > 0)
    {
        isMutuallyExclusive = false;
    }

    var optionIdInputs = optionSelection.QuerySelectorAll("input[__Id][type=hidden]");

    if (optionIdInputs.Count <= 0)
    {
        throw new Exception("cannot find option identifier element");
    }

    var optionId = optionIdInputs.FirstOrDefault().GetAttributeValue("value") ?? string.Empty;

    if (string.IsNullOrWhiteSpace(optionId))
    {
        throw new Exception("option identifier is empty");
    }

    var optionLabel = optionSelection.QuerySelector("label.form-check-label");
    var rawOptionPrice = optionLabel.QuerySelector("span.product-option-price").InnerText;

    var optionPrice = new double();

    if (string.IsNullOrWhiteSpace(rawOptionPrice))
    {
        optionPrice = 0;
    }
    
    else
    {
        
        var optionPriceParts = rawOptionPrice.Split(" ");

        if (optionPriceParts.Length != 3)
        {
            throw new Exception($"invalid option price format: {rawOptionPrice}");
        }

        currencySymbol = optionPriceParts[1];

        var optionPriceStr = optionPriceParts[2].Replace(",", ".");

        if (!double.TryParse(optionPriceStr, out double result))
        {
            throw new Exception("couldn't parse option price");
        }

        optionPrice = result;
    }

    var option = new ItemOption
    {
        Id = optionId,
        GroupId = groupId,
        Name = optionLabel.ToString().Trim(),
        IsOptional = isOptional,
        IsMutuallyExclusive = isMutuallyExclusive,
        Price = new Currency
        {
            Value = optionPrice,
            CurrencySymbol = currencySymbol,
        }
    };
    return option;
}

ItemAssociation ConstructItemAssociation(string name, string groupId, bool isOptional, string currencySymbol,
    HtmlNode associationSelection)
{
    var isAlwaysChecked = true;

    if (associationSelection.HasClass("checkbox") || !associationSelection.HasClass("checked"))
    {
        isAlwaysChecked = false;
    }

    var associationIdInputs = associationSelection.QuerySelectorAll("input[__id][type=hidden");

    if (associationIdInputs is null)
    {
        throw new Exception("cannot find option identifier element");
    }

    var associationId = associationIdInputs.FirstOrDefault().GetAttributeValue("value") ?? string.Empty;

    if (associationId is null)
    {
        throw new Exception("association identifier is empty");
    }

    var description = associationSelection.QuerySelector(".product-association-title").InnerText.Trim();

    if (name == description)
    {
        description = string.Empty;
    }

    var rawPrice = associationSelection.QuerySelector(".product-association-price").InnerText;

    var associationPrice = new double();

    if (rawPrice.Length <= 0)
    {
        associationPrice = 0;
    }
    else
    {
        var associationPriceParts = rawPrice.Split(' ');
        if (associationPriceParts.Length != 2)
        {
            throw new Exception($"invalid association price format {rawPrice}");
        }

        currencySymbol = associationPriceParts[0];

        var optionPriceStr = associationPriceParts[1].Replace(",", ".");

        if (!double.TryParse(optionPriceStr, out double result))
        {
            throw new Exception("couldn't parse association price");
        }

        associationPrice = result;
    }

    var association = new ItemAssociation
    {
        Id = associationId,
        GroupId = groupId,
        Name = name,
        Description = description,
        IsOptional = isOptional,
        IsAlwaysChecked = isAlwaysChecked,
        Price = new Currency
        {
            Value = associationPrice,
            CurrencySymbol = currencySymbol
        }
    };
    
    return association;
}

HtmlNode GetHtml(string url)
{
    var webPage = scrapingBrowser.NavigateToPage(new Uri(url));
    return webPage.Html;
}