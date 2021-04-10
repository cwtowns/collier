using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Collier.Host;
using Collier.IO;
using Collier.Mining;
using Collier.Mining.OutputParsing;
using Collier.Mining.State;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CollierTests.Mining
{
    public class MinerEventStateTests
    {
        private Func<bool> AddEventHandler(TrexMiner.MinerStateHandler handler, IMinerState desiredNotificationState)
        {
            var result = false;

            handler.MiningInformationChanged += (o, e) =>
            {
                result = e.Value.Equals(desiredNotificationState.StateName);
            };

            return () =>
            {
                return result;
            };
        }

        private Func<bool> AddVerifyableEventHandler(TrexMiner.MinerStateHandler handler)
        {
            var result = false;

            handler.MiningInformationChanged += (o, e) =>
            {
                result = true;
            };

            return () =>
            {
                return result;
            };
        }


        [Fact]
        public void MinerCreatedInUnknownState()
        {
            var mockWebClient = new Mock<ITrexWebClient>();

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();
            var mockLogListener = new Mock<IMinerLogListener>();
            var mockLogObserver = new Mock<IInternalLoggingFrameworkObserver>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, mockLogListener.Object, mockLogObserver.Object);

            miner.CurrentState.StateName.Should().Be(new UnknownMinerState().StateName);
        }

        [Fact]
        public async void MinerStoppedFromGaming()
        {
            var mockMiner = new Mock<IMiner>();

            mockMiner.SetupProperty(x => x.CurrentState);

            mockMiner.Object.CurrentState = new MinerStartedFromNoGaming();

            var stateHandler = new TrexMiner.MinerStateHandler(mockMiner.Object);
            var newState = new MinerStoppedFromGaming();
            var eventVerification = AddEventHandler(stateHandler, newState);
            var stateTransitionSuccess = await stateHandler.TransitionToStateAsync(newState);

            stateTransitionSuccess.Should().Be(true, "We should be able to stop from gaming if we started from gaming.");
            mockMiner.Verify(x => x.Stop(), Times.Once, "The miner should ask to stop every time.");
            mockMiner.Object.CurrentState.Should().Be(newState, "The state value should reflect the new state.");
            eventVerification().Should().Be(true, "We should be notified during successful state transition.");
        }

        [Fact]
        public async void MinerWontStartWhenManuallyPaused()
        {
            var mockMiner = new Mock<IMiner>();

            mockMiner.SetupProperty(x => x.CurrentState);

            var firstState = new MinerStoppedFromUserRequest();
            mockMiner.Object.CurrentState = firstState;

            var stateHandler = new TrexMiner.MinerStateHandler(mockMiner.Object);
            var newState = new MinerStartedFromNoGaming();
            var eventVerification = AddVerifyableEventHandler(stateHandler);
            var stateTransitionSuccess = await stateHandler.TransitionToStateAsync(newState);

            stateTransitionSuccess.Should().Be(false, "We should not start mining when there is no gaming activity even when the user has manually paused.");
            mockMiner.Verify(x => x.Start(), Times.Never);
            mockMiner.Object.CurrentState.Should().Be(firstState);
            eventVerification().Should().Be(false, "No notification should happen when state does not change.");
        }

        [Fact]
        public async void MinerStopsWhenManuallyPaused()
        {
            var mockMiner = new Mock<IMiner>();

            mockMiner.SetupProperty(x => x.CurrentState);

            var firstState = new MinerStartedFromNoGaming();
            mockMiner.Object.CurrentState = firstState;

            var stateHandler = new TrexMiner.MinerStateHandler(mockMiner.Object);
            var newState = new MinerStoppedFromUserRequest();
            var eventVerification = AddEventHandler(stateHandler, newState);
            var stateTransitionSuccess = await stateHandler.TransitionToStateAsync(newState);

            stateTransitionSuccess.Should().Be(true, "We should pause from user request if running.");
            mockMiner.Verify(x => x.Stop(), Times.Once);
            mockMiner.Object.CurrentState.Should().Be(newState);
            eventVerification().Should().Be(true, "We should be notified during successful state transition.");
        }

        [Fact]
        public async void MinerStartsFromNogaming()
        {
            var mockMiner = new Mock<IMiner>();

            mockMiner.SetupProperty(x => x.CurrentState);

            var firstState = new UnknownMinerState();
            mockMiner.Object.CurrentState = firstState;

            var stateHandler = new TrexMiner.MinerStateHandler(mockMiner.Object);
            var newState = new MinerStartedFromNoGaming();
            var eventVerification = AddVerifyableEventHandler(stateHandler);
            var stateTransitionSuccess = await stateHandler.TransitionToStateAsync(newState);

            stateTransitionSuccess.Should().Be(true, "We should start when there is no gaming activity");
            mockMiner.Verify(x => x.Start(), Times.Once);
            mockMiner.Object.CurrentState.Should().Be(newState);
            eventVerification().Should().Be(true, "we should be notified during successful state transition.");
        }

        [Fact]
        public async void MinerStartsFromUserRequest()
        {
            var mockMiner = new Mock<IMiner>();

            mockMiner.SetupProperty(x => x.CurrentState);

            var firstState = new MinerStoppedFromUserRequest();
            mockMiner.Object.CurrentState = firstState;

            var stateHandler = new TrexMiner.MinerStateHandler(mockMiner.Object);
            var newState = new MinerStartedFromUserRequest();
            var eventVerification = AddVerifyableEventHandler(stateHandler);
            var stateTransitionSuccess = await stateHandler.TransitionToStateAsync(newState);

            stateTransitionSuccess.Should().Be(true, "We should start when the user asks to resume from a pause.");
            mockMiner.Verify(x => x.Start(), Times.Once);
            mockMiner.Object.CurrentState.Should().Be(newState);
            eventVerification().Should().Be(true, "we should be notified during successful state transition.");
        }

        [Fact]
        public async void MinerDoesNotStopWhenNotRunning()
        {
            var mockMiner = new Mock<IMiner>();

            mockMiner.SetupProperty(x => x.CurrentState);

            var firstState = new MinerStoppedFromUserRequest();
            mockMiner.Object.CurrentState = firstState;

            var stateHandler = new TrexMiner.MinerStateHandler(mockMiner.Object);
            var newState = new MinerStoppedFromGaming();
            var eventVerification = AddVerifyableEventHandler(stateHandler);
            var stateTransitionSuccess = await stateHandler.TransitionToStateAsync(newState);

            stateTransitionSuccess.Should().Be(false, "We should not transition state when not running.");
            mockMiner.Verify(x => x.Start(), Times.Never);
            mockMiner.Object.CurrentState.Should().Be(firstState);
            eventVerification().Should().Be(false, "we should not be notified if state transition failed.");
        }

        [Fact]
        public async void UserCanOnlySuccessfullyStartWhenTheyHavePaused()
        {
            var mockMiner = new Mock<IMiner>();

            mockMiner.SetupProperty(x => x.CurrentState);

            var firstState = new MinerStartedFromNoGaming();
            mockMiner.Object.CurrentState = firstState;

            var stateHandler = new TrexMiner.MinerStateHandler(mockMiner.Object);
            var newState = new MinerStartedFromUserRequest();
            var eventVerification = AddVerifyableEventHandler(stateHandler);
            var stateTransitionSuccess = await stateHandler.TransitionToStateAsync(newState);

            stateTransitionSuccess.Should().Be(false, "We should not transition state when the user has not paused.");
            mockMiner.Verify(x => x.Start(), Times.Never);
            mockMiner.Object.CurrentState.Should().Be(firstState);
            eventVerification().Should().Be(false, "we should not be notified if state transition failed.");
        }

        [Fact]
        public async void StateTransitionDoesNotHappenWhenThereIsNoChange()
        {
            var mockMiner = new Mock<IMiner>();

            mockMiner.SetupProperty(x => x.CurrentState);

            var firstState = new MinerStartedFromNoGaming();
            mockMiner.Object.CurrentState = firstState;

            var stateHandler = new TrexMiner.MinerStateHandler(mockMiner.Object);
            var newState = new MinerStartedFromNoGaming();
            var eventVerification = AddVerifyableEventHandler(stateHandler);
            var stateTransitionSuccess = await stateHandler.TransitionToStateAsync(newState);

            stateTransitionSuccess.Should().Be(false, "We should not transition state when we are already in that state.");
            mockMiner.Verify(x => x.Start(), Times.Never);
            mockMiner.Object.CurrentState.Should().Be(firstState);
            eventVerification().Should().Be(false, "we should not be notified if state transition failed.");
        }

    }
}
