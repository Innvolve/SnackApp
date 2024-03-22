using System.Text.Json.Serialization;
using Entities;

namespace SnackApp.Models.ItemProperties;

public record ItemOption
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("price")] 
    public Currency Price { get; init; }
    
    [JsonPropertyName("option-group-id")]
    public string GroupId { get; init; }
    
    [JsonPropertyName("is-optional")]
    public bool IsOptional { get; init; }
    
    [JsonPropertyName("is-mutually-exclusive")]
    public bool IsMutuallyExclusive { get; init; }
}