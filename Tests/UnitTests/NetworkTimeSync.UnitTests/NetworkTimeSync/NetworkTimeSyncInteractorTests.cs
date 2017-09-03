using System;
using Moq;
using NetworkTimeSync.NetworkTimeSync;
using NetworkTimeSync.TimeServices.NetworkTimeService;
using NetworkTimeSync.TimeServices.WindowsTimeService;
using NUnit.Framework;

namespace NetworkTimeSync.UnitTests.NetworkTimeSync
{
    [TestFixture]
    public class NetworkTimeSyncInteractorTests
    {
        private Mock<NetworkTimeService> mockNetworkTimeService;
        private Mock<WindowsTimeService> mockWindowsTimeService;
        private NetworkTimeSyncInteractor interactor;
        private Exception caughtException;
        private bool result;

        [SetUp]
        public void Setup()
        {
            result = false;
            caughtException = null;
            mockNetworkTimeService = new Mock<NetworkTimeService>();
            mockWindowsTimeService = new Mock<WindowsTimeService>();
            interactor = new NetworkTimeSyncInteractor(mockNetworkTimeService.Object, mockWindowsTimeService.Object);
        }
        
        [Test]
        public void NetworkTimeServiceErrorDoesNotUpdateTime()
        {
            GivenTheNetworkTimeServiceWillThrowAnException();
            WhenIUpdateTheTime();
            ThenTheWindowsTimeWasNeverUpdated();
            ThenNoExceptionWasThrownByTheInteractor();
            ThenTheResultWas(false);
        }

        [Test]
        public void WindowsTimeServiceExceptionIsHandledAndNotRethrown()
        {
            GivenTheWindowsTimeServiceWillThrownAnException();
            WhenIUpdateTheTime();
            ThenNoExceptionWasThrownByTheInteractor();
            ThenTheResultWas(false);
        }

        [Test]
        public void TimeIsSuccessfullyUpdated()
        {
            var networkTime = new DateTime(2017, 8, 5, 9, 44, 5);
            GivenTheNetworkTimeServiceWillReturn(networkTime, "America/Denver");
            WhenIUpdateTheTime();
            ThenTheWindowsTimeWasUpdated(networkTime);
            ThenTheResultWas(true);
        }

        private void GivenTheNetworkTimeServiceWillThrowAnException()
        {
            mockNetworkTimeService.Setup(m => m.GetTimeForZone(It.IsAny<string>()))
                .Throws(new NetworkTimeServiceException("ExceptionMessage"));
        }

        private void GivenTheWindowsTimeServiceWillThrownAnException()
        {
            mockWindowsTimeService.Setup(m => m.SetWindowsTime(It.IsAny<DateTime>()))
                .Throws(new WindowsTimeServiceException("ExceptionMessage"));
        }

        private void GivenTheNetworkTimeServiceWillReturn(DateTime networkTime, string timeZone)
        {
            mockNetworkTimeService.Setup(m => m.GetTimeForZone(timeZone)).Returns(networkTime);
        }

        private void WhenIUpdateTheTime()
        {
            try
            {
                result = interactor.Execute();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }
        }

        private void ThenTheWindowsTimeWasNeverUpdated()
        {
            mockWindowsTimeService.Verify(m => m.SetWindowsTime(It.IsAny<DateTime>()), Times.Never);
        }

        private void ThenTheWindowsTimeWasUpdated(DateTime expectedDateTime)
        {
            mockWindowsTimeService.Verify(m => m.SetWindowsTime(expectedDateTime), Times.Once);
        }

        private void ThenNoExceptionWasThrownByTheInteractor()
        {
            Assert.IsNull(caughtException);
        }

        private void ThenTheResultWas(bool expectedResult)
        {
            Assert.AreEqual(expectedResult, result);
        }
    }
}
