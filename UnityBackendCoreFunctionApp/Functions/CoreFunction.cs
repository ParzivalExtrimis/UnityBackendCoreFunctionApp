using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using UnityBackendCoreFunctionApp.Models;

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

        log.LogInformation($"Core: Auth returned -- {userData.Email}");

        try {
            if (userData != null) {
                var storageWasInitialized = await context.CallActivityWithRetryAsync<bool>("StorageInit", retryOptions, userData.Email);
                if (storageWasInitialized) {
                   log.LogInformation($"Core: {userData.UserName}: containers successfully setup.");
                }
                var contentList = await context.CallActivityAsync<List<string>>("ContentMatcher", userData);
                if (contentList == null) {
                    return new NotFoundObjectResult("Setup Phase completed. Content Matcher could not find user data.");
                }
                log.LogInformation("Content Matching successfully completed.");
                return new OkObjectResult(contentList);
            }
            return new BadRequestObjectResult($"{userData.UserName} could not be authenticated or Storage Initialization failed.");
        }
        catch (Exception ex) {
            log.LogError(ex.Message);
            return new BadRequestObjectResult("Exception thrown during execution. Something went wrong.");
        }
    }  

    
}
