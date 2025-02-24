using Newtonsoft.Json;

namespace Functions.Article;

public class Protection
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("level")]
    public string Level { get; set; }

    [JsonProperty("expiry")]
    public string Expiry { get; set; }
}