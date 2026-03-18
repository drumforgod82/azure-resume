using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public sealed class GetResumeCounter
{
    private readonly ILogger<GetResumeCounter> _logger;

    public GetResumeCounter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GetResumeCounter>();
    }

    [Function("GetResumeCounterJamesDean")]
    public async Task<CounterResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "GetResumeCounter")] HttpRequestData req,
        [CosmosDBInput(
            databaseName: "AzureResume",
            containerName: "Counter",
            Connection = "AzureResumeConnectionString",
            Id = "1",
            PartitionKey = "1")]
        Counter? counter)
    {
        _logger.LogInformation("Processing resume counter request.");

        var updatedCounter = IncrementCounter(counter);
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(updatedCounter));

        return new CounterResponse
        {
            UpdatedCounter = updatedCounter,
            HttpResponse = response
        };
    }

    public static Counter IncrementCounter(Counter? counter)
    {
        var updatedCounter = counter ?? new Counter();
        updatedCounter.Count += 1;
        return updatedCounter;
    }
}
