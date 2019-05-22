// using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
// using System.Net;
using System.Threading.Tasks;
// using WhatIfDemo.Common;

namespace WhatIfDemo.IntegrationTest
{
    [TestClass]
    public class IntegrationTest
    {
        public TestContext TestContext { get; set; }

        // Takes the app service URL from environment variable.
        // AZURERMWEBAPPDEPLOYMENT_APPSERVICEAPPLICATIONURL is supposed to be populated by the "Deploy Azure App Service" preceding pipeline task.
        private static string AppServiceBaseUrl
        {
            get
            {
                string url = Environment.GetEnvironmentVariable("AZURERMWEBAPPDEPLOYMENT_APPSERVICEAPPLICATIONURL");
                // Don't know why this URL can appear with http:// prefix in it
                url = url.Replace("http://", "https://");
                return url;
            }
        }

        // EasyAuth session token, obtained at the beginning of each test run
        private static string EasyAuthSessionToken;
/*
        [ClassInitialize]
        public static async Task Init(TestContext testContext)
        {
            testContext.WriteLine("### IntegrationTest.Init() 1");

            // This ctor will take the connection string from AzureServicesAuthConnectionString environment variable
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(AppServiceBaseUrl);

            testContext.WriteLine("### IntegrationTest.Init() 2");

            // Exchanging AAD access token to EasyAuth session token and saving it in a static variable
            using (var client = new WebClient())
            {
                string stringResponse = await client.UploadStringTaskAsync(AppServiceBaseUrl + Constants.AuthAadLoginEndpointUri, 
                    new { access_token = accessToken }.ToJson());

                EasyAuthSessionToken = stringResponse.FromJson().authenticationToken;
            }

            testContext.WriteLine("### IntegrationTest.Init() 1");
        }
*/
        [TestMethod]
        public async Task TestGetQuotes()
        {
            this.TestContext.WriteLine("### IntegrationTest.TestGetQuotes() started");
/*
            decimal priceBeforePurchase, priceAfterPurchase;

            using (var client = new WebClient())
            {
                client.Headers.Add(Constants.SessionTokenHeaderName, EasyAuthSessionToken);

                // Cleaning up
                await client.DownloadStringTaskAsync(AppServiceBaseUrl + "/api/Cleanup");

                // Getting quotes
                string stringResponse = await client.DownloadStringTaskAsync(AppServiceBaseUrl + "/api/GetQuotes");

                dynamic product2 = stringResponse.FromJson()[0];
                dynamic product3 = stringResponse.FromJson()[2];
                priceBeforePurchase = product3.price;

                this.TestContext.WriteLine($"Product #3 before: {product3}");

                // Need to replace quoteId with something unique, so that order messages from two consequtive test runs do not get deduplicated
                product2.quoteId = Guid.NewGuid().ToString();

                // Buying product #2
                await client.UploadStringTaskAsync(AppServiceBaseUrl + "/api/Purchase", product2.ToString());

                // Giving it some time to process the purchase
                await Task.Delay(TimeSpan.FromSeconds(10));

                // Getting quotes again and checking that discount was applied
                stringResponse = await client.DownloadStringTaskAsync(AppServiceBaseUrl + "/api/GetQuotes");

                product3 = stringResponse.FromJson()[2];
                priceAfterPurchase = product3.price;

                this.TestContext.WriteLine($"Product #3 after: {product3}");
            }

            // Validating that the discount was applied
            decimal discount = 1m - (priceAfterPurchase / priceBeforePurchase);

            this.TestContext.WriteLine($"Current discount: {discount}");
            Assert.IsTrue(discount > 0.03m && discount < 0.07m);
*/
        }
    }
}
