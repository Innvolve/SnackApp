using System.Text.Json.Serialization;
using Entities;

namespace SnackApp.Models.ItemProperties;

public record ItemDimension
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("price")]
    public Currency Price { get; init; }

    [JsonPropertyName("is-mutually-exclusive")]
    public bool IsMutuallyExclusive { get; init; }
    
    [JsonPropertyName("always-checked")]
    public bool IsAlwaysChecked { get; init; }

    [JsonPropertyName("group-id")] 
    public string GroupId { get; init; }

    [JsonPropertyName("isOptional")] 
    public bool IsOptional { get; init; }
}