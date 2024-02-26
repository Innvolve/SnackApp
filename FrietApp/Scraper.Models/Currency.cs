using System.Text.Json.Serialization;

namespace SnackApp.Models;

public record Currency
{
    [JsonPropertyName("currency-symbol")]
    public string CurrencySymbol;
    
    [JsonPropertyName("value")]
    public double Value;
}