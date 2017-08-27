using System.ServiceProcess;

namespace NetworkTimeSync
{
    public static class Program
	{
		public static void Main()
		{
		    var servicesToRun = new ServiceBase[]
            {
                new NetworkTimeSyncService()
            };
             ServiceBase.Run(servicesToRun);
		}
	}
}