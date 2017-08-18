using System;

namespace NetworkTimeSync.TimeServices.NetworkTimeService
{
    public class NetworkTimeServiceException : Exception
    {
        public NetworkTimeServiceException(Exception ex) : base(ex.Message, ex)
        {
        }

        public NetworkTimeServiceException(string message) : base(message)
        {
        }

    }
}