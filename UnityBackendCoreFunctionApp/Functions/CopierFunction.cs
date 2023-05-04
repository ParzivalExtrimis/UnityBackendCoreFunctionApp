using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using UnityBackendCoreFunctionApp.Models;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using Azure.Identity;

namespace UnityBackendCoreFunctionApp.Functions {
    public class CopierFunction {
        [FunctionName("Copier")]
        public static async Task<bool> Copier(
        [ActivityTrigger] CopierInput input,
        ILogger log) {

            //check container client is not null or expired
            if (input.container == null) {
                log.LogWarning($"Container Cient: {input.container} is null.");
                return false;
            }

            //check content list is not empty
            if (input.data == null || input.data.Chapters.Count <= 0) {
                log.LogWarning($"Container Cient: {input.container} - ContentList is null or empty.");
                return false;
            }
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
                new Uri("https://pancakestorageaccount.blob.core.windows.net/"),
                new DefaultAzureCredential());
#endif

            var storageClient = blobServiceClient.GetBlobContainerClient("content-storage");

            var userGlobalMeta = JsonConvert.SerializeObject(input.data);
            try {
                var userContainerClient = blobServiceClient.GetBlobContainerClient(input.container);
                foreach (string chapterName in input.data.Chapters) {
                    var outputBlobs = await SearchBlobsByRegexAsync(storageClient, $".*{chapterName}.*");
                    foreach (var blob in outputBlobs) {
                        log.LogWarning($"Blobs Selected: {blob.Name}");
                        var blobClient = storageClient.GetBlobClient(blob.Name);

                        var memoryStream = new MemoryStream();
                        await blobClient.DownloadToAsync(memoryStream);
                        memoryStream.Position = 0;
                        userContainerClient.UploadBlob(blob.Name, memoryStream);
                    }
                }
                userContainerClient.UploadBlob("global-content-meta-" + input.container + ".json", BinaryData.FromString(userGlobalMeta));
            }
            catch(Exception e) {
                log.LogError($"Copier: Upload Blob failed \n {e.Message}");
            }
          
            return true;
        }

        public static async Task<IEnumerable<BlobItem>> SearchBlobsByRegexAsync(BlobContainerClient containerClient, string regexPattern) {
            //returning meta data probably and not atual blobs. Investigate
            var blobItems = containerClient.GetBlobsAsync(BlobTraits.Metadata, BlobStates.All);

            var matchingBlobs = new List<BlobItem>();
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            await foreach (var blobItem in blobItems) {

                if (regex.IsMatch(blobItem.Name)) {
                    matchingBlobs.Add(blobItem);
                }
            }

            return matchingBlobs;
        }
    }
}
