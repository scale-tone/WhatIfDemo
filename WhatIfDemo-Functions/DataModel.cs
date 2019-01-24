using System;
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
            public Guid policyId { get; set; }
            public decimal amount { get; set; }
            public DateTime dateCreated { get; set; }
        }

        private const string ConnectionStringVariableName = "AzureSqlConnection";

        public WhatIfDemoDbDataContext() 
            : base(new DbContextOptionsBuilder<WhatIfDemoDbDataContext>().UseSqlServer(Environment.GetEnvironmentVariable(ConnectionStringVariableName)).Options)
        {
        }

        // History of user purchases
        public DbSet<Policy> Policies { get; set; }

        // History of user claims
        public DbSet<Claim> Claims { get; set; }
    }
}