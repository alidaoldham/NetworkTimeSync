using System;
using System.Threading;
using Moq;
using NetworkTimeSync.NetworkTimeSync;
using NetworkTimeSync.TimeServices.NetworkTimeService;
using NetworkTimeSync.TimeServices.WindowsTimeService;
using NUnit.Framework;

namespace NetworkTimeSync.UnitTests.NetworkTimeSync
{
    [TestFixture]
    public class NetworkTimeSyncRunnerTests
    {
        internal class NetworkTimeSyncRunnerSub : NetworkTimeSyncRunner
        {
            public NetworkTimeSyncRunnerSub(NetworkTimeService networkTimeService, WindowsTimeService windowsTimeService) : base(networkTimeService, windowsTimeService)
            {
            }

            public new bool IsRunning => IsRunning();

            public int NumberOfTimesSyncToNetworkWasExecuted { get; private set; }

            protected override void SyncTimeToNetwork()
            {
                base.SyncTimeToNetwork();
                waitingForUpdateIntervalToPass = false;
                NumberOfTimesSyncToNetworkWasExecuted++;
            }

            private bool overrideWaitForUpdateIntervalToPass;
            private int numberOfTimesUpdateIntervalHasPassed;
            private bool waitingForUpdateIntervalToPass;
            private int capturedIntervalInMilliseconds;

            public void GivenIOverrideWaitForUpdateIntervalToPass()
            {
                overrideWaitForUpdateIntervalToPass = true;
            }
            
            protected override void WaitForUpdateIntervalToPass(CancellationToken ct, int intervalInMilliseconds)
            {
                waitingForUpdateIntervalToPass = true;
                capturedIntervalInMilliseconds = intervalInMilliseconds;

                if (overrideWaitForUpdateIntervalToPass)
                    SpinWait.SpinUntil(() => ct.IsCancellationRequested || numberOfTimesUpdateIntervalHasPassed > NumberOfTimesSyncToNetworkWasExecuted);
                else
                    base.WaitForUpdateIntervalToPass(ct, intervalInMilliseconds);
            }

            public void WhenIAllowTheUpdateIntervalToPass()
            {
                numberOfTimesUpdateIntervalHasPassed++;
            }

            public void WhenIWaitForTheTimeToBeUpdated(int numberOfTimes)
            {
                SpinWait.SpinUntil(() => NumberOfTimesSyncToNetworkWasExecuted == numberOfTimes);
            }

            public void WhenIWaitForTheUpdaterToBeWaitingForTheIntervalToPass()
            {
                SpinWait.SpinUntil(() => waitingForUpdateIntervalToPass);
            }

            public int GetIntervalInMilliseconds()
            {
                return capturedIntervalInMilliseconds;
            }
        }

        private Mock<NetworkTimeService> mockNetworkTimeService;
        private Mock<WindowsTimeService> mockWindowsTimeService;
        private NetworkTimeSyncRunnerSub runner;
        private const int ShortIntervalInMilliseconds = 10 * 1000;
        private const int LongIntervalInMilliseconds = 10 * 60 * 1000;

