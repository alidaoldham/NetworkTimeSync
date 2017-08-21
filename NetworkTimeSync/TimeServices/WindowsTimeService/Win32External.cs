namespace NetworkTimeSync.TimeServices.WindowsTimeService
{
    public interface Win32External
    {
        bool SetLocalTime(ref SYSTEMTIME systemTime);

        int GetLastError();
    }
}