using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;

namespace WhatIfDemo
{
    // Order submitted to /Orders queue
    public class OrderMessage
    {
        public string userId { get; set; }
        public string productId { get; set; }
        public decimal price { get; set; }
    }    

    // Product document, stored in Cosmos DB
    public class Product
    {
        public string id { get; set; }
        public string title { get; set; }
        public decimal price { get; set; }
    }

    // Return value for GetQuotes function
    public class Quote
    {
        public string productId { get; set; }
        public string quoteId { get; set; }
        public string title { get; set; }
        public decimal price { get; set; }
    }

    // Entity Framework data context for data stored in Azure SQL database
    public class WhatIfDemoDbDataContext : DbContext
    {
        public class Policy
        {
            public Guid id { get; set; }
            public string userId { get; set; }
            public string productId { get; set; }
            public decimal paymentAmount { get; set; }
            public DateTime dateCreated { get; set; }
        }

        public class Claim
        {
            public Guid id { get; set; }
            public string userId { get; set; }
            public string licenseId { get; set; }
            public decimal amount { get; set; }
            public DateTime dateCreated { get; set; }
        }

        private const string ConnectionStringVariableName = "AzureSqlConnection";

        // A fabric method for creating instances of WhatIfDemoDbDataContext,
        // that can work with Managed Identities. Internally obtains an access token and uses it.
        public static async Task<WhatIfDemoDbDataContext> CreateAsync()
        {
            // Obtaining an access token for current Managed Identity. For this to work, Managed Identity should be configured for your Azure App.
            // It also works on local devboxes (although subsequently that token is not used).
            // The token is obtained by a localhost call and besides is cached internally, so 
            // there should be no performance hit.
            string azureSqlAccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");

            return new WhatIfDemoDbDataContext(azureSqlAccessToken);
        }

        // Ctor for using with ordinary connection string (with user/password)
        public WhatIfDemoDbDataContext() 
            : base(new DbContextOptionsBuilder<WhatIfDemoDbDataContext>()
                .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionStringVariableName)).Options)
        {
        }

        // Ctor for using with Managed Identities. Takes an access token, that should be obtained via AzureServiceTokenProvider.
        // Still requires a connection string to be provided via Application Settings, 
        // but that connection string _should not contain any secrets_.
        public WhatIfDemoDbDataContext(string azureSqlAccessToken)
            : base(new DbContextOptionsBuilder<WhatIfDemoDbDataContext>()
                .UseSqlServer(CreateSqlConnection(azureSqlAccessToken)).Options)
        {
        }

        // History of user purchases
        public DbSet<Policy> Policies { get; set; }

        // History of user claims
        public DbSet<Claim> Claims { get; set; }

        private static SqlConnection CreateSqlConnection(string azureSqlAccessToken)
        {
            string connString = Environment.GetEnvironmentVariable(ConnectionStringVariableName);

            var conn = new SqlConnection(connString);

            // SqlServer LocalDb doesn't support token authentication, so for local devbox we just bypass setting the AccessToken property
            if (!connString.Contains("(localdb)"))
            {
                conn.AccessToken = azureSqlAccessToken;
            }

            return conn;
        }
    }
}