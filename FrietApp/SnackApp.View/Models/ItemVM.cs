using SnackApp.Models;

namespace SnackApp.View.Models;

public class ItemVM(Item item)
{
    public string Name { get; } = item.Name;
    public string? Description { get; } = item.Description;
    public string ImgUrl { get; } = item.ImgUrl;
    public bool Availability { get; } = item.Availability;
}