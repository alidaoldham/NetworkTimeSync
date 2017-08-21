using System;
using System.Runtime.InteropServices;
using Moq;
using NetworkTimeSync.TimeServices.WindowsTimeService;
using NUnit.Framework;

namespace NetworkTimeSync.UnitTests.TimeServices.WindowsTimeService
{
    [TestFixture]
    public class WindowsTimeServiceTests
    {
        private MockWin32External mockWin32External;
        private WindowsTimeServiceImpl service;
        private Exception caughtException;

        [SetUp]
        public void Setup()
        {
            caughtException = null;
            mockWin32External = new MockWin32External();
            service = new WindowsTimeServiceImpl(mockWin32External);
        }

        [Test]
        public void SettingWindowsTimeWillCorrectlyConvertTheDateTime()
        {
            var dateTime = new DateTime(2017, 8, 22, 18, 33, 9);
            GivenSetLocalTimeWillReturn(true);
            WhenISetTheWindowsTime(dateTime);
            var expectedSystemTime = new SYSTEMTIME
            {
                wYear = 2017,
                wMonth = 8,
                wDay = 22,
                wDayOfWeek = 0,
                wHour = 18,
                wMinute = 33,
                wSecond = 9,
                wMilliseconds = 0
            };
            Assert.AreEqual(expectedSystemTime, mockWin32External.GetCapturedSetLocalTime());
            Assert.IsNull(caughtException);
        }

        [Test]
        public void SetTimeReturningFalseWillThrowAnExceptionWithTheLastErrorCode()
        {
            var dateTime = new DateTime(2017, 8, 22, 19, 6, 44);
            GivenSetLocalTimeWillReturn(false);
            GivenGetLastErrorWillReturn(1314);
            WhenISetTheWindowsTime(dateTime);
            Assert.IsNotNull(caughtException);
            Assert.IsInstanceOf<WindowsTimeServiceException>(caughtException);
            Assert.AreEqual("Set Local Time failed with error [1314]", caughtException.Message);
        }

        private void GivenGetLastErrorWillReturn(int lastError)
        {
            mockWin32External.GetLastErrorWillReturn(lastError);
        }

        private void GivenSetLocalTimeWillReturn(bool returnValue)
        {
            mockWin32External.SetLocalTimeWillReturn(returnValue);
        }

        private void WhenISetTheWindowsTime(DateTime dateTime)
        {
            try
            {
                service.SetWindowsTime(dateTime);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }
        }
    }
}
