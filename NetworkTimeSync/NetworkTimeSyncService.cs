using System.ServiceProcess;
using Common.Logging;
using NetworkTimeSync.TimeServices.NetworkTimeService;
using NetworkTimeSync.TimeServices.WindowsTimeService;
using NetworkTimeSync.UpdateTime;

namespace NetworkTimeSync
{
	public partial class NetworkTimeSyncService : ServiceBase
	{
	    private static readonly ILog Log = LogManager.GetLogger(typeof(NetworkTimeSyncService));
	    private readonly UpdateTimeRunner runner;

		public NetworkTimeSyncService()
		{
			InitializeComponent();
            var networkTimeService = new TimeZoneDbService("YourApiKeyHere");
            var windowsTimeService = new WindowsTimeServiceImpl(new Win32ExternalImpl());
            runner = new UpdateTimeRunner(networkTimeService, windowsTimeService);
		}

		protected override void OnStart(string[] args)
		{
            Log.Trace("Starting Network Time Sync service");
            runner.Start();
		}

		protected override void OnStop()
		{
		    Log.Trace("Stopping Network Time Sync service");
            runner.Stop();
        }
	}
}
