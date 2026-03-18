using System.Text.Json.Serialization;

namespace Company.Function;

public class Counter
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "1";

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
