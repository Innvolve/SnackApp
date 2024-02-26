using System.Text.Json;
using System.Text.Json.Serialization;

namespace SnackApp.Models.ItemProperties;

public record ItemAssociation
{
    [JsonPropertyName("id")]
    public string Id;
    
    [JsonPropertyName("name")]
    public string Name;
    
    public string Description;
    
    public Currency Price;
    
    [JsonPropertyName("association-group-id")]
    public string GroupId;
    
    [JsonPropertyName("always-checked")]
    public bool IsAlwaysChecked;
    
    public bool IsOptional;
}