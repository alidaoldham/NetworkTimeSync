using System;

namespace NetworkTimeSync.TimeServices.WindowsTimeService
{
    public interface WindowsTimeService
    {
        void SetWindowsTime(DateTime dateTime);
    }
}