using System.ServiceProcess;

namespace NetworkTimeSync
{
	public partial class NetworkTimeSyncService : ServiceBase
	{
		public NetworkTimeSyncService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
		}

		protected override void OnStop()
		{
		}
	}
}
