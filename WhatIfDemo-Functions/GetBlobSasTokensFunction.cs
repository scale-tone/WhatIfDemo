using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace WhatIfDemo
{
    public static class GetBlobSasTokensFunction
    {
        private const int TokenTtlInMinutes = 5;
        private const string ConnectionStringVariableName = "AzureWebJobsStorage";
        private const string ContainerName = "claims";

        // Method response
        public class GetBlobSasTokenResponse
        {
            public string blobUri { get; set; }
            public string containerName { get; set; }
            public string folderName { get; set; }
            public string[] sasTokens { get; set; }
        }

        // Issues a time-limited SAS tokens for uploading files to Azure Blob
        [FunctionName(nameof(GetBlobSasTokens))]
        public static async Task<GetBlobSasTokenResponse> GetBlobSasTokens(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request,
            ILogger log)
        {
            // Expecting a JSON array of filenames in the request.
            // Going to generate and return individual tokens for each file.
            var fileNames = await GetFileNamesFromRequest(request);

            var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable(ConnectionStringVariableName));
            var container = storageAccount.CreateCloudBlobClient().GetContainerReference(ContainerName);

            // Just to ensure the container exists
            await container.CreateIfNotExistsAsync();

            var policy = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(TokenTtlInMinutes),
                Permissions = SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Write,

            };

            // userId will be used as a folder name. Generated tokens will be tied to that folder.
            string userId = await Helpers.GetAccessingUserIdAsync(request);

            // Generating tokens for each file
            var sasTokens = fileNames.Select(fileName => 
                {
                    var blobFolder = container.GetBlobReference($"{userId}/{fileName}");
                    return blobFolder.GetSharedAccessSignature(policy);
                });

            return new GetBlobSasTokenResponse
            {
                blobUri = storageAccount.BlobEndpoint.ToString(),
                containerName = ContainerName,
                folderName = userId,
                sasTokens = sasTokens.ToArray()
            };
        }

        private static async Task<List<string>> GetFileNamesFromRequest(HttpRequest request)
        {
            var result = new List<string>();
            using (var reader = new StreamReader(request.Body))
            {
                dynamic requestJson = JsonConvert.DeserializeObject(await reader.ReadToEndAsync());

                // expecting the request body to be an array of strings
                foreach (dynamic fileName in requestJson)
                {
                    result.Add(fileName.ToString());
                }
            }
            return result;
        }
    }
}
