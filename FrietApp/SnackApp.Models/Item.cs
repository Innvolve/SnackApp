namespace SnackApp.Models;

public class Item
{
    public string Name { get; init; }
    public string? Description { get; init; } = string.Empty;
    public string ImgUrl { get; init; }
    public double Price { get; init; }
    public List<Dictionary<string,List<string>>>? Options { get; init; }
    public string? Associations { get; init; }
    public string? Dimensions { get; init; }
    public bool Availability { get; set; } = true;
}