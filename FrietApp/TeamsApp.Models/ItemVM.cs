using Entities;
using SnackApp.Models;

namespace SnackApp.View.Models;

public class ItemVM
{
    public string Name { get; } 
    public string? Description { get; } 
    public string ImgUrl { get; } 
    public bool Availability { get; } 
    public Currency Price { get; } 

    public ItemVM(Item item)
    {
        Name = item.Name;
        Description = item.Description;
        ImgUrl = item.ImgUrl;
        Availability = item.Availability;
        Price = item.Price;
    }
    
}