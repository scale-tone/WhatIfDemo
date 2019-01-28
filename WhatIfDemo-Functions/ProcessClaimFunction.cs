using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace WhatIfDemo
{
    public static class ProcessClaimFunction
    {
        private const string CognitiveServicesUriVariableName = "CognitiveServicesUri";
        private const string CognitiveServicesKeyVariableName = "CognitiveServicesKey";

        static readonly Regex LicenseNrDateRegex = new Regex("4d. (?<LicenseNr>[0-9 ]{8,13})\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static string DriversLicenseFileName = "drivers-license";
        private static int MaxCognitiveServicesTimeoutInSeconds = 10;

        // Picks up claim document scans from Azure Blob and processes them
        [FunctionName(nameof(ProcessClaim))]
        public static async Task ProcessClaim(
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

            string licenseNr = await ExtractLicenseNr(fileStream);

            log.LogWarning($"#### Extracted driving license id: " + licenseNr);
        }

        private static async Task<string> ExtractLicenseNr(Stream fileStream)
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
    }
}
