namespace Entities;

public class GroupOrder
{
    public string Owner { get; init; }
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public List<Order> Orders { get; init; } = new();

    public Currency PriceTotal
    {
        get
        {
            return new Currency
            {
                CurrencySymbol = Orders.FirstOrDefault()?.OrderPrice.CurrencySymbol ?? string.Empty,
                Value = Orders.Sum(orders => orders.OrderPrice.Value),
            };
        }
    }
}