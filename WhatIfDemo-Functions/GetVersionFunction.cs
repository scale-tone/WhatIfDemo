using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using System;
using System.Reflection;

namespace WhatIfDemo
{
    public static class GetVersionFunction
    {
        // Returns version of currently executing WhatIfDemo-Functions.dll
        [FunctionName(nameof(GetVersion))]
        public static string GetVersion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest request,
            ILogger log)
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
