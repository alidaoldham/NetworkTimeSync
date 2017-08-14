using System.ServiceProcess;

namespace NetworkTimeSync
{
    internal static class Program
	{
		static void Main()
		{
		    var servicesToRun = new ServiceBase[]
		    {
		        new MainService()
		    };
		    ServiceBase.Run(servicesToRun);
		}
	}
}
