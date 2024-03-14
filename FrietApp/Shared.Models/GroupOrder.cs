using SnackApp.Models;

namespace Shared.Models;

public class GroupOrder
{
    public DateTime Date { get; } = DateTime.UtcNow;
    public List<Order> Orders { get; set; } = new();

    public Currency PriceTotal
    {
        get
        {
            return new Currency
            {
                //TODO: 
                CurrencySymbol = Orders.FirstOrDefault()?.OrderPrice.CurrencySymbol ?? string.Empty,
                Value = Orders.Sum(orders => orders.OrderPrice.Value),
            };
        }
    }
    public void AddOrderToGroupOrder(Order order)
    {
        Orders.Add(order);
    }
}