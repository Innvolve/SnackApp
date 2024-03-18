using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Shared.Models;
using SnackApp.Models;
using SnackApp.Models.ItemProperties;

namespace SnackApp.Logic.ItemPropertyHandlers;

public class ItemOptionHandler
{
    public List<ItemOption> CollectItemOptions(HtmlNode node)
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

            options.AddRange(HandleOptionsGroup(group, isOptional));
        }

        return options;
    }
    
    List<ItemOption> HandleOptionsGroup(HtmlNode optionGroupNode, bool isOptional)
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

        var groupId = optionGroupIdInputs.FirstOrDefault().GetAttributeValue("value","N/A");
        if (groupId.Length <= 0)
        {
            throw new Exception("option group identifier is empty");
        }

        var options = new List<ItemOption>();
        foreach (var element in optionElements)
        {
            options.Add(ConstructItemOption(element, groupId, isOptional));
        }

        return options;
    }
    
    

ItemOption ConstructItemOption(HtmlNode optionSelection, string groupId, bool isOptional)
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

    var optionId = optionIdInputs.FirstOrDefault().GetAttributeValue("value","N/A") ?? string.Empty;

    if (string.IsNullOrWhiteSpace(optionId))
    {
        throw new Exception("option identifier is empty");
    }

    var optionLabel = optionSelection.QuerySelector("label.form-check-label");
    var rawOptionPrice = optionLabel.QuerySelector("span.product-option-price")?.InnerText ?? string.Empty;

    double optionPrice;
    string currencySymbol;

    if (string.IsNullOrWhiteSpace(rawOptionPrice))
    {
        optionPrice = 0;
        currencySymbol = string.Empty;
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
}