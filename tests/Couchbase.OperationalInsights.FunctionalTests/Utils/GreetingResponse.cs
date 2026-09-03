using System.Text.Json.Serialization;

namespace Couchbase.OperationalInsightsClient.FunctionalTests.Utils;

public class GreetingResponse
{
    [JsonPropertyName("greeting")]
    public string? Greeting { get; set; }
}
