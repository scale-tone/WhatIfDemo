# WhatIfDemo-Functions.IntegrationTest
A set of integration tests for [WhatIfDemo-Functions](https://github.com/scale-tone/WhatIfDemo/tree/master/WhatIfDemo-Functions) backend.
Built with [MSTest](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest).

Can run on any machine, but is indended to be run as part of [Azure DevOps Release Pipeline](https://docs.microsoft.com/en-us/azure/devops/pipelines/release/?view=azure-devops).

Expects the following environment variables to be present:
- **AZURERMWEBAPPDEPLOYMENT_APPSERVICEAPPLICATIONURL**. Should contain your Azure Function Instance's base URL (e.g. 'https://myfunc.azurewebsites.net'). An output variable with such name is typically produced by [Deploy Azure App Service](https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/deploy/azure-rm-web-app-deployment?view=azure-devops#output-variables) pipeline task. On machines outside Azure DevOps you'll need to fill it out manually. 
- **AzureServicesAuthConnectionString**. A connection string to be used by [Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider](https://github.com/Azure/azure-sdk-for-net/blob/ddda7cb74b979f03bb03e240c06c924914ee8bdd/src/SdkCommon/AppAuthentication/Azure.Services.AppAuthentication/AzureServiceTokenProvider.cs#L16) for obtaining AAD access tokens. If omitted, then on Azure VMs AzureServiceTokenProvider will try to use [Managed Identities](https://docs.microsoft.com/en-us/azure/key-vault/service-to-service-authentication) and on other machines it will apply some heuristics.

To allow this integration test to make calls to your [WhatIfDemo-Functions](https://github.com/scale-tone/WhatIfDemo/tree/master/WhatIfDemo-Functions) backend, you can use a specially designated [Azure Service Principal](https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest). To do that:
1. Create a new Service Principal via Azure CLI:
```
  az ad sp create-for-rbac --name <MyIntegrationTestServicePrincipal>
```
and take a note of it's **appId** and **password** from the command output.

2. Get that newly created Service Principal's **ObjectId** (note that it is **not** the same as **appId** returned by the step1):
```
   az ad sp show --id <appId-from-step1>
```
That command outputs some JSON, from which you need to take and memorize the **objectId** value.

3. [Configure AAD authentication](https://docs.microsoft.com/en-us/azure/app-service/configure-authentication-provider-aad) for your WhatIfDemo-Functions App Service. The **Express** management mode should be enough, but you need to ensure that your Azure Function Instance's base URL is mentioned in the list of **ALLOWED TOKEN AUDIENCES**. The outcome of this step should be your AAD Application's **ClientId**. You will see this GUID on the **Azure Active Directory Settings** tab in Azure Portal, right after you configured the AAD authentication.

4. Figure out your AAD Application's Service Principal's **ObjectId**:
```
   az ad sp list --display-name <MyAppServiceNameFromStep3>
```
Note that this is a different Service Principal and a different **ObjectId**, it has nothing in common with the one from step2. Also make sure you're taking the **ObjectId** of a Service Principal (not of a Managed Identity, which might also be output by this command). Another way of obtaining this particular **ObjectId** is to navigate to Azure Active Directory **Enterprise Applications** tap in Azure Portal and find your AAD app by **ClientId** or by name there. Yes, it is very easy to get confused by all these ids... :(

5. Now you need to allow your integration test Service Principal (from step1) to access your Function's AAD application (from step3). That needs to be done with this Powershell command:
```
   New-AzureADServiceAppRoleAssignment -Id 00000000-0000-0000-0000-000000000000 -ObjectId <objectId-from-step2> -PrincipalId <objectId-from-step2-again> -ResourceId <objectId-from-step-4>
```
A zero GUID is intended, don't be surprised.

6. Now finally you can construct the **Connection String** for AzureServiceTokenProvider:
```
   RunAs=App;AppId=<appId-from-step1>;TenantId=<your-AAD-tenantId>;AppKey=<password-from-step1>
```
Save this string as a **AzureServicesAuthConnectionString** locally or as your Release Pipeline Parameter in Azure DevOps. From now on the integration test should be able to generate access tokens and call your backend with those.
