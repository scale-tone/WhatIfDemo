using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WhatIfDemo
{
    public static class ProcessClaimFunction
    {
        private const string CognitiveServicesUriVariableName = "CognitiveServicesUri";
        private const string CognitiveServicesKeyVariableName = "CognitiveServicesKey";
        private const string NotificationHubConnectionVariableName = "NotificationHubConnection";
        private const string NotificationHubPathVariableName = "NotificationHubPath";

        static readonly Regex LicenseNrDateRegex = new Regex("4d. (?<LicenseNr>[0-9 ]{8,13})\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static string DriversLicenseFileName = "drivers-license";
        private static int MaxCognitiveServicesTimeoutInSeconds = 10;

        // Picks up claim document scans from Azure Blob and processes them
        [FunctionName(nameof(ProcessClaim))]
        public static async Task ProcessClaim(
            [OrchestrationClient] DurableOrchestrationClient orchestrationClient,
            [BlobTrigger("claims/{fileFullName}", Connection = "AzureWebJobsStorage")] Stream fileStream, 
            string fileFullName, 
            ILogger log)
        {
            // Expecting the file name be in form '<userId>/<fileName>'
            var fileNameParts = fileFullName.Split('/');
            if(fileNameParts.Length != 2)
            {
                return;
            }

            string userId = fileNameParts[0];
            string fileName = fileNameParts[1];

            if(fileName != DriversLicenseFileName)
            {
                return;
            }

            // Analyzing fileStream before starting a Saga, since fileStream isn't serializable
            // and since there's a built-in retry logic for BlobTriggers anyway.
            string licenseId = await ExtractLicenseId(fileStream);

            log.LogWarning($"Extracted driving license id {licenseId}, starting claim processing for user {userId}");

            var claim = new WhatIfDemoDbDataContext.Claim
            {
                id = Guid.NewGuid(),
                userId = userId,
                licenseId = licenseId,
                dateCreated = DateTime.UtcNow,
                amount = new Random().Next(1, 1000)
            };

            // Starting the claim processing Saga
            await orchestrationClient.StartNewAsync(nameof(ProcessClaimOrchestrator), claim.id.ToString(), claim);
        }

        // Orchestrates claim processing
        [FunctionName(nameof(ProcessClaimOrchestrator))]
        public static async Task ProcessClaimOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            var claim = context.GetInput<WhatIfDemoDbDataContext.Claim>();

            // Sending mobile notification
            await context.CallActivityAsync(nameof(SendMobileNotification), claim);

            // Waiting for the claim to be approved
            await context.WaitForExternalEvent(nameof(ApproveClaim));

            // Saving the claim to DB
            var retryOptions = new RetryOptions(TimeSpan.FromSeconds(5), 3);
            await context.CallActivityWithRetryAsync<WhatIfDemoDbDataContext.Claim>(nameof(SaveClaimToDb), retryOptions, claim);

            log.LogWarning($"Claim {claim.id} processing finished!");
        }

        // Sends a GCM notification to operator's phone
        [FunctionName(nameof(SendMobileNotification))]
        public static async Task SendMobileNotification(
            [ActivityTrigger] WhatIfDemoDbDataContext.Claim claim,
            ILogger log)
        {
            // Notifications binding is not yet available for Functions 2.0, unfortunately.
            // So just using NotificationHubClient manually.
            var hub = NotificationHubClient.CreateClientFromConnectionString(Environment.GetEnvironmentVariable(NotificationHubConnectionVariableName), Environment.GetEnvironmentVariable(NotificationHubPathVariableName));

            var payload = new
            {
                message = $"Click to approve claim {claim.id} from user {claim.userId} with license {claim.licenseId}",
                uri = $"{Helpers.GetHostName()}/api/{nameof(ApproveClaim)}?claimId={claim.id}"
            };
            string fcmNotificationJson = $"{{\"data\":{payload.ToJson()}}}";

            await hub.SendNotificationAsync(new FcmNotification(fcmNotificationJson));

            log.LogWarning("GCM Notification sent!");
            log.LogWarning(fcmNotificationJson);
        }

        // Receives an approval via HTTP GET
        // WARNING: in production a method like that should of course be protected! E.g. with AuthorizationLevel.Function
        [FunctionName(nameof(ApproveClaim))]
        public static async Task<string> ApproveClaim(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient orchestrationClient,
            ILogger log)
        {
            string claimId = req.Query[nameof(claimId)];

            await orchestrationClient.RaiseEventAsync(claimId, nameof(ApproveClaim));

            return $"Claim {claimId} approved successfully";
        }

        // Stores the claim in Azure SQL
        [FunctionName(nameof(SaveClaimToDb))]
        public static async Task SaveClaimToDb(
            [ActivityTrigger] WhatIfDemoDbDataContext.Claim claim,
            ILogger log)
        {
            var ctx = new WhatIfDemoDbDataContext();
            ctx.Claims.Add(claim);

            await ctx.SaveChangesIdempotentlyAsync(ex => {
                // Explicitly handling the case of duplicated execution.
                // Which might happen, if the process crashes or restarts.
                log.LogError($"Failed to add policy {claim.id} from userId {claim.userId}: {ex.Message}");
            });

            log.LogWarning($"Saved claim {claim.id} from userId {claim.userId}");
        }

        private static async Task<string> ExtractLicenseId(Stream fileStream)
        {
            string cognitiveServicesKey = Environment.GetEnvironmentVariable(CognitiveServicesKeyVariableName);
            string cognitiveServicesUri = Environment.GetEnvironmentVariable(CognitiveServicesUriVariableName);

            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/octet-stream");
                client.Headers.Add("Ocp-Apim-Subscription-Key", cognitiveServicesKey);

                // Could use WebClient.OpenWrite() instead, but in that case ResponseHeaders property isn't available :(
                await client.UploadDataTaskAsync(cognitiveServicesUri, await fileStream.ToArrayAsync());

                var recognitionResultUri = client.ResponseHeaders["Operation-Location"];

                // Now waiting for result to be available
                int i = 0;
                string recognitionResultJson;
                do
                {
                    if (i++ > MaxCognitiveServicesTimeoutInSeconds)
                    {
                        throw new InvalidDataException("Timed out waiting for Cognitive Services!");
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    recognitionResultJson = await client.DownloadStringTaskAsync(recognitionResultUri);
                }
                while (((dynamic)JObject.Parse(recognitionResultJson)).status != "Succeeded");

                var match = LicenseNrDateRegex.Match(recognitionResultJson);
                if (match.Success)
                {
                    return match.Groups["LicenseNr"].Value;
                }
                else
                {
                    throw new InvalidDataException("Failed to extract license number!");
                }
            }
        }

        private static async Task<byte[]> ToArrayAsync(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static string ToJson(this object o)
        {
            return JsonConvert.SerializeObject(o);
        }
    }
}
