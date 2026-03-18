using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Company.Function;

public sealed class CounterResponse
{
    [CosmosDBOutput(
        databaseName: "AzureResume",
        containerName: "Counter",
        Connection = "AzureResumeConnectionString")]
    public Counter UpdatedCounter { get; init; } = new();

    public HttpResponseData HttpResponse { get; init; } = default!;
}
