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
    public static class GetAllProductsFunction
    {
        // Returns a list of all products from CosmosDB
        [FunctionName(nameof(GetAllProducts))]
        public static async Task<IEnumerable<Product>> GetAllProducts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] 
                HttpRequest request,
            // Getting all documents from Cosmos DB "Products" collection as a method parameter
            [CosmosDB(databaseName: "WhatIfDemoDb", collectionName: "Products", ConnectionStringSetting = "CosmosDBConnection", SqlQuery = "select * from c")]
                IEnumerable<Product> products,
            ILogger log)
        {
            return products;
        }
    }
}