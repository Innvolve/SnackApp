using System.Text.Json.Serialization;
using SnackApp.Models.ItemProperties;

namespace SnackApp.Models;

public class Item
{
    [JsonPropertyName("slug")]
    public string Slug { get; init; }
    
    [JsonPropertyName("item-url")]
    public string ItemUrl { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; }
    
    [JsonPropertyName("price")]
    public double Price { get; init; }
    
    //Todo added jsonproperties
    public string? Description { get; init; }
    
    
    public string ImgUrl { get; init; }
    
    [JsonPropertyName("options")]
    public List<ItemOption> Options { get; init; }
    
    [JsonPropertyName("associations")]
    public List<ItemAssociation>? Associations { get; init; }
    
    [JsonPropertyName("dimensions")]
    public ItemDimension? Dimension { get; init; }
    
    [JsonPropertyName("availability")]
    public bool Availability { get; set; }
    
   
}