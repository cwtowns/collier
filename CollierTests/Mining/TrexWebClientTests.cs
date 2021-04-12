using System.Net;
using System.Net.Http;
using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Collier.Mining;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TestingUtilities;
using Xunit;
using Collier.Mining.Trex;

namespace CollierTests.Mining
{
    public class TrexWebClientTests
    {


        [Fact]
        public async void WebClientIndicatesRunningWithPositiveHashrate()
        {
            var settings = new TrexWebClient.Settings() { StatusUrl = "http://localhost" };
            var httpClient = HttpClientMock.GetClientWithSpecificResultPayload("{ 'hashrate' : 100 }");
            var mockLogger = new Mock<ILogger<TrexWebClient>>();
            var webClient = new TrexWebClient(mockLogger.Object, Options.Create(settings), httpClient);

            (await webClient.IsMiningAsync()).Should().Be(true, "the hashrate is above zero");
        }

        [Fact]
        public async void WebClientIndicatesIdleWithZeroHashrate()
        {
            var settings = new TrexWebClient.Settings() { StatusUrl = "http://localhost" };
            var httpClient = HttpClientMock.GetClientWithSpecificResultPayload("{ 'hashrate' : 0 }");
            var mockLogger = new Mock<ILogger<TrexWebClient>>();
            var webClient = new TrexWebClient(mockLogger.Object, Options.Create(settings), httpClient);

            (await webClient.IsMiningAsync()).Should().Be(false, "the hashrate is zero");
        }

        [Fact]
        public async void WebClientIndicatesIdleWithMissingHashrate()
        {
            var settings = new TrexWebClient.Settings() { StatusUrl = "http://localhost" };
            var mockLogger = new Mock<ILogger<TrexWebClient>>();
            var httpClient = HttpClientMock.GetClientWithSpecificResultPayload("{ 'hashrate' : 0 }");

            var webClient = new TrexWebClient(mockLogger.Object, Options.Create(settings), httpClient);

            (await webClient.IsMiningAsync()).Should().Be(false, "the hashrate is missing");
        }

        [Fact]
        public async void IsRunningAsyncWhenResultCodeFails()
        {
            var mockLogger = new Mock<ILogger<TrexWebClient>>();
            var settings = new TrexWebClient.Settings() { StatusUrl = "http://localhost" };
            var socketException = new System.Net.Sockets.SocketException(0);
            var ex = new HttpRequestException("test", socketException);
            var httpClient = HttpClientMock.GetResponseThatThrowsException(ex);
            var webClient = new TrexWebClient(mockLogger.Object, Options.Create(settings), httpClient);

            (await webClient.IsRunningAsync()).Should().Be(false, "We should swallow the conenction refused exception");
        }

        [Fact]
        public async void IsNotRunningAsyncWhenConnectionFails()
        {
            var mockLogger = new Mock<ILogger<TrexWebClient>>();
            var settings = new TrexWebClient.Settings() { StatusUrl = "http://localhost" };
            var httpClient = HttpClientMock.GetResponseWithStatusCode(HttpStatusCode.NotFound);
            var webClient = new TrexWebClient(mockLogger.Object, Options.Create(settings), httpClient);

            (await webClient.IsRunningAsync()).Should().Be(false, "Because the request was not found.");
        }

        [Fact]
        public async void IsRunningAsyncWhenResultCodeSucceeds()
        {
            var mockLogger = new Mock<ILogger<TrexWebClient>>();
            var settings = new TrexWebClient.Settings() { StatusUrl = "http://localhost" };
            var httpClient = HttpClientMock.GetResponseWithStatusCode(HttpStatusCode.OK);
            var webClient = new TrexWebClient(mockLogger.Object, Options.Create(settings), httpClient);

            (await webClient.IsRunningAsync()).Should().Be(true, "Because the request was not found.");
        }

    }
}
