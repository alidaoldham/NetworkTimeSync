using System;

namespace NetworkTimeSync.TimeServices.NetworkTimeService
{
    public interface NetworkTimeService
    {
        DateTime GetTimeForZone(string timeZone);
    }
}
