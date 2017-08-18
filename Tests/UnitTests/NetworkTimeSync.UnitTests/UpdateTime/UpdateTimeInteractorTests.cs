using System;
using Moq;
using NetworkTimeSync.TimeServices.NetworkTimeService;
using NetworkTimeSync.TimeServices.WindowsTimeService;
using NetworkTimeSync.UpdateTime;
using NUnit.Framework;

namespace NetworkTimeSync.UnitTests.UpdateTime
{
    [TestFixture]
    public class UpdateTimeInteractorTests
    {
        private Mock<NetworkTimeService> mockNetworkTimeService;
        private Mock<WindowsTimeService> mockWindowsTimeService;
        private UpdateTimeInteractor interactor;

        [SetUp]
        public void Setup()
        {
            mockNetworkTimeService = new Mock<NetworkTimeService>();
            mockWindowsTimeService = new Mock<WindowsTimeService>();
            interactor = new UpdateTimeInteractor(mockNetworkTimeService.Object, mockWindowsTimeService.Object);
        }
        
        [Test]
        public void NetworkTimeServiceErrorDoesNotUpdateTime()
        {
            GivenTheNetworkTimeServiceWillThrowAnException();
            WhenIUpdateTheTime();
            ThenTheWindowsTimeWasNeverUpdated();
        }

        [Test]
        public void TimeIsSuccessfullyUpdated()
        {
            var networkTime = new DateTime(2017, 8, 5, 9, 44, 5);
            GivenTheNetworkTimeServiceWillReturn(networkTime, "America/Denver");
            WhenIUpdateTheTime();
            ThenTheWindowsTimeWasUpdated(networkTime);
        }

        private void GivenTheNetworkTimeServiceWillThrowAnException()
        {
            mockNetworkTimeService.Setup(m => m.GetTimeForZone(It.IsAny<string>()))
                .Throws(new NetworkTimeServiceException("ExceptionMessage"));
        }

        private void GivenTheNetworkTimeServiceWillReturn(DateTime networkTime, string timeZone)
        {
            mockNetworkTimeService.Setup(m => m.GetTimeForZone(timeZone)).Returns(networkTime);
        }

        private void WhenIUpdateTheTime()
        {
            interactor.Execute();
        }

        private void ThenTheWindowsTimeWasNeverUpdated()
        {
            mockWindowsTimeService.Verify(m => m.SetWindowsTime(It.IsAny<DateTime>()), Times.Never);
        }

        private void ThenTheWindowsTimeWasUpdated(DateTime expectedDateTime)
        {
            mockWindowsTimeService.Verify(m => m.SetWindowsTime(expectedDateTime), Times.Once);
        }
    }
}
