using System;
using Common.Logging;
using NetworkTimeSync.TimeServices.NetworkTimeService;
using NetworkTimeSync.TimeServices.WindowsTimeService;

namespace NetworkTimeSync.UpdateTime
{
    public class UpdateTimeInteractor
    {
        private static readonly ILog Log = LogManager.GetLogger<UpdateTimeInteractor>();

        private readonly NetworkTimeService networkTimeService;
        private readonly WindowsTimeService windowsTimeService;

        public UpdateTimeInteractor(NetworkTimeService networkTimeService, WindowsTimeService windowsTimeService)
        {
            this.networkTimeService = networkTimeService;
            this.windowsTimeService = windowsTimeService;
        }

        public void Execute()
        {
            try
            {
                var timeZone = "America/Denver";
                var dateTime = GetNetworkTime(timeZone);
                SetWindowsTime(dateTime);
            }
            catch (Exception ex)
            {
                Log.Error("Updated time has failed with exception:", ex);
            }
        }

        private DateTime GetNetworkTime(string timeZone)
        {
            Log.Trace($"Requesting network time for time zone [{timeZone}]");
            var dateTime = networkTimeService.GetTimeForZone(timeZone);
            return dateTime;
        }

        private void SetWindowsTime(DateTime dateTime)
        {
            Log.Trace($"Setting windows time to [{dateTime}]");
            windowsTimeService.SetWindowsTime(dateTime);
        }
    }
}