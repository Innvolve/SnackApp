using System.Text;
using System.Text.Json.Nodes;
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
var items = new List<Item>();
foreach (var link in productLinks)
{
    var productLink = baseDomain + link;
    var node = GetHtml(productLink);
    CollectItem(items, node);
}


//Test
Console.OutputEncoding = Encoding.UTF8;

foreach (var item in items)
{
    Console.WriteLine(item.Name);
    Console.WriteLine(item.Price);
    foreach (var dimension in item.Dimensions)
    {
        Console.WriteLine($"Dimension: {dimension.Name}");
        Console.WriteLine($"Dimension Price: {dimension.Price}");
    }
}

return items.Count;

//Methods
void CollectItem(List<Item> items, HtmlNode node)
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
    
    //Deposit Value?
    var rawDepositValue = node.QuerySelector("div.product-deposit")?.InnerText ?? string.Empty;

    var depositValue = new double();

    if (string.IsNullOrWhiteSpace(rawDepositValue))
    {
        depositValue = 0;
    }
    
    else
    {
        var depositValueParts = rawDepositValue.Split(" ");

        if (depositValueParts.Length != 4)
        {
            throw new Exception($"invalid deposit value format: {rawDepositValue}");
        }

        currencySymbol = depositValueParts[1];

        var depositValueStr = depositValueParts[2].Replace(".", ",");

        if (!double.TryParse(depositValueStr, out var result))
        {
            throw new Exception("couldn't parse option price");
        }

        depositValue = result;
    }

    var availability = false;
    var jsonNode = node.QuerySelector("head").QuerySelector("script");
   
    var jsonString = jsonNode.InnerText;

    if (!string.IsNullOrEmpty(jsonString))
    {
        var jsonObject = JsonObject.Parse(jsonString);
    
        if (jsonObject["offers"]["availability"] != null && jsonObject["offers"]["availability"].ToString() == "https://schema.org/InStock")
        {
            availability = true;
        }
    }
    
    var options = CollectItemOptions(node, currencySymbol);

    var associations = CollectItemAssociations(node, currencySymbol);

    var dimensions = CollectItemDimensions(node, currencySymbol);

    var imgUrl = node.QuerySelector("img").GetAttributeValue("src");
    var item = new Item
    {
        Slug = node.QuerySelector("input[id=Editor_Slug]").GetAttributeValue("value"),
        Name = node.QuerySelector("h1").InnerText,
        Description = node.QuerySelector(".product-description-short")?.InnerText ?? string.Empty,
        ItemUrl = node.QuerySelector("img").GetAttributeValue("src"),
        
        Price = new Currency
        {
            CurrencySymbol = currencySymbol,
            Value = itemPrice,
        },
        Options = options,
        Associations = associations,
        Dimensions = dimensions,
        Availability = availability,
        DepositValue = new Currency
        {
            CurrencySymbol = currencySymbol,
            Value = depositValue,
        },
    };
    
    items.Add(item);
}

List<ItemOption> CollectItemOptions(HtmlNode node, string currencySymbol)
{
    var optionGroups = node.QuerySelectorAll(".product-option-group");

    if (optionGroups.Count <= 0)
    {
        return new List<ItemOption>();
    }

    var options = new List<ItemOption>();
    foreach (var group in optionGroups)
    {
        var isOptional = false;

        var optionalSpan = group.QuerySelector("control-optional");
        if (optionalSpan?.HasChildNodes ?? false)
        {
            isOptional = true;
        }

        options.AddRange(HandleOptionsGroup(options, group, currencySymbol, isOptional));
    }

    return options;
}

List<ItemAssociation> CollectItemAssociations(HtmlNode node, string currencySymbol)
{
    var associationsGroups = node.QuerySelectorAll(".product-association-group");

    if (associationsGroups.Count <= 0)
    {
        return new List<ItemAssociation>();
    }

    var associations = new List<ItemAssociation>();

    foreach (var groupSelection in associationsGroups)
    {
        var isOptional = false;

        var optionalSpan = groupSelection.QuerySelector("legend span.badge.control-optional");

        if (optionalSpan?.HasChildNodes ?? false)
        {
            isOptional = true;
        }
        
        associations.AddRange(HandleAssociationGroup(associations, groupSelection, currencySymbol, isOptional));
    }

    return associations;
}

