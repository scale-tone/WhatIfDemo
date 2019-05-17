using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WhatIfDemo_Functions.IntegrationTest
{
    [TestClass]
    public class IntegrationTest
    {
        public TestContext TestContext { get; set; }

        private string easyAuthSessionToken;

        [ClassInitialize]
        public void Init()
        {
        }

        [TestMethod]
        public void TestMethod()
        {
            this.TestContext.WriteLine("WhatIfDemo_Functions.IntegrationTest.TestMethod works!");
        }
    }
}
