using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using WhatIfDemo.Common;

namespace WhatIfDemo
{
    public static class CleanupFunction
    {
        // This Function should only accept AAD tokens, since it should only be used from integration tests
        private static string[] acceptedAuthProviders = new[] { "aad" };

        // Reverts user data to it's initial state.
        // Intended to be used from integration tests only.
        [FunctionName(nameof(Cleanup))]
        public static async Task Cleanup(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] 
                HttpRequest request,
            ILogger log)
        {
            string userId = await Helpers.GetAccessingUserIdAsync(request, acceptedAuthProviders);

            // Loading user-specific data from Azure SQL database
            var ctx = await WhatIfDemoDbDataContext.CreateAsync();

            // Removing all records for this user from SQL DB
            foreach(var policy in ctx.Policies.Where(p => p.userId == userId))
            {
                ctx.Policies.Remove(policy);
            }

            foreach (var claim in ctx.Claims.Where(p => p.userId == userId))
            {
                ctx.Claims.Remove(claim);
            }

            await ctx.SaveChangesAsync();
        }
    }
}
