using System;
using System.IO;
using System.Net;
using System.Net.Http;
using NetworkTimeSync.NetworkTimeService;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetworkTimeSync.UnitTests.NetworkTimeService
{
    [TestFixture]
    public class GetTimeForZoneTests
    {
        private TimeZoneDbServiceSub service;
        private Exception caughtException;
        private DateTime dateTimeResult;

        [SetUp]
        public void Setup()
        {
            caughtException = null;
            dateTimeResult = DateTime.MinValue;
            service = new TimeZoneDbServiceSub("ApiKey");
        }

        [Test]
        public void ConnectionAttemptThrowsAnException()
        {
            Exception connectionException = new IOException("ExceptionMessage");
            service.GivenTheConnectionAttemptThrowsAnException(connectionException);
            WhenIGetTheTimeForZone("TimeZone");

            Assert.IsNotNull(caughtException);
            Assert.IsInstanceOf<NetworkTimeServiceException>(caughtException);
            Assert.AreEqual(connectionException, caughtException.InnerException);
            Assert.AreEqual("ExceptionMessage", caughtException.Message);
        }

        [Test]
        public void ConnectionAttemptHasCorrectDetails()
        {
            WhenIGetTheTimeForZone("TimeZone");
            Assert.IsNotNull(service.CapturedClient);
            Assert.AreEqual(new Uri("http://api.timezonedb.com/v2/get-time-zone"), service.CapturedClient.BaseAddress);
            Assert.AreEqual("?key=ApiKey&by=zone&format=json&zone=TimeZone", service.CapturedParameters);
        }

        [Test]
        public void ConnectionResponseIsBadRequest()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            service.GivenTheResponseMessageWillBe(responseMessage);
            WhenIGetTheTimeForZone("TimeZone");
            Assert.IsNotNull(caughtException);
            Assert.IsInstanceOf<NetworkTimeServiceException>(caughtException);
            Assert.IsNull(caughtException.InnerException);
            Assert.AreEqual("Response is [BadRequest]", caughtException.Message);
        }

        [Test]
        public void ConnectionResponseIsOkayButStatusIsFailed()
        {
            var responseBody = new TimeZoneDbResponse
            {
                Status = "Status",
                Message = "Message"
            };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(responseBody))
            };
            service.GivenTheResponseMessageWillBe(responseMessage);
            WhenIGetTheTimeForZone("TimeZone");
            Assert.IsNotNull(caughtException);
            Assert.IsInstanceOf<NetworkTimeServiceException>(caughtException);
            Assert.IsNull(caughtException.InnerException);
            Assert.AreEqual("Response is [OK] with status [Status] and message [Message]", caughtException.Message);
        }

        [Test]
        public void ConnectionResponseIsOkay()
        {
            var responseBody = new TimeZoneDbResponse
            {
                Status = "OK",
                Message = "",
                CountryCode = "US",
                CountryName = "United States",
                ZoneName = "America/Denver",
                Abbreviation = "MDT",
                GmtOffset = -21600,
                Dst = "1",
                DstStart = 1489309200,
                DstEnd = 1509868799,
                NextAbbreviation = "MST",
                TimeStamp = 1502724667,
                Formatted = "2017-08-14 15:31:07"
            };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(responseBody))
            };
            service.GivenTheResponseMessageWillBe(responseMessage);
            WhenIGetTheTimeForZone("America/Denver");
            Assert.IsNull(caughtException);
            var expectedDateTime = new DateTime(2017, 8, 14, 15, 31, 07);
            Assert.AreEqual(expectedDateTime, dateTimeResult);
        }

        private void WhenIGetTheTimeForZone(string timeZone)
        {
            try
            {
                dateTimeResult = service.GetTimeForZone(timeZone);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }
        }
    }
}
