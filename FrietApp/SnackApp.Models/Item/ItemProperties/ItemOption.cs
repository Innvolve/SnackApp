using Newtonsoft.Json;

namespace SnackApp.Models.ItemProperties;

public record ItemOption
{
    [JsonProperty("id")]
    public string Id;
    
    [JsonProperty("name")]
    public string Name;
    
    public Currency Price;
    
    [JsonProperty("option-group-id")]
    public string GroupId;
    
    public bool IsOptional;
    
    public bool IsMutuallyExclusive;
}