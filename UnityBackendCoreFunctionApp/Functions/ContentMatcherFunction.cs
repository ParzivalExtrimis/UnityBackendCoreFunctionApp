using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnityBackendCoreFunctionApp.Models;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace UnityBackendCoreFunctionApp.Functions;
public static class ContentMatcherFunction {
    [FunctionName("ContentMatcher")]
    public static async Task<IActionResult> ContentMatcher(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
        [CosmosDB(
        databaseName: "Catalog",
        containerName: "BatchContent",
        Connection = "CosmosDBConnection",
        Id = "{Query.Id}",
        PartitionKey = "{Query.Grade}"
        )] ContentData contentData,
        ILogger log) {

        log.LogInformation($"Content matching executed at: {DateTime.Now}");
        log.LogInformation($"Content returned: \n {contentData}");


        if(contentData == null) {
            return new NotFoundResult();
        }
        return new OkObjectResult(contentData);

    }
}
