# WhatIfDemo-Functions

2. Two


11. Eleven


100. Hundred
101. Hundred and one

A set of Azure Functions written in C# and demonstrating some of the core concepts: different types of triggers, in/out bindings, Durable Functions and authentication.
While trying to keep things simple.

- [GetQuotes](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/GetQuotesFunction.cs) - combines documents stored in Cosmos DB with user-specific information from Azure SQL database to calculate a personalized pricelist.
- [Purchase](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/PurchaseFunction.cs) - receives an order and puts it into Azure Service Bus queue for further processing.
- [ProcessOrder](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/ProcessOrderFunction.cs) - processes orders from Azure Service Bus queue. Adds a record to Azure SQL table and then starts a process of sending periodic emails via SendGrid.
- [ProcessClaim](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/ProcessClaimFunction.cs) - picks up a newly uploaded image file, tries to extract a driving license number from it with Azure Cognitive Services, then sends a push notification via Azure Notification Hubs, waits for an approval event (an HTTP request) and finally writes data to Azure SQL table.
- [GetBlobSasTokenFunctions](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/GetBlobSasTokenFunctions.cs) - generates temporary write-only SAS tokens for uploading files to Azure Blob.

Expects the following Application Settings to be present in your Azure Function App's configuration:

- **AzureWebJobsStorage** - connection string to Azure Storage account to be used.
- **CosmosDBConnection** - connection string to Azure Cosmos DB account to be used. Expects a **WhatIfDemoDb** database with a **Products** collection to be created there. The collection should contain some [**Product**](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/DataModel.cs#L15) documents.
- **AzureSqlConnection** - connection string to an Azure SQL database. 
	[**Policies**](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/DataModel.cs#L34) and [**Claims**](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/DataModel.cs#L43) tables should be created there. [A SQL script for creating them is included](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/WhatIfDemoDb.sql).
	Current code expects [Azure Managed Identity](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview) to be configured for your Function App, so there should be no passwords in this connection string.
	To make it work:
	1) [Enable system-assigned Managed Identity for your Function App](https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity).
	2) Execute this SQL against your Azure SQL DB (this adds your managed identity to db_owner role):
		```
		CREATE USER [<your azure app instance name>] FOR EXTERNAL PROVIDER;
		ALTER ROLE db_owner ADD MEMBER [<your azure app instance name>];

		```
- **ServiceBusConnection** - connection string to Azure Service Bus account to be used.
- **SendGridApiKey** - API key for [SendGrid](https://sendgrid.com/). Register and get a free tier.
- **TestEmailAddress** - email address where test emails will be sent to. Put your own email there, don't put anyone else's.
- **CognitiveServicesUri** - URI to [Azure Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/) optical character recognition endpoint. Typically looks like "https://<your-region>.api.cognitive.microsoft.com/vision/v2.0/recognizeText?mode=Printed".
- **CognitiveServicesKey** - secret key for your [Azure Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/) account.
- **NotificationHubConnection** - connection string to your Azure Notification Hubs account.
- **NotificationHubPath** - name of your Azure Notification Hubs namespace.
  
  

Once deployed, [Facebook authentication needs to be configured](https://docs.microsoft.com/en-us/azure/app-service/configure-authentication-provider-facebook) in your Function App.
  
