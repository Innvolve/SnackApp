using Newtonsoft.Json;
using SnackApp.Models.ItemProperties;

namespace SnackApp.Models;

public class Item
{
    [JsonProperty("slug")]
    public string Slug { get; init; }
    
    [JsonProperty("item-url")]
    public string ItemUrl { get; init; }
    
    [JsonProperty("name")]
    public string Name { get; init; }
    
    [JsonProperty("price")]
    public double Price { get; init; }
    
    //Todo added jsonproperties
    public string? Description { get; init; }
    
    
    public string ImgUrl { get; init; }
    
    [JsonProperty("options")]
    public List<ItemOption> Options { get; init; }
    
    [JsonProperty("associations")]
    public List<ItemAssociation>? Associations { get; init; }
    
    [JsonProperty("dimensions")]
    public ItemDimension? Dimension { get; init; }
    
    [JsonProperty("availability")]
    public bool Availability { get; set; }
    
   
}