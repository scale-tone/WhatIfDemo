using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using SendGrid.Helpers.Mail;

namespace WhatIfDemo
{
    public static class ProcessOrderFunction
    {
        private static readonly TimeSpan ChargingPeriod = TimeSpan.FromSeconds(20);
        private static readonly TimeSpan ChargingInterval = TimeSpan.FromMinutes(2);
        private const string EmailAddressVariableName = "TestEmailAddress";

        // Processes orders from the queue
        [FunctionName(nameof(ProcessOrder))]
        public static async Task ProcessOrder(
            [ServiceBusTrigger("Orders", Connection = "ServiceBusConnection")]
                OrderMessage order,
            [OrchestrationClient]
                DurableOrchestrationClient orchestrationClient,
            ILogger log)
        {
            log.LogWarning($"######### Processing order from userId {order.userId}");

            // Here is where we instantiate a new Policy object
            var policy = new WhatIfDemoDbDataContext.Policy
            {
                id = Guid.NewGuid(),
                userId = order.userId,
                productId = order.productId,
                paymentAmount = order.price,
                dateCreated = DateTime.UtcNow
            };

            // Starting the policy processing Saga
            await orchestrationClient.StartNewAsync(nameof(Orchestrator), policy.id.ToString(), policy);
        }

        // Orchestrates policy creation and starts the charging process
        [FunctionName(nameof(Orchestrator))]
        public static async Task Orchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, 
            ILogger log)
        {
            var policy = context.GetInput<WhatIfDemoDbDataContext.Policy>();

            // Configuring retries for SavePolicyToDb activity
            var retryOptions = new RetryOptions(TimeSpan.FromSeconds(5), 3);
            policy = await context.CallActivityWithRetryAsync<WhatIfDemoDbDataContext.Policy>(nameof(SavePolicyToDb), retryOptions, policy);

            // Now let's start charging the customer via a sub-orchestration
            await context.CallSubOrchestratorAsync(nameof(SubOrchestrator), policy);
        }

        // Charges the customer periodically
        [FunctionName(nameof(SubOrchestrator))]
        public static async Task SubOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            // Waiting...
            await context.CreateTimer(context.CurrentUtcDateTime.Add(ChargingPeriod), CancellationToken.None);

            var policy = context.GetInput<WhatIfDemoDbDataContext.Policy>();

            // If the charging time interval is not expired yet
            if ((context.CurrentUtcDateTime - policy.dateCreated) < ChargingInterval)
            {
                // Sending a periodic billing email
                await context.CallActivityAsync<WhatIfDemoDbDataContext.Policy>(nameof(ChargeTheCustomer), policy);

                // Recursively calling ourselves
                context.ContinueAsNew(policy);
            }
        }

        // Stores the newly issued policy to Azure SQL
        [FunctionName(nameof(SavePolicyToDb))]
        public static async Task<WhatIfDemoDbDataContext.Policy> SavePolicyToDb(
            [ActivityTrigger] WhatIfDemoDbDataContext.Policy policy, ILogger log)
        {
            // Saving the policy to Azure SQL DB
            var ctx = new WhatIfDemoDbDataContext();
            ctx.Policies.Add(policy);
            await ctx.SaveChangesAsync();

            log.LogWarning($"######### Saved policy {policy.id} from userId {policy.userId}");

            // Returning the Policy object back, just to show this context propagation mechanism
            return policy;
        }

        // Sends a billing email to the customer
        [FunctionName(nameof(ChargeTheCustomer))]
        [return: SendGrid(ApiKey = "SendGridApiKey")]
        public static SendGridMessage ChargeTheCustomer(
            [ActivityTrigger] WhatIfDemoDbDataContext.Policy policy, ILogger log)
        {
            string emailAddress = Environment.GetEnvironmentVariable(EmailAddressVariableName);

            log.LogWarning($"Sending email to {emailAddress} (userId {policy.userId}) about policy {policy.id}");

            var message = new SendGridMessage();
            message.SetFrom(emailAddress);
            message.AddTo(emailAddress);

            message.SetSubject($"Your regular payment for policy {policy.id} is due");
            message.AddContent("text/html", $"Please, pay ${policy.paymentAmount}");

            return message;
        }
    }
}
