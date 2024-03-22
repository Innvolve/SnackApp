using SnackApp.Models;

namespace Entities;

public class Order
{
    public string Owner { get; init; }
    
    public List<Item> Items;

    public Currency OrderPrice
    {
        get
        {
            return new Currency
            {
                CurrencySymbol = Items.FirstOrDefault()?.Price.CurrencySymbol ?? string.Empty,
                Value = Items.Sum(item => item.Price.Value)
            };
        }
    }

    public DateTime Time { get; } = DateTime.UtcNow;

    public Order(List<Item> items, string owner)
    {
        Owner = owner;
        Items = items;
    }
    
}
