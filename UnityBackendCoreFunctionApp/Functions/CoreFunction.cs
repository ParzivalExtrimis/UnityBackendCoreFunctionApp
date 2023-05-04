using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using UnityBackendCoreFunctionApp.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace UnityBackendCoreFunctionApp.Functions;
public static class CoreFunction {

    [FunctionName("Core")]
    public static async Task<string> Core(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log) {

        var retryOptions = new RetryOptions(
            firstRetryInterval: TimeSpan.FromSeconds(4),
            maxNumberOfAttempts: 18
        );

        var jsonContent = context.GetInput<string>();
        var userData = await context.CallActivityWithRetryAsync<User>("Auth", retryOptions, jsonContent);

        try {
            if (userData != null) {
                Result result = new Result();
                var storageInitResult = await context.CallActivityWithRetryAsync<string>("StorageInit", retryOptions, userData.StudentUID);
                var contentMatchResult = await context.CallActivityAsync<ContentData>("ContentMatcher", userData);

                result.location = storageInitResult;
                result.data = contentMatchResult;
                log.LogWarning($"Core: Authenticated -- {userData.Name}");

                var init_match_results = new List<Task<bool>> {
                    storageInitResult != null
                    ? Task<bool>.FromResult(true)
                    : Task<bool>.FromResult(false),

                    contentMatchResult != null
                    ? Task<bool>.FromResult(true)
                    : Task<bool>.FromResult(false)
                };

                var results = await Task.WhenAll(init_match_results);
                bool isSuccess = results.All(b => b);
                   
                if (isSuccess) {
                    var copierInput = new CopierInput(storageInitResult, contentMatchResult);
                    if (await context.CallActivityAsync<bool>("Copier", copierInput)) {
                        result.state = "Copy Successful";
                    }
                    else {
                        result.state = "Copy Failed";
                    }
                }
                log.LogWarning($"Core Executed Successfully -- ({userData.Name}), ({userData.StudentUID})");
                return JsonConvert.SerializeObject(result);
            }
            return new BadRequestObjectResult($"User could not be authenticated.").ToString();
        }
        catch (Exception ex) {
            log.LogError(ex.Message);
            return new BadRequestObjectResult("Exception thrown during execution. Something went wrong.").ToString();
        }
    }


}
