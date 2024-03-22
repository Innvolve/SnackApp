using Entities;
using SnackApp.View.Models;

namespace SnackApp.Models;

public class GroupOrderVM
{
    public string Owner { get; }
    public DateTime Date { get;}
    public List<OrderVM> Orders { get; }
    public Currency PriceTotal { get; }

    public GroupOrderVM()
    {
        Owner = "TestOwner"; //TODO: get the owner from the logged in user
        Date = DateTime.Now;
        Orders = new List<OrderVM>();
        PriceTotal =  new Currency
        {
            CurrencySymbol = Orders.FirstOrDefault()?.OrderPrice.CurrencySymbol ?? string.Empty,
            Value = Orders.Sum(item => item.OrderPrice.Value)
        };

    }
    public GroupOrderVM(GroupOrder groupOrder)
    {
        Owner = groupOrder.Owner;
        Date = groupOrder.Date;
        Orders = new List<OrderVM>(groupOrder.Orders.Select(order => new OrderVM(order)));
        PriceTotal = groupOrder.PriceTotal; //TODO: Test if this price gets updated
    }
  
    public void AddOrderToGroupOrder(OrderVM order)
    {
        Orders.Add(order);
    }
}