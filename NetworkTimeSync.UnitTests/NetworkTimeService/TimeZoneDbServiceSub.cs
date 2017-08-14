using System;
using System.Net.Http;
using NetworkTimeSync.NetworkTimeService;

namespace NetworkTimeSync.UnitTests.NetworkTimeService
{
    internal class TimeZoneDbServiceSub : TimeZoneDbService
    {
        private Exception connectionException;
        private HttpResponseMessage responseMessage;

        public TimeZoneDbServiceSub(string apiKey) : base(apiKey)
        { }

        public HttpClient CapturedClient { get; private set; }
        public string CapturedParameters { get; private set; }

        public void GivenTheConnectionAttemptThrowsAnException(Exception exception)
        {
            connectionException = exception;
        }

        public void GivenTheResponseMessageWillBe(HttpResponseMessage message)
        {
            responseMessage = message;
        }

        protected override HttpResponseMessage GetAsync(HttpClient client, string parameters)
        {
            CapturedClient = client;
            CapturedParameters = parameters;

            if (connectionException != null)
                throw connectionException;

            return responseMessage;
        }
    }
}