List<ItemDimension> CollectItemDimensions(HtmlNode node, string currencySymbol)
{
    var dimensionGroups = node.QuerySelectorAll(".product-dimensions-placeholder");

    if (dimensionGroups.Count <= 0)
    {
        return new List<ItemDimension>();
    }

    var dimensions = new List<ItemDimension>();
    foreach (var group in dimensionGroups)
    {
        var isOptional = false;

        var optionalSpan = group.QuerySelector("control-optional");
        if (optionalSpan?.HasChildNodes ?? false)
        {
            isOptional = true;
        }

        dimensions.AddRange(HandleDimensionsGroup(dimensions, group, currencySymbol, isOptional));
    }

    return dimensions;
}


List<ItemOption> HandleOptionsGroup(List<ItemOption> options, HtmlNode optionGroupNode, string currencySymbol, bool isOptional)
{
    var optionElements = optionGroupNode.QuerySelectorAll(".product-option.form-check");
    
    if (optionElements.Count <= 0)
    {
        throw new Exception("no options found in option group");
    }
    
    var optionGroupIdInputs = optionGroupNode.QuerySelectorAll("input[id$='__Id'][type=hidden]");
    if (optionGroupIdInputs.Count <= 0)
    {
        throw new Exception("cannot find option group identifier element");
    }

    var groupId = optionGroupIdInputs.FirstOrDefault().GetAttributeValue("value");
    if (groupId.Length <= 0)
    {
        throw new Exception("option group identifier is empty");
    }

    foreach (var element in optionElements)
    {
        options.Add(ConstructItemOption(groupId, isOptional, currencySymbol, element));
    }

    return options;
}

List<ItemAssociation> HandleAssociationGroup(List<ItemAssociation> associations, HtmlNode groupSelection, string currencySymbol, bool isOptional)
{
    var associationNameLegends = groupSelection.QuerySelectorAll("legend:not([class])");

    if (associationNameLegends.Count <= 0)
    {
        throw new Exception("cannot find association name element");
    }

    var associationName = associationNameLegends.First().InnerText.Trim(); //TODO check correctness

    if (string.IsNullOrWhiteSpace(associationName))
    {
        throw new Exception("association name is empty");
    }

    var associationElements = groupSelection.QuerySelectorAll("label.product-association");
    if (associationElements.Count <= 0)
    {
        throw new Exception("no associations found in association group");
    }

    var associationGroupInputs = groupSelection.QuerySelectorAll("input[id$='__Id'][type=hidden]");

    if (associationGroupInputs.Count <= 0)
    {
        throw new Exception("cannot find association group identifier element");
    }

    var groupId = associationGroupInputs.First().GetAttributeValue("value"); //TODO: automatically returns empty of n/a?

    if (groupId.Length <= 0)
    {
        throw new Exception("association group identifier is empty");
    }

    foreach (var associationSelection in associationElements)
    {
        var association = ConstructItemAssociation(associationName, groupId, isOptional, currencySymbol, associationSelection);
        associations.Add(association);
    }

    return associations;
}

List<ItemDimension> HandleDimensionsGroup(List<ItemDimension> dimensions, HtmlNode dimensionGroupNode, string currencySymbol, bool isOptional)
{
    var dimensionElements = dimensionGroupNode.QuerySelectorAll(".product-dimension");
    
    if (dimensionElements.Count <= 0)
    {
        throw new Exception("no dimensions found in dimension group");
    }
    
    var dimensionGroupIdInputs = dimensionGroupNode.QuerySelectorAll("label[for^='Editor.Dimensions']");
    if (dimensionGroupIdInputs.Count <= 0)
    {
        throw new Exception("cannot find dimensions group identifier element");
    }

    var groupId = dimensionGroupIdInputs.FirstOrDefault().GetAttributeValue("class");
    if (groupId.Length <= 0)
    {
        throw new Exception("dimensions group identifier is empty");
    }

    foreach (var element in dimensionElements)
    {
        dimensions.Add(ConstructItemDimension(groupId, isOptional, currencySymbol, element));
    }

    return dimensions;
}


