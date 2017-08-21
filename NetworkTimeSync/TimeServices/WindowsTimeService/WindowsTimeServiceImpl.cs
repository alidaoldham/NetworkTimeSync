using System;

namespace NetworkTimeSync.TimeServices.WindowsTimeService
{
    public class WindowsTimeServiceImpl : WindowsTimeService
    {
        private readonly Win32External win32;

        public WindowsTimeServiceImpl(Win32External win32)
        {
            this.win32 = win32;
        }

        public void SetWindowsTime(DateTime dateTime)
        {
            var systemTime = ConvertDateTimeToSystemTime(dateTime);
            if (win32.SetLocalTime(ref systemTime) == false)
            {
                throw new WindowsTimeServiceException($"Set Local Time failed with error [{win32.GetLastError()}]");
            }
        }

        private static SYSTEMTIME ConvertDateTimeToSystemTime(DateTime dateTime)
        {
            return new SYSTEMTIME
            {
                wYear = Convert.ToUInt16(dateTime.Year),
                wMonth = Convert.ToUInt16(dateTime.Month),
                wDay = Convert.ToUInt16(dateTime.Day),
                wDayOfWeek = 0,
                wHour = Convert.ToUInt16(dateTime.Hour),
                wMinute = Convert.ToUInt16(dateTime.Minute),
                wSecond = Convert.ToUInt16(dateTime.Second),
                wMilliseconds = 0
            };
        }
    }
}