using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WhatIfDemo;

namespace WhatIfDemo_Functions.Test
{
    [TestClass]
    public class PurchaseFunctionTest
    {
        [TestMethod]
        public async Task CreatesPurchaseMessage()
        {
            // Arrange
            PurchaseFunction.GetAccessingUserIdAsync = async _ => "userId234";
            var logMock = new Mock<ILogger>();
            var requestMock = new Mock<HttpRequest>();

            var requestJson = JsonConvert.SerializeObject(new {
                quoteId = "quote345",
                productId = "product456",
                price = 56.78
            });
            var requestBodyStream = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));
            requestMock.SetupGet(x => x.Body).Returns(requestBodyStream);

            // Act
            var result = await PurchaseFunction.Purchase(requestMock.Object, logMock.Object);

            // Assert
            var resultMessage = result.Body.ToObject<OrderMessage>();

            Assert.AreEqual("quote345", result.MessageId);
            Assert.AreEqual("userId234", resultMessage.userId);
            Assert.AreEqual("product456", resultMessage.productId);
            Assert.AreEqual(56.78m, resultMessage.price);
        }
    }
}
