using System.Text.Json.Serialization;

namespace SnackApp.Models;

public struct Currency
{
    [JsonPropertyName("currency-symbol")]
    public string CurrencySymbol;
    
    [JsonPropertyName("value")]
    public double Value;
}