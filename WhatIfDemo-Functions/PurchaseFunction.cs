using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Microsoft.Azure.ServiceBus.InteropExtensions;

namespace WhatIfDemo
{
    public static class PurchaseFunction
    {
        // Takes an order from user and puts it to the order processing queue
        [FunctionName(nameof(Purchase))]
        [return: ServiceBus("Orders", Connection = "ServiceBusConnection")]
        public static async Task<Message> Purchase(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request,
            ILogger log)
        {
            // Extracting userId from session token
            string userId = await Helpers.GetAccessingUserId(request);

            using (var reader = new StreamReader(request.Body))
            {
                dynamic requestJson = JsonConvert.DeserializeObject(await reader.ReadToEndAsync());

                // Creating an Order message
                var orderMsg = new OrderMessage
                {
                    userId = userId,
                    productId = requestJson.productId,
                    price = requestJson.price
                };

                log.LogWarning($"Got an order from userId {userId} with quoteId {requestJson.quoteId}");

                return new Message(orderMsg.ToByteArray())
                {
                    // Using quoteId as message Id, to avail from built-in message deduplication
                    MessageId = requestJson.quoteId
                };
            }
        }

        // Does message binary serialization
        private static byte[] ToByteArray<T>(this T msg)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractBinarySerializer<T>.Instance.WriteObject(stream, msg);
                return stream.ToArray();
            }
        }
    }
}
