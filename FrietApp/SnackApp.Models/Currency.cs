using Newtonsoft.Json;

namespace SnackApp.Models;

public struct Currency
{
    [JsonProperty("currency-symbol")]
    public string CurrencySymbol;
    
    [JsonProperty("value")]
    public double Value;
}