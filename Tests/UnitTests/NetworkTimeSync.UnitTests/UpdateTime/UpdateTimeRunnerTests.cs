using System.Threading;
using Moq;
using NetworkTimeSync.TimeServices.NetworkTimeService;
using NetworkTimeSync.TimeServices.WindowsTimeService;
using NetworkTimeSync.UpdateTime;
using NUnit.Framework;

namespace NetworkTimeSync.UnitTests.UpdateTime
{
    [TestFixture]
    public class UpdateTimeRunnerTests
    {
        internal class UpdateTimeRunnerSub : UpdateTimeRunner
        {
            public UpdateTimeRunnerSub(NetworkTimeService networkTimeService, WindowsTimeService windowsTimeService) : base(networkTimeService, windowsTimeService)
            {
            }

            public new bool IsRunning => IsRunning();

            public int NumberOfTimesUpdateTimeWasExecuted { get; private set; }

            protected override void UpdateTime()
            {
                waitingForUpdateIntervalToPass = false;
                NumberOfTimesUpdateTimeWasExecuted++;
                base.UpdateTime();
            }

            private bool overrideWaitForUpdateIntervalToPass;
            private int numberOfTimesUpdateIntervalHasPassed;
            private bool waitingForUpdateIntervalToPass;

            public void GivenIOverrideWaitForUpdateIntervalToPass()
            {
                overrideWaitForUpdateIntervalToPass = true;
            }

            protected override void WaitForUpdateIntervalToPass(CancellationToken ct)
            {
                waitingForUpdateIntervalToPass = true;

                if (overrideWaitForUpdateIntervalToPass)
                    SpinWait.SpinUntil(() => numberOfTimesUpdateIntervalHasPassed > NumberOfTimesUpdateTimeWasExecuted);
                else
                    base.WaitForUpdateIntervalToPass(ct);
            }

            public void WhenIAllowTheUpdateIntervalToPass()
            {
                numberOfTimesUpdateIntervalHasPassed++;
            }

            public void WhenIWaitForTheTimeToBeUpdated(int numberOfTimes)
            {
                SpinWait.SpinUntil(() => NumberOfTimesUpdateTimeWasExecuted == numberOfTimes);
            }

            public void WhenIWaitForTheUpdaterToBeWaitingForTheIntervalToPass()
            {
                SpinWait.SpinUntil(() => waitingForUpdateIntervalToPass);
            }
        }

        private Mock<NetworkTimeService> mockNetworkTimeService;
        private Mock<WindowsTimeService> mockWindowsTimeService;
        private UpdateTimeRunnerSub runner;

        [SetUp]
        public void Setup()
        {
            mockWindowsTimeService = new Mock<WindowsTimeService>();
            mockNetworkTimeService = new Mock<NetworkTimeService>();
            runner = new UpdateTimeRunnerSub(mockNetworkTimeService.Object, mockWindowsTimeService.Object);
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

        private void WhenIWaitTheRunnerWillUpdateTheTime(int numberOfTimes)
        {
            runner.WhenIAllowTheUpdateIntervalToPass();
            runner.WhenIWaitForTheTimeToBeUpdated(numberOfTimes);
            Assert.AreEqual(numberOfTimes, runner.NumberOfTimesUpdateTimeWasExecuted);
            mockNetworkTimeService.Verify(m => m.GetTimeForZone(It.IsAny<string>()), Times.Exactly(numberOfTimes));
            Assert.IsTrue(runner.IsRunning);
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
            Assert.AreEqual(0, runner.NumberOfTimesUpdateTimeWasExecuted);
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
    }
}
