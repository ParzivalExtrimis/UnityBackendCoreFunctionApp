using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using UnityBackendCoreFunctionApp.Models;

namespace UnityBackendCoreFunctionApp.Functions {
    public static class CoreFunction {

        [FunctionName("CoreFunction")]
        public static async Task<ActionResult> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log) {

            var jsonContent = context.GetInput<string>();
            var userData = await context.CallActivityAsync<User>("Auth", jsonContent);

            log.LogInformation($"Core: Auth returned -- {userData.Email}");

            if (userData != null) {
                var storageWasInitialized =  await context.CallActivityAsync<bool>("StorageInit", userData.Email);
                if(storageWasInitialized) {
                    log.LogInformation($"Core: {userData.UserName}: containers successfully setup.");
                    //await context.CallActivityAsync<List<string>>("ContentMatcher", userData);
                    return new OkObjectResult(userData);
                }
            }
            return new BadRequestObjectResult($"{userData.UserName} could not be authenticated or Storage Initialization failed.");
        }

    }

}