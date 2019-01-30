# WhatIfDemo-Functions

A set of Azure Functions written in C# and demonstrating some of the core concepts of Azure Functions: different types of triggers, in/out bindings, Durable Functions and authentication.
While trying to keep things simple.

- [GetQuotes](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/GetQuotesFunction.cs) - combines documents stored in Cosmos DB with user-specific information from Azure SQL database to calculate a personalized pricelist.
- [Purchase](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/PurchaseFunction.cs) - receives an order and puts it into Azure Service Bus queue for further processing.
- [ProcessOrder](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/ProcessOrderFunction.cs) - processes orders from Azure Service Bus queue. Adds a record to Azure SQL table and then starts a process of sending periodic emails via SendGrid.
- [ProcessClaim](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/ProcessClaimFunction.cs) - picks up a newly uploaded image file, tries to extract a driving license number from it with Azure Cognitive Services, then waits for an approval event (an HTTP request) and finally writes data to Azure SQL table.
- [GetBlobSasTokenFunction](https://github.com/scale-tone/WhatIfDemo/blob/master/WhatIfDemo-Functions/GetBlobSasTokenFunction.cs) - generates a temporary write-only SAS token for uploading files to Azure Blob.


