using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace NetworkTimeSync.TimeServices.NetworkTimeService
{
    public class TimeZoneDbService
    {
        private const string GetTimeZoneUri = "http://api.timezonedb.com/v2/get-time-zone";
        private readonly string apiKey;

        public TimeZoneDbService(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public DateTime GetTimeForZone(string timeZone)
        {
            var response = GetTimeZoneFromTimeZoneDb(timeZone);
            ThrowExceptionIfResponseIsNotOkay(response.StatusCode);

            var timeZoneDetails = GetTimeZoneDetailsFromResponse(response);
            ThrowExceptionIfTimeZoneDetailsAreNotOkay(response.StatusCode, timeZoneDetails);

            return UnixTimeStampToDateTime(timeZoneDetails.TimeStamp - timeZoneDetails.GmtOffset);
        }

        private HttpResponseMessage GetTimeZoneFromTimeZoneDb(string timeZone)
        {
            try
            {
                var client = new HttpClient { BaseAddress = new Uri(GetTimeZoneUri) };
                var parameters = $"?key={apiKey}&by=zone&format=json&zone={timeZone}";
                return GetAsync(client, parameters);
            }
            catch (Exception ex)
            {
                throw new NetworkTimeServiceException(ex);
            }
        }

        protected virtual HttpResponseMessage GetAsync(HttpClient client, string parameters)
        {
            return client.GetAsync("").Result;
        }

        private static void ThrowExceptionIfResponseIsNotOkay(HttpStatusCode statusCode)
        {
            if (statusCode != HttpStatusCode.OK)
                throw new NetworkTimeServiceException($"Response is [{statusCode}]");
        }

        private static TimeZoneDbResponse GetTimeZoneDetailsFromResponse(HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<TimeZoneDbResponse>(response.Content.ReadAsStringAsync().Result);
        }

        private static void ThrowExceptionIfTimeZoneDetailsAreNotOkay(HttpStatusCode statusCode, TimeZoneDbResponse timeZoneDetails)
        {
            if (timeZoneDetails.Status != "OK")
            {
                var message = $"Response is [{statusCode}] with status [{timeZoneDetails.Status}] and message [{timeZoneDetails.Message}]";
                throw new NetworkTimeServiceException(message);
            }
        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}