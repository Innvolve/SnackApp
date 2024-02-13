namespace SnackApp.Models;

public class Item
{
    public string Name { get; init; }
    public string? Description { get; init; } = string.Empty;
    public string ImgUrl { get; init; }
    public double Price { get; init; }
    public string? Options { get; init; }
    public string? Additives { get; init; }
    public string? Size { get; init; }
    public bool Availability { get; set; } = true;
}