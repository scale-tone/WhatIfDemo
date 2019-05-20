using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WhatIfDemo.Common;

namespace WhatIfDemo
{
    public static class GetQuotesFunction
    {
        // Returns a list of customized offers for specific user.
        // List of products is taken from Cosmos DB and prices are calculated based on user-specific data from Azure SQL
        [FunctionName(nameof(GetQuotes))]
        public static async Task<IEnumerable<Quote>> GetQuotes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] 
                HttpRequest request,
            // Getting all documents from Cosmos DB "Products" collection as a method parameter
            [CosmosDB(databaseName: "WhatIfDemoDb", collectionName: "Products", ConnectionStringSetting = "CosmosDBConnection", SqlQuery = "select * from c")]
                IEnumerable<Product> products,
            ILogger log)
        {
            string userId = await Helpers.GetAccessingUserIdAsync(request);

            // Loading user-specific data from Azure SQL database
            var ctx = await WhatIfDemoDbDataContext.CreateAsync();

            int policiesCount = await ctx.Policies.CountAsync(p => p.userId == userId);
            int claimsCount = await ctx.Claims.CountAsync(c => c.userId == userId);

            // Giving 10% penalty for each claim, but not more than 200%
            decimal penalty = 1M + 0.1M * claimsCount;
            penalty = penalty > 2M ? 2M : penalty;

            // Giving 5% discount for each previously bought policy, but not more than 50%
            decimal discount = 1M - 0.05M * policiesCount;
            discount = discount < 0.5M ? 0.5M : discount;

            return products.Select(p => new Quote 
            {
                productId = p.id,
                // Inferring quoteId from productId and the number of existing policies. 
                // quoteId will then be used as identifier for order message, and this is how we deduplicate them
                quoteId = p.id + "_" + policiesCount,
                title = p.title,
                price = p.price * penalty * discount
            });
        }
    }
}
