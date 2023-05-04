using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnityBackendCoreFunctionApp.Models;
using Microsoft.Azure.Cosmos;
using User = UnityBackendCoreFunctionApp.Models.User;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Azure.Storage.Blobs;
using System.Net;

#nullable enable

namespace UnityBackendCoreFunctionApp.Functions;
public static class ContentMatcherFunction {
    [FunctionName("ContentMatcher")]
    public static async Task<ContentData?> ContentMatcher(
        [ActivityTrigger] User user,
        ILogger log) {

        log.LogWarning($"Content matching executed at: {DateTime.Now}");

        CosmosClient client;
#if DEBUG
        var builder = new ConfigurationBuilder()
           .SetBasePath(Environment.CurrentDirectory)
           .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        var config = builder.Build();
        var connectionString = config.GetConnectionString("CosmosDBConnection");
        client = new CosmosClient(connectionString);

#else
            //connects to Storage Account via Managed Identity
            client = new CosmosClient(
                "https://pancakecosmosaccount.documents.azure.com/",
                new DefaultAzureCredential());
#endif

        Database database = client.GetDatabase(id: "Catalog");
        Container container = database.GetContainer(id: "SelectedContent");

        string studentId = user.StudentUID;

        try {
            //Find the document with the Id of Student Id and Partition key
            ItemResponse<ContentData> response = await container
            .ReadItemAsync<ContentData>(studentId, new PartitionKey(studentId));

            return (ContentData)response.Resource;
        }
        catch (CosmosException ex) {
            log.LogError("Failed to retrieve from catalog: {0} \n {1}", ex.Message, ex.InnerException);           
        }
        return null;
    }
}
