using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AdminPartDevelop.Common;
using AdminPartDevelop.Services.EmailsSender;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace AdminPartDevelop.Tests.ServicesTests.EmailsSenderTests
{
    public class EmailsToLoginDbSenderTest
    {
        private EmailsToLoginDbSender CreateSender(HttpResponseMessage responseMessage, out Mock<ILogger<EmailsToLoginDbSender>> loggerMock)
        {
            // Mock HTTP message handler
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(handlerMock.Object);
            loggerMock = new Mock<ILogger<EmailsToLoginDbSender>>();

            return new EmailsToLoginDbSender(httpClient, loggerMock.Object);
        }

        [Fact]
        public async Task AddEmailsToAllowedListAsync_ReturnsSuccess_WhenStatusCodeIsOK()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var sender = CreateSender(response, out var loggerMock);
            var emails = new List<string> { "test@example.com" };

            // Act
            var result = await sender.AddEmailsToAllowedListAsync(emails);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task AddEmailsToAllowedListAsync_ReturnsFalse_WhenStatusCodeIsBadRequest()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var sender = CreateSender(response, out var loggerMock);
            var emails = new List<string> { "test@example.com" };

            // Act
            var result = await sender.AddEmailsToAllowedListAsync(emails);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task AddEmailsToAllowedListAsync_ReturnsFailure_WhenExceptionThrown()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection failed"));

            var httpClient = new HttpClient(handlerMock.Object);
            var loggerMock = new Mock<ILogger<EmailsToLoginDbSender>>();
            var sender = new EmailsToLoginDbSender(httpClient, loggerMock.Object);

            var emails = new List<string> { "test@example.com" };

            // Act
            var result = await sender.AddEmailsToAllowedListAsync(emails);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Nepodařilo se uložit emaily do databáze!", result.ErrorMessage);
        }

    }
}
