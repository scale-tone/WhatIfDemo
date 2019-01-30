# WhatIfDemo
A set of demo projects for Azure Serverless.

Demonstrates core concepts of Azure Functions, like triggers, in/out bindings, Durable Functions and authentication.
While trying to keep things simple.

- **WhatIfDemo-Angular** - SPA (single-page app) built with Angular 7 and TypeScript. Allows you to login with your Facebook account and then either buy a some insurance product or submit a claim.

- **WhatIfDemo-Functions** - Azure Functions written in C#, processing requests from WhatIfDemo-Angular. Stores data in Cosmos DB, Azure SQL and Azure Blobs, communicates with Azure Service Bus, Azure Notiication Hubs, Azure Cognitive Services and SendGrid.