        [SetUp]
        public void Setup()
        {
            mockWindowsTimeService = new Mock<WindowsTimeService>();
            mockNetworkTimeService = new Mock<NetworkTimeService>();
            runner = new NetworkTimeSyncRunnerSub(mockNetworkTimeService.Object, mockWindowsTimeService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            runner.Stop();
        }

        [Test]
        public void RunnerDoesNotStartOnCreation()
        {
            Assert.IsFalse(runner.IsRunning);
        }
        
        [Test]
        public void TheRunnerWillContinueToUpdateAfterBeingStarted()
        {
            runner.GivenIOverrideWaitForUpdateIntervalToPass();
            runner.Start();
            WhenIWaitTheRunnerWillUpdateTheTime(1);
            WhenIWaitTheRunnerWillUpdateTheTime(2);
        }

        [Test]
        public void StoppingTheRunnerWhenItHasNotStartedDoesntDoAnything()
        {
            runner.Stop();
            Assert.IsFalse(runner.IsRunning);
        }

        [Test]
        public void StoppingTheRunnerStopsIt()
        {
            runner.Start();
            runner.WhenIWaitForTheUpdaterToBeWaitingForTheIntervalToPass();
            runner.Stop();
            Assert.IsFalse(runner.IsRunning);
        }

        [Test]
        public void TimeIsSyncedAfterStartingAndBeforeWaitingForTheIntervalToPass()
        {
            runner.Start();
            runner.WhenIWaitForTheUpdaterToBeWaitingForTheIntervalToPass();
            runner.Stop();
            Assert.AreEqual(1, runner.NumberOfTimesSyncToNetworkWasExecuted);
        }

        [Test]
        public void TimeSyncFailsFirstTimeSoIntervalToWaitIsSmall()
        {
            GivenTheTimeSyncWithNetworkWillFail();
            runner.Start();
            runner.WhenIWaitForTheUpdaterToBeWaitingForTheIntervalToPass();
            Assert.AreEqual(ShortIntervalInMilliseconds, runner.GetIntervalInMilliseconds());
        }

        [Test]
        public void TimeSyncSucceedsSoIntervalTimeIsLong()
        {
            runner.Start();
            runner.WhenIWaitForTheUpdaterToBeWaitingForTheIntervalToPass();
            Assert.AreEqual(LongIntervalInMilliseconds, runner.GetIntervalInMilliseconds());
        }

        [Test]
        public void IntervalToWaitWillChangeOnceTimeSyncSucceeds()
        {
            runner.GivenIOverrideWaitForUpdateIntervalToPass();
            GivenTheTimeSyncWithNetworkWillFail();
            runner.Start();
            WhenIWaitTheRunnerWillUpdateTheTime(1);
            Assert.AreEqual(ShortIntervalInMilliseconds, runner.GetIntervalInMilliseconds());
            GivenTheTimeSyncWithNetworkWillSucceed();
            WhenIWaitTheRunnerWillUpdateTheTime(2);
            Assert.AreEqual(LongIntervalInMilliseconds, runner.GetIntervalInMilliseconds());
        }

        [Test]
        public void IntervalToWaitWillChangeOnceTimeSyncFails()
        {
            runner.GivenIOverrideWaitForUpdateIntervalToPass();
            runner.Start();
            WhenIWaitTheRunnerWillUpdateTheTime(1);
            Assert.AreEqual(LongIntervalInMilliseconds, runner.GetIntervalInMilliseconds());
            GivenTheTimeSyncWithNetworkWillFail();
            WhenIWaitTheRunnerWillUpdateTheTime(2);
            Assert.AreEqual(ShortIntervalInMilliseconds, runner.GetIntervalInMilliseconds());
        }

        [Test]
        [Repeat(1000)]
        public void StartingTwiceDoesNotHaveAnEffect()
        {
            runner.GivenIOverrideWaitForUpdateIntervalToPass();
            runner.Start();
            runner.WhenIAllowTheUpdateIntervalToPass();
            runner.Start();
            runner.WhenIAllowTheUpdateIntervalToPass();
            WhenIWaitTheRunnerWillUpdateTheTime(3);
        }

        private void GivenTheTimeSyncWithNetworkWillFail()
        {
            mockNetworkTimeService.Setup(m => m.GetTimeForZone(It.IsAny<string>()))
                .Throws(new NetworkTimeServiceException("ExceptionMessage"));
        }

        private void GivenTheTimeSyncWithNetworkWillSucceed()
        {
            mockNetworkTimeService.Setup(m => m.GetTimeForZone(It.IsAny<string>())).Returns(DateTime.Now);
        }

        private void WhenIWaitTheRunnerWillUpdateTheTime(int numberOfTimes)
        {
            runner.WhenIAllowTheUpdateIntervalToPass();
            runner.WhenIWaitForTheTimeToBeUpdated(numberOfTimes);
            Assert.AreEqual(numberOfTimes, runner.NumberOfTimesSyncToNetworkWasExecuted);
            mockNetworkTimeService.Verify(m => m.GetTimeForZone(It.IsAny<string>()), Times.Exactly(numberOfTimes));
            Assert.IsTrue(runner.IsRunning);
        }
    }
}
