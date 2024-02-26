using System.Text.Json.Serialization;

namespace SnackApp.Models.ItemProperties;

public record ItemDimension
{
    [JsonPropertyName("name")]
    public string Name { get; init; }
    
    [JsonPropertyName("price")]
    public Currency Price { get; init; }
    
    [JsonPropertyName("is-mutually-exclusive")]
    public bool IsMutuallyExclusive;
    
    [JsonPropertyName("always-checked")]
    public bool IsAlwaysChecked;

    [JsonPropertyName("group-id")] 
    public string GroupId;

    [JsonPropertyName("isOptional")] 
    public bool IsOptional;
}