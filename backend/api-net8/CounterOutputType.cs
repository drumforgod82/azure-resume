using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Company.Function;
public class MyOutputType
{
    [CosmosDBOutput(databaseName: "AzureResume", collectionName: "Counter", ConnectionStringSetting = "CosmosDbConnectionString")]
    public Counter? UpdatedCounter { get; set; }
    public HttpResponseData? HttpResponse { get; set; }
}
