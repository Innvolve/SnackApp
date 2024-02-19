using System.Text.Json.Serialization;

namespace SnackApp.Models.ItemProperties;

public record ItemOption
{
    [JsonPropertyName("id")]
    public string Id;
    
    [JsonPropertyName("name")]
    public string Name;
    
    public Currency Price;
    
    [JsonPropertyName("option-group-id")]
    public string GroupId;
    
    public bool IsOptional;
    
    public bool IsMutuallyExclusive;
}