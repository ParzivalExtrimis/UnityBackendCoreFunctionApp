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

namespace UnityBackendCoreFunctionApp.Functions;
public static class ContentMatcherFunction {
    [FunctionName("ContentMatcher")]
    public static async Task<List<string>> ContentMatcher(
        [ActivityTrigger] User user,
        ILogger log) {

        log.LogInformation($"Content matching executed at: {DateTime.Now}");

        var builder = new ConfigurationBuilder()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        var config = builder.Build();
        var connectionString = config.GetConnectionString("CosmosDBConnection");

        using CosmosClient client = new(connectionString);

        Database database = client.GetDatabase(id: "Catalog");
        Container container = database.GetContainer(id: "BatchContent");

        string batch = user.Grade + "A";
        string grade = user.Grade;
        string school = user.School;

        // Create query using a SQL string and parameters
        var query = new QueryDefinition(
            query: "SELECT * FROM BatchContent x WHERE x.batch = @batch AND x.grade = @grade AND x.school = @school"
        )
          .WithParameter("@batch", batch)
          .WithParameter("@grade", grade)
          .WithParameter("@school", school);
        //parameters not being accepted something about string objects being used. Raw strings work.

        using FeedIterator<ContentData> feed = container.GetItemQueryIterator<ContentData>(
            queryDefinition: query
        );
        log.LogInformation($"query : {query.QueryText}");
        while (feed.HasMoreResults) {
            FeedResponse<ContentData> response = await feed.ReadNextAsync();
            foreach (ContentData contentData in response) {
                log.LogInformation($"Found item:\t{contentData.batch}");
                if (contentData == null) {
                    continue;
                }
                return contentData.content;
            }
        }
        return null;
    }
}
