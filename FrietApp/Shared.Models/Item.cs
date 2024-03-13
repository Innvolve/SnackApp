using System.Text.Json.Serialization;
using SnackApp.Models.ItemProperties;

namespace SnackApp.Models;

public record Item
{
    [JsonPropertyName("slug")]
    public string Slug { get; init; }
    
    [JsonPropertyName("item-url")]
    public string ItemUrl { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; }
    
    [JsonPropertyName("price")]
    public Currency Price { get; init; }
    
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    
    [JsonPropertyName("imgurl")]
    public string ImgUrl { get; init; }
    
    [JsonPropertyName("options")]
    public List<ItemOption> Options { get; init; }
    
    [JsonPropertyName("associations")]
    public List<ItemAssociation>? Associations { get; init; }
    
    [JsonPropertyName("dimensions")]
    public List<ItemDimension>? Dimensions { get; init; }
    
    [JsonPropertyName("availability")]
    public bool Availability { get; set; }
    
   [JsonPropertyName("product-deposit")]
   public Currency DepositValue { get; init; }
}