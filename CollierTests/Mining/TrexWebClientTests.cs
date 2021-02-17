using FluentAssertions;
using Microsoft.Extensions.Options;
using MiningAutomater.Mining;
using TestingUtilities;
using Xunit;

namespace MiningAutomaterTests.Mining
{
    public class TrexWebClientTests
    {


        [Fact]
        public async void WebClientIndicatesRunningWithPositiveHashrate()
        {
            var settings = new TrexWebClient.Settings() { StatusUrl = "http://localhost" };
            var httpClient = HttpClientMock.GetClientWithSpecificResultPayload("{ 'hashrate' : 100 }");
            var webClient = new TrexWebClient(Options.Create(settings), httpClient);

            (await webClient.IsMiningAsync()).Should().Be(true, "the hashrate is above zero");
        }

        [Fact]
        public async void WebClientIndicatesIdleWithZeroHashrate()
        {
            var settings = new TrexWebClient.Settings() { StatusUrl = "http://localhost" };
            var httpClient = HttpClientMock.GetClientWithSpecificResultPayload("{ 'hashrate' : 0 }");
            var webClient = new TrexWebClient(Options.Create(settings), httpClient);

            (await webClient.IsMiningAsync()).Should().Be(false, "the hashrate is zero");
        }

        [Fact]
        public async void WebClientIndicatesIdleWithMissingHashrate()
        {
            var settings = new TrexWebClient.Settings() { StatusUrl = "http://localhost" };
            var httpClient = HttpClientMock.GetClientWithSpecificResultPayload("{ 'hashrate' : 0 }");

            var webClient = new TrexWebClient(Options.Create(settings), httpClient);

            (await webClient.IsMiningAsync()).Should().Be(false, "the hashrate is missing");
        }

    }
}
