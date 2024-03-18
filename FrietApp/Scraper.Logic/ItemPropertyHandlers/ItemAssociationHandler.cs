using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Shared.Models;
using SnackApp.Models;
using SnackApp.Models.ItemProperties;

namespace SnackApp.Logic.ItemPropertyHandlers;

public class ItemAssociationHandler
{
    internal List<ItemAssociation> CollectItemAssociations(HtmlNode node)
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
        
            associations.AddRange(HandleAssociationGroup(groupSelection, isOptional));
        }

        return associations;
    }
    

    private List<ItemAssociation> HandleAssociationGroup(HtmlNode groupSelection, bool isOptional)
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

        var groupId = associationGroupInputs.First().GetAttributeValue("value","N/A"); //TODO: automatically returns empty or n/a?

        if (groupId.Length <= 0)
        {
            throw new Exception("association group identifier is empty");
        }

        var associations = new List<ItemAssociation>();
        foreach (var associationSelection in associationElements)
        {
            var association = ConstructItemAssociation(associationSelection, associationName, groupId, isOptional);
            associations.Add(association);
        }

        return associations;
    }
    

    ItemAssociation ConstructItemAssociation(HtmlNode associationSelection, string name, string groupId,
        bool isOptional)
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

        var associationId = associationIdInputs.FirstOrDefault()?.GetAttributeValue("value","N/A") ?? string.Empty;

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

        double associationPrice;
        string currencySymbol;

        if (rawPrice.Length <= 0)
        {
            associationPrice = 0;
            currencySymbol = string.Empty;
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

}