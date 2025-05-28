using AdminPartDevelop.Services.RouteServices;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AdminPartDevelop.Tests.ServicesTests.RouteServicesTests
{
    public class RouteByCarPlannerTest
    {
        private static RouteByCarPlanner CreatePlannerWithResponse(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("MapyClient")).Returns(httpClient);

            var loggerMock = new Mock<ILogger<RouteByCarPlanner>>();
            return new RouteByCarPlanner(loggerMock.Object, factoryMock.Object, "dummy-key");
        }

        [Fact]
        public async Task CalculateRoute_ReturnsSuccess_WhenApiReturnsValidResponse()
        {
            string validJson = @"{ ""length"": 5000, ""duration"": 600 }";
            var planner = CreatePlannerWithResponse(validJson);

            var result = await planner.CalculateRoute(50.0f, 14.0f, 50.1f, 14.1f);

            Assert.True(result.IsSuccess);
            Assert.Equal(5, result.Data.Item1);  // km
            Assert.Equal(10, result.Data.Item2); // min
        }

        [Fact]
        public async Task CalculateRoute_ReturnsFailure_WhenHttpRequestFails()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("MapyClient")).Returns(httpClient);

            var loggerMock = new Mock<ILogger<RouteByCarPlanner>>();
            var planner = new RouteByCarPlanner(loggerMock.Object, factoryMock.Object, "dummy-key");

            var result = await planner.CalculateRoute(50.0f, 14.0f, 50.1f, 14.1f);

            Assert.False(result.IsSuccess);
            Assert.Equal("Nepodařilo se vypočítat cestu!", result.ErrorMessage);
        }

        [Fact]
        public async Task CalculateRoute_ReturnsFailure_WhenJsonIsInvalid()
        {
            string malformedJson = @"{ ""length"": 5000, "; // Invalid JSON
            var planner = CreatePlannerWithResponse(malformedJson);

            var result = await planner.CalculateRoute(50.0f, 14.0f, 50.1f, 14.1f);

            Assert.False(result.IsSuccess);
            Assert.Contains("Nepodařilo se vypočítat cestu (problémy s parsováním)!", result.ErrorMessage);
        }

        [Fact]
        public async Task CalculateRoute_ReturnsFailure_WhenJsonMissingFields()
        {
            string incompleteJson = @"{ ""duration"": 600 }"; // No length
            var planner = CreatePlannerWithResponse(incompleteJson);

            var result = await planner.CalculateRoute(50.0f, 14.0f, 50.1f, 14.1f);

            Assert.False(result.IsSuccess);
            Assert.Contains("Nepodařilo se vypočítat cestu (neznáma chyba)!", result.ErrorMessage);
        }

        [Fact]
        public async Task CalculateRoute_ReturnsFailure_OnUnexpectedException()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Unexpected"));

            var httpClient = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("GoogleMapsClient")).Returns(httpClient);

            var loggerMock = new Mock<ILogger<RouteByCarPlanner>>();
            var planner = new RouteByCarPlanner(loggerMock.Object, factoryMock.Object, "fake-api-key");

            // Act
            var result = await planner.CalculateRoute(50.0f, 14.0f, 50.1f, 14.1f);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Nepodařilo se vypočítat cestu (neznáma chyba)!", result.ErrorMessage);
        }
    }

}

