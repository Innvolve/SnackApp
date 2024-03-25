using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Entities;
using SnackApp.Models.ItemProperties;

namespace SnackApp.Models;

public record Item
{
    [JsonPropertyName("slug")]
    public required string Slug { get; init; }

    [JsonPropertyName("item-url")]
    public required string ItemUrl { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("price")]
    public required Currency Price { get; init; }
    
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    
    [JsonPropertyName("imgurl")]
    public required string ImgUrl { get; init; }
    
    [JsonPropertyName("options")]
    public List<ItemOption>? Options { get; init; }
    
    [JsonPropertyName("associations")]
    public List<ItemAssociation>? Associations { get; init; }
    
    [JsonPropertyName("dimensions")]
    public List<ItemDimension>? Dimensions { get; init; }
    
    [JsonPropertyName("availability")]
    public required bool Availability { get; set; }
    
   [JsonPropertyName("product-deposit")]
   public Currency? DepositValue { get; init; }
}