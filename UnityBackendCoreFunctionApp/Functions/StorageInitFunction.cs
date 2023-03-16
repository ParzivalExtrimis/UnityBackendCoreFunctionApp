using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using Azure.Storage.Blobs;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using UnityBackendCoreFunctionApp.Models;
using Azure.Identity;

namespace UnityBackendCoreFunctionApp.Functions {
    public static class StorageInitFunction {
        [FunctionName("StorageInit")]
        public static async Task<string> StorageInit(
           [ActivityTrigger]
           string username,
           ILogger log) {
            log.LogWarning($"Storage Init executed at: {DateTime.Now}");

            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(Environment.CurrentDirectory)
            //    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
            //var config = builder.Build();
            //var connectionString = config.GetConnectionString("CloudStorageAccount");
            //BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            BlobServiceClient blobServiceClient = new BlobServiceClient(
                new Uri("https://unityaddressablestorage.blob.core.windows.net/"),
                new DefaultAzureCredential());

            var containerClient = await CreateContainerAsync(blobServiceClient, username, log);
         
            if(containerClient != null) {
                return containerClient.Name;
            }
            return null;
        }

        private static async Task<BlobContainerClient> CreateContainerAsync(BlobServiceClient blobServiceClient, string containerName, ILogger log) {

            var clock = new Stopwatch();
            string pattern = @"[^a-zA-Z0-9]+";
            string replacement = "-";

            string cleanedContanierName = Regex.Replace(containerName, pattern, replacement);
            ////If container exists delete container  [TODO: Call cleaner function before execution]
            //var prematureContainerClient = blobServiceClient.GetBlobContainerClient(cleanedContanierName);
            //clock.Start();
            //while (await prematureContainerClient.ExistsAsync()) {
            //    prematureContainerClient.SetAccessPolicy(PublicAccessType.None);
            //    if (await prematureContainerClient.DeleteIfExistsAsync()) {
            //        log.LogWarning("Premature container existed and was cleaned. Proceeding...");
            //        if (clock.ElapsedMilliseconds > 45000) break;
            //    }
            //}

            cleanedContanierName = "con-" + new Random().Next() + "-" + new Random().Next();

            // Create the container
            BlobContainerClient container = await blobServiceClient.CreateBlobContainerAsync(cleanedContanierName);

            if (await container.ExistsAsync()) {
                log.LogWarning($"***************************************************************** \n Created container {container.Name} \n *****************************************************************");
                return container;
            }
            else {
                return null;
            }
        }
    }
}
