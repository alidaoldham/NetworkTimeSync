using NetworkTimeSync.TimeServices.WindowsTimeService;

namespace NetworkTimeSync.UnitTests.TimeServices.WindowsTimeService
{
    public class MockWin32External : Win32External
    {
        private SYSTEMTIME capturedSetLocalTime;
        private bool setLocalTimeReturnValue;
        private int lastErrorReturnValue;

        #region Win32External

        public bool SetLocalTime(ref SYSTEMTIME systemTime)
        {
            capturedSetLocalTime = systemTime;
            return setLocalTimeReturnValue;
        }

        public int GetLastError()
        {
            return lastErrorReturnValue;
        }

        #endregion

        public SYSTEMTIME GetCapturedSetLocalTime()
        {
            return capturedSetLocalTime;
        }

        public void SetLocalTimeWillReturn(bool returnValue)
        {
            setLocalTimeReturnValue = returnValue;
        }

        public void GetLastErrorWillReturn(int lastError)
        {
            lastErrorReturnValue = lastError;
        }
    }
}