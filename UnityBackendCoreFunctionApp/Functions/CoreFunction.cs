using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using UnityBackendCoreFunctionApp.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace UnityBackendCoreFunctionApp.Functions;
public static class CoreFunction {

    [FunctionName("Core")]
    public static async Task<ActionResult> Core(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log) {

        var retryOptions = new RetryOptions(
            firstRetryInterval: TimeSpan.FromSeconds(4),
            maxNumberOfAttempts: 18
        );

        var jsonContent = context.GetInput<string>();
        var userData = await context.CallActivityWithRetryAsync<User>("Auth", retryOptions, jsonContent);

        log.LogWarning($"Core: Authenticated -- {userData.Email}");

        try {

            var storageInitResult = await context.CallActivityWithRetryAsync<string>("StorageInit", retryOptions, userData.Email);
            var contentMatchResult = await context.CallActivityAsync<ContentData>("ContentMatcher", userData);
            if (userData != null) {
                var init_match_results = new List<Task<bool>> {
                    storageInitResult != null
                    ? Task<bool>.FromResult(true)
                    : Task<bool>.FromResult(false),

                    contentMatchResult != null
                    ? Task<bool>.FromResult(true)
                    : Task<bool>.FromResult(false)
                };

                var results = await Task.WhenAll(init_match_results);
                bool result = results.All(b => b);
                   
                if (result) {
                    var copierInput = new CopierInput(storageInitResult, contentMatchResult);
                    if (await context.CallActivityAsync<bool>("Copier", copierInput)) {
                        return new OkObjectResult("Copy successful");
                    }
                    return new NotFoundObjectResult("Copy Failed.");
                }
            }
            return new BadRequestObjectResult($"{userData.UserName} could not be authenticated or Storage Initialization failed.");
        }
        catch (Exception ex) {
            log.LogError(ex.Message);
            return new BadRequestObjectResult("Exception thrown during execution. Something went wrong.");
        }
    }


}
