using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using System;
using WhatIfDemo.Common;

namespace WhatIfDemo
{
    public static class PurchaseFunction
    {
        // Allowing this dependency to be mocked in unit tests
        public static Func<HttpRequest, Task<string>> GetAccessingUserIdAsync = Helpers.GetAccessingUserIdAsync;

        // Takes an order from user and puts it to the order processing queue
        [FunctionName(nameof(Purchase))]
        [return: ServiceBus("Orders", Connection = "ServiceBusConnection")]
        public static async Task<Message> Purchase(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request,
            ILogger log)
        {
            // Extracting userId from session token
            string userId = await GetAccessingUserIdAsync(request);

            using (var reader = new StreamReader(request.Body))
            {
                dynamic requestBody = (await reader.ReadToEndAsync()).FromJson();

                // Creating an Order message
                var orderMsg = new OrderMessage
                {
                    userId = userId,
                    productId = requestBody.productId,
                    price = requestBody.price
                };

                log.LogWarning($"Got an order from userId {userId} with quoteId {requestBody.quoteId}");

                return new Message(orderMsg.ToByteArray())
                {
                    // Using quoteId as message Id, to avail from built-in message deduplication
                    MessageId = requestBody.quoteId
                };
            }
        }
    }
}