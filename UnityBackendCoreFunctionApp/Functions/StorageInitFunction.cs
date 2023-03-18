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

            BlobServiceClient blobServiceClient;

#if DEBUG
            var builder = new ConfigurationBuilder()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
            var config = builder.Build();
            var connectionString = config.GetConnectionString("CloudStorageAccount");
            blobServiceClient = new BlobServiceClient(connectionString);

#else
            //connects to Storage Account via Managed Identity
            blobServiceClient = new BlobServiceClient(
                new Uri("https://unityaddressablestorage.blob.core.windows.net/"),
                new DefaultAzureCredential());
#endif

            var containerClient = await CreateContainerAsync(blobServiceClient, username, log);
         
            if(containerClient != null) {
                return containerClient.Name;
            }
            return null;
        }

        private static async Task<BlobContainerClient> CreateContainerAsync(BlobServiceClient blobServiceClient, string containerName, ILogger log) {

            string pattern = @"[^a-zA-Z0-9]+";
            string replacement = "-";

            string cleanedContanierName = Regex.Replace(containerName, pattern, replacement);
            //If container exists delete container  [TODO: Call cleaner function before execution]
            var prematureContainerClient = blobServiceClient.GetBlobContainerClient(cleanedContanierName);
           
            try {
                if (await prematureContainerClient.ExistsAsync()) {
                    foreach (var blob in prematureContainerClient.GetBlobs()) {
                        await prematureContainerClient.DeleteBlobIfExistsAsync(blob.Name, new DeleteSnapshotsOption());
                    }
                    return prematureContainerClient;
                }
                else {
                    // Create the container
                    BlobContainerClient container = await blobServiceClient.CreateBlobContainerAsync(cleanedContanierName);
                    if (await container.ExistsAsync()) {
                        log.LogWarning($"***************************************************************** \n Created container {container.Name} \n *****************************************************************");
                        return container;
                    }
                }
            }
            catch(Exception E) {
                log.LogError(E.Message);
            }
            return null;
        }
    }
}
