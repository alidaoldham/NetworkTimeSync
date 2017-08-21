using System;

namespace NetworkTimeSync.TimeServices.WindowsTimeService
{
    public class WindowsTimeServiceException : Exception
    {
        public WindowsTimeServiceException(string message) : base(message)
        {
        }
    }
}