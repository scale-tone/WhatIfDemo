# WhatIfDemo-Functions.IntegrationTest
A set of integration tests for [WhatIfDemo-Functions](https://github.com/scale-tone/WhatIfDemo/tree/master/WhatIfDemo-Functions) backend.
Built with [MSTest](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest).

Can run on any machine, but is indended to be run as part of [Azure DevOps Release Pipeline](https://docs.microsoft.com/en-us/azure/devops/pipelines/release/?view=azure-devops).

Expects the following environment variables to be present:
- **AZURERMWEBAPPDEPLOYMENT_APPSERVICEAPPLICATIONURL**. Should contain your Azure Function Instance's base URL (e.g. 'https://myfunc.azurewebsites.net'). An output variable with such name is typically produced by [Deploy Azure App Service](https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/deploy/azure-rm-web-app-deployment?view=azure-devops#output-variables) pipeline task. On machines outside Azure DevOps you'll need to fill it out manually. 
- **AzureServicesAuthConnectionString**. A connection string to be used by [Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider](https://github.com/Azure/azure-sdk-for-net/blob/ddda7cb74b979f03bb03e240c06c924914ee8bdd/src/SdkCommon/AppAuthentication/Azure.Services.AppAuthentication/AzureServiceTokenProvider.cs#L16) for obtaining AAD access tokens.

