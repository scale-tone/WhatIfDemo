using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WhatIfDemo
{
    public static class GetBlobSasTokenFunction
    {
        private const int TokenTtlInMinutes = 5;
        private const string ConnectionStringVariableName = "AzureWebJobsStorage";
        private const string ContainerName = "claims";

        // Method response
        public class GetBlobSasTokenResponse
        {
            public string blobUri { get; set; }
            public string sasToken { get; set; }
        }

        // Issues a time-limited SAS token for uploading files to Azure Blob
        [FunctionName(nameof(GetBlobSasToken))]
        public static async Task<GetBlobSasTokenResponse> GetBlobSasToken(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable(ConnectionStringVariableName));
            var container = storageAccount.CreateCloudBlobClient().GetContainerReference(ContainerName);

            // Just to ensure the container exists
            await container.CreateIfNotExistsAsync();

            var policy = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(TokenTtlInMinutes),
                Permissions = SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Write
            };

            return new GetBlobSasTokenResponse
            {
                blobUri = storageAccount.BlobEndpoint.ToString(),
                sasToken = container.GetSharedAccessSignature(policy)
            };
        }
    }
}