ItemOption ConstructItemOption(string groupId, bool isOptional, string currencySymbol, HtmlNode optionSelection)
{
    var isMutuallyExclusive = true;

    var checkBoxInput = optionSelection.QuerySelectorAll("input[type=checkbox]:not([type=hidden])");

    if (checkBoxInput is not null && checkBoxInput.Count > 0)
    {
        isMutuallyExclusive = false;
    }

    var optionIdInputs = optionSelection.QuerySelectorAll("input[id$='__Id'][type=hidden]");

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
    var rawOptionPrice = optionLabel.QuerySelector("span.product-option-price")?.InnerText ?? string.Empty;

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

        var optionPriceStr = optionPriceParts[2].Replace(".", ",");

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
        Name = optionLabel.FirstChild.InnerText.Trim(),
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

    var associationIdInputs = associationSelection.QuerySelectorAll("input[id$='__Id'][type=hidden]");

    if (associationIdInputs is null)
    {
        throw new Exception("cannot find association identifier element");
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
        var associationPriceParts = rawPrice.Split(" ");
        if (associationPriceParts.Length != 2)
        {
            throw new Exception($"invalid association price format {rawPrice}");
        }

        currencySymbol = associationPriceParts[0];

        var optionPriceStr = associationPriceParts[1].Replace(".", ",");

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

ItemDimension ConstructItemDimension(string groupId, bool isOptional, string currencySymbol, HtmlNode dimensionSelection)
{
    var isAlwaysChecked = !(dimensionSelection.HasClass("checkbox") || !dimensionSelection.HasClass("checked"));
    
    var isMutuallyExclusive = true;

    var checkBoxInput = dimensionSelection.QuerySelectorAll("input[type=checkbox]:not([type=hidden])");

    if (checkBoxInput is not null && checkBoxInput.Count > 0)
    {
        isMutuallyExclusive = false;
    }

    var dimensionIdInput = dimensionSelection.QuerySelector("input[id$='__Id']");
    
   var dimensionId = dimensionIdInput.GetAttributeValue("value") ?? string.Empty;

    if (string.IsNullOrWhiteSpace(dimensionId))
    {
        throw new Exception("dimension identifier is empty");
    }
    
    var rawDimensionPrice = dimensionSelection.QuerySelector("div[class=product-dimension-price]")?.InnerText ?? string.Empty;

    var dimensionPrice = new double();

    if (string.IsNullOrWhiteSpace(rawDimensionPrice))
    {
        dimensionPrice = 0;
    }
    
    else
    {
        var dimensionPriceParts = rawDimensionPrice.Split(" ");

        if (dimensionPriceParts.Length != 2)
        {
            throw new Exception($"invalid dimension price format: {rawDimensionPrice}");
        }

        currencySymbol = dimensionPriceParts[0];

        var dimensionPriceStr = dimensionPriceParts[1].Replace(".", ",");

        if (!double.TryParse(dimensionPriceStr, out var result))
        {
            throw new Exception("couldn't parse dimension price");
        }

        dimensionPrice = result;
    }

    var dimension = new ItemDimension
    {
        Name = dimensionSelection.QuerySelector("div[class=product-dimension-title]").InnerText.Trim(),
        IsMutuallyExclusive = isMutuallyExclusive,
        IsAlwaysChecked = isAlwaysChecked,
        GroupId = groupId,
        IsOptional = isOptional,
        Price = new Currency
        {
            Value = dimensionPrice,
            CurrencySymbol = currencySymbol,
        }
    };
    return dimension;
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
    var webPage = scrapingBrowser.NavigateToPage(new Uri(url));
    return webPage.Html;
}