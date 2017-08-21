using System.Runtime.InteropServices;

namespace NetworkTimeSync.TimeServices.WindowsTimeService
{
    public class Win32ExternalImpl : Win32External
    {
        private static class Win32Extern
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetLocalTime(ref SYSTEMTIME systemTime);

        }

        public bool SetLocalTime(ref SYSTEMTIME systemTime)
        {
            return Win32Extern.SetLocalTime(ref systemTime);
        }

        public int GetLastError()
        {
            return Marshal.GetLastWin32Error();
        }
    }
}
