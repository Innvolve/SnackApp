using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using SnackApp.Models;
using SnackApp.Models.ItemProperties;

namespace SnackApp.Logic.ItemPropertyHandlers;

public class ItemDimensionHandler
{
    
    internal List<ItemDimension> CollectItemDimensions(HtmlNode node)
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

            dimensions.AddRange(HandleDimensionsGroup(group, isOptional));
        }

        return dimensions;
    }
    
    List<ItemDimension> HandleDimensionsGroup(HtmlNode dimensionGroupNode, bool isOptional)
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

        var groupId = dimensionGroupIdInputs.FirstOrDefault()?.GetAttributeValue("class","N/A") ?? string.Empty;
        if (groupId.Length <= 0)
        {
            throw new Exception("dimensions group identifier is empty");
        }

        var dimensions = new List<ItemDimension>();
        foreach (var element in dimensionElements)
        {
            dimensions.Add(ConstructItemDimension(element, groupId, isOptional));
        }

        return dimensions;
    }
    
    
    ItemDimension ConstructItemDimension(HtmlNode dimensionSelection, string groupId, bool isOptional)
    {
        var isAlwaysChecked = !(dimensionSelection.HasClass("checkbox") || !dimensionSelection.HasClass("checked"));
    
        var isMutuallyExclusive = true;

        var checkBoxInput = dimensionSelection.QuerySelectorAll("input[type=checkbox]:not([type=hidden])");

        if (checkBoxInput is not null && checkBoxInput.Count > 0)
        {
            isMutuallyExclusive = false;
        }

        var dimensionIdInput = dimensionSelection.QuerySelector("input[id$='__Id']");
    
        var dimensionId = dimensionIdInput.GetAttributeValue("value","N/A") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(dimensionId))
        {
            throw new Exception("dimension identifier is empty");
        }
    
        var rawDimensionPrice = dimensionSelection.QuerySelector("div[class=product-dimension-price]")?.InnerText ?? string.Empty;

        double dimensionPrice;
        string currencySymbol;

        if (string.IsNullOrWhiteSpace(rawDimensionPrice))
        {
            dimensionPrice = 0;
            currencySymbol = string.Empty;
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

        var name = dimensionSelection.QuerySelector("div[class=product-dimension-title]").InnerText.Trim();

        var dimension = new ItemDimension
        {
            Name = name,
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

}