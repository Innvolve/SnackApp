using Entities;
using SnackApp.View.Models;

namespace SnackApp.Models;

public class OrderVM
{
    public string Owner { get; }

    public List<ItemVM> Items { get; }

    public Currency OrderPrice { get; }
    
    public DateTime Time { get; }
    
    public OrderVM(Order order)
    {
        Owner = order.Owner;
        Items = new List<ItemVM>(order.Items.Select(item => new ItemVM(item)));
        OrderPrice = order.OrderPrice;
        Time = order.Time;
    }
}