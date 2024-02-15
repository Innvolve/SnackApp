using Newtonsoft.Json;

namespace SnackApp.Models.ItemProperties;

public record ItemAssociation
{
    [JsonProperty("id")]
    public string Id;
    
    [JsonProperty("name")]
    public string Name;
    
    public string Description;
    
    public Currency Price;
    
    [JsonProperty("association-group-id")]
    public string GroupId;
    
    [JsonProperty("always-checked")]
    public bool IsAlwaysChecked;
    
    public bool IsOptional;
}