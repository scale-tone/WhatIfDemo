using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace WhatIfDemo
{
    public static class ProcessOrderFunction
    {
        // Processes orders from the queue
        [FunctionName(nameof(ProcessOrder))]
        public static void ProcessOrder(
            [ServiceBusTrigger("Orders", Connection = "ServiceBusConnection")] OrderMessage order, 
            ILogger log)
        {            
            log.LogInformation($"// Received order from: {order.userId} with productId={order.productId}");
            // To be implemented...
        }
    }
}
