using System;
using Common.Logging;
using NetworkTimeSync.TimeServices.NetworkTimeService;
using NetworkTimeSync.TimeServices.WindowsTimeService;

namespace NetworkTimeSync.NetworkTimeSync
{
    public class NetworkTimeSyncInteractor
    {
        private static readonly ILog Log = LogManager.GetLogger<NetworkTimeSyncInteractor>();

        private readonly NetworkTimeService networkTimeService;
        private readonly WindowsTimeService windowsTimeService;

        public NetworkTimeSyncInteractor(NetworkTimeService networkTimeService, WindowsTimeService windowsTimeService)
        {
            this.networkTimeService = networkTimeService;
            this.windowsTimeService = windowsTimeService;
        }

        public bool Execute()
        {
            try
            {
                return SyncTimeToNetwork();
            }
            catch (Exception ex)
            {
                Log.Error("Updated time has failed with exception:", ex);
                return false;
            }
        }

        private bool SyncTimeToNetwork()
        {
            var timeZone = "America/Denver";
            var dateTime = GetNetworkTime(timeZone);
            SetWindowsTime(dateTime);
            return true;
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