using System.Text.Json.Serialization;

namespace Shared.Models;

public record Currency
{
    [JsonPropertyName("currency-symbol")]
    public string CurrencySymbol { get; init; }
    
    [JsonPropertyName("value")]
    public double Value { get; init; }
    
    public string CurrencyString => CurrencySymbol + Value;
}