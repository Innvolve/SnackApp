using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Models;

namespace SnackApp.Models.ItemProperties;

public record ItemAssociation
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; }
    
    [JsonPropertyName("description")]
    public string Description { get; init; }
    
    [JsonPropertyName("price")]
    public Currency Price { get; init; }
    
    [JsonPropertyName("association-group-id")]
    public string GroupId { get; init; }
    
    [JsonPropertyName("always-checked")]
    public bool IsAlwaysChecked { get; init; }
    
    [JsonPropertyName("is-optional")]
    public bool IsOptional { get; init; }
}