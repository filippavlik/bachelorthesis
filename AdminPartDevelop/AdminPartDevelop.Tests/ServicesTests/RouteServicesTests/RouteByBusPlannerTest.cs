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
    public class RouteByBusPlannerTest
    {
        [Fact]
        public async Task CalculateRoute_ReturnsSuccess_WhenGoogleApiReturnsValidResponse()
        {
            // Arrange
            var jsonResponse = @"
                {
                  ""status"": ""OK"",
                  ""routes"": [{
                    ""legs"": [{
                      ""distance"": { ""value"": 12000 },
                      ""duration"": { ""value"": 1800 }
                    }]
                  }]
                }";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(f => f.CreateClient("GoogleMapsClient"))
                .Returns(httpClient);

            var loggerMock = new Mock<ILogger<RouteByBusPlanner>>();
            var planner = new RouteByBusPlanner(loggerMock.Object, httpClientFactoryMock.Object, "fake-api-key");

            float startLat = 50.0f, startLon = 14.0f, endLat = 50.1f, endLon = 14.1f;

            // Act
            var result = await planner.CalculateRoute(startLat, startLon, endLat, endLon);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(12, result.Data.Item1);      // 12000m = 12km
            Assert.Equal(30, result.Data.Item2);      // 1800s = 30min
        }
        [Fact]
        public async Task CalculateRoute_ReturnsFailure_WhenGoogleApiReturnsNonOkStatus()
        {
            // Arrange
            var response = @"{ ""status"": ""ZERO_RESULTS"" }";
            var planner = CreatePlannerWithResponse(response);

            // Act
            var result = await planner.CalculateRoute(50.0f, 14.0f, 50.1f, 14.1f);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Nepodařilo se vypočítat cestu!", result.ErrorMessage);
        }
        [Fact]
        public async Task CalculateRoute_ReturnsFailure_OnMalformedJson()
        {
            // Arrange
            var malformedJson = @"{ ""status"": ""OK"", ""routes"": [ { ""legs"": [ {} ] } ]"; // missing end brackets
            var planner = CreatePlannerWithResponse(malformedJson);

            // Act
            var result = await planner.CalculateRoute(50.0f, 14.0f, 50.1f, 14.1f);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("parsováním", result.ErrorMessage);
        }
        [Fact]
        public async Task CalculateRoute_ReturnsFailure_OnHttpRequestException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(f => f.CreateClient("GoogleMapsClient"))
                .Returns(httpClient);

            var loggerMock = new Mock<ILogger<RouteByBusPlanner>>();
            var planner = new RouteByBusPlanner(loggerMock.Object, httpClientFactoryMock.Object, "fake-api-key");

            // Act
            var result = await planner.CalculateRoute(50.0f, 14.0f, 50.1f, 14.1f);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Nepodařilo se vypočítat cestu!", result.ErrorMessage);
        }
        [Fact]
        public async Task CalculateRoute_UsesDepartureTime_WhenProvided()
        {
            // Arrange
            var now = DateTime.Now.AddHours(1);
            var jsonResponse = @"
            {
                ""status"": ""OK"",
                ""routes"": [{
                    ""legs"": [{
                        ""distance"": { ""value"": 1000 },
                        ""duration"": { ""value"": 600 }
                    }]
                }]
            }";

            HttpRequestMessage capturedRequest = null;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("GoogleMapsClient")).Returns(httpClient);

            var loggerMock = new Mock<ILogger<RouteByBusPlanner>>();
            var planner = new RouteByBusPlanner(loggerMock.Object, factoryMock.Object, "fake-api-key");

            // Act
            await planner.CalculateRoute(50.0f, 14.0f, 50.1f, 14.1f, now);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Contains("departure_time=", capturedRequest.RequestUri.Query);
        }
        [Fact]
        public async Task CalculateRoute_NotUsesDepartureTime_WhenInPast()
        {
            // Arrange
            var now = DateTime.Now.AddHours(-1);
            var jsonResponse = @"
            {
                ""status"": ""OK"",
                ""routes"": [{
                    ""legs"": [{
                        ""distance"": { ""value"": 1000 },
                        ""duration"": { ""value"": 600 }
                    }]
                }]
            }";

            HttpRequestMessage capturedRequest = null;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("GoogleMapsClient")).Returns(httpClient);

            var loggerMock = new Mock<ILogger<RouteByBusPlanner>>();
            var planner = new RouteByBusPlanner(loggerMock.Object, factoryMock.Object, "fake-api-key");

            // Act
            await planner.CalculateRoute(50.0f, 14.0f, 50.1f, 14.1f, now);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.DoesNotContain("departure_time=", capturedRequest.RequestUri.Query);
        }

        private static RouteByBusPlanner CreatePlannerWithResponse(string jsonResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
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
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(f => f.CreateClient("GoogleMapsClient"))
                .Returns(httpClient);

            var loggerMock = new Mock<ILogger<RouteByBusPlanner>>();

            return new RouteByBusPlanner(loggerMock.Object, httpClientFactoryMock.Object, "fake-api-key");
        }

    }
}
