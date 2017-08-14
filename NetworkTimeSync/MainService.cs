using System.ServiceProcess;

namespace NetworkTimeSync
{
	public partial class MainService : ServiceBase
	{
		public MainService()
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
