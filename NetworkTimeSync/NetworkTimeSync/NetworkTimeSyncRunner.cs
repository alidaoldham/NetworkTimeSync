using System.Threading;
using System.Threading.Tasks;
using NetworkTimeSync.TimeServices.NetworkTimeService;
using NetworkTimeSync.TimeServices.WindowsTimeService;

namespace NetworkTimeSync.NetworkTimeSync
{
    public class NetworkTimeSyncRunner : NetworkTimeSyncInteractor
    {
        public NetworkTimeSyncRunner(NetworkTimeService networkTimeService, WindowsTimeService windowsTimeService) : base(
            networkTimeService, windowsTimeService)
        {
        }
    
        private CancellationTokenSource cts;
        private Task updateTimeRunner;
        private int updateIntervalInMilliseconds;

        public void Start()
        {
            if (!IsRunning())
            {
                cts = new CancellationTokenSource();
                updateTimeRunner = new Task(() => SyncTimeToNetwork(cts.Token), TaskCreationOptions.LongRunning);
                updateTimeRunner.Start();
            }
        }

        protected bool IsRunning()
        {
            return updateTimeRunner != null && 
                   updateTimeRunner.Status != TaskStatus.Created &&
                   !updateTimeRunner.IsCompleted;
        }

        private void SyncTimeToNetwork(CancellationToken ct)
        {
            SyncTimeToNetwork();
            while (!ct.IsCancellationRequested)
            {
                WaitForUpdateIntervalToPass(ct, updateIntervalInMilliseconds);
                if (!ct.IsCancellationRequested)
                    SyncTimeToNetwork();
            }
        }

        protected virtual void WaitForUpdateIntervalToPass(CancellationToken ct, int intervalInMilliseconds)
        {
            SpinWait.SpinUntil(() => ct.IsCancellationRequested, intervalInMilliseconds);
        }

        protected virtual void SyncTimeToNetwork()
        {
            if (Execute())
                updateIntervalInMilliseconds = 10 * 60 * 1000;
            else
                updateIntervalInMilliseconds = 10 * 1000;
        }

        public void Stop()
        {
            if (IsRunning())
            {
                cts.Cancel();
                updateTimeRunner.Wait();
            }
        }
    }
}
