using Entities;

namespace SnackApp.Models;

public class GroupOrderVM
{
    public string Owner { get; }
    public DateTime Date { get;}
    public List<OrderVM> Orders { get; }
    public Currency PriceTotal { get; }

    public GroupOrderVM(GroupOrder groupOrder)
    {
        Owner = groupOrder.Owner;
        Date = groupOrder.Date;
        Orders = new List<OrderVM>(groupOrder.Orders.Select(order => new OrderVM(order)));
        PriceTotal = groupOrder.PriceTotal;
    }
  
    public void AddOrderToGroupOrder(OrderVM order)
    {
        Orders.Add(order);
    }
}