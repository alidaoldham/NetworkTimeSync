﻿using System.Threading;
using System.Threading.Tasks;
using NetworkTimeSync.TimeServices.NetworkTimeService;
using NetworkTimeSync.TimeServices.WindowsTimeService;

namespace NetworkTimeSync.UpdateTime
{
    public class UpdateTimeRunner : UpdateTimeInteractor
    {
        public UpdateTimeRunner(NetworkTimeService networkTimeService, WindowsTimeService windowsTimeService) : base(
            networkTimeService, windowsTimeService)
        {
            updateIntervalInMilliseconds = 10 * 60 * 1000;
        }
    
        private CancellationTokenSource cts;
        private Task updateTimeRunner;
        private readonly int updateIntervalInMilliseconds;

        public void Start()
        {
            if (!IsRunning())
            {
                cts = new CancellationTokenSource();
                updateTimeRunner = new Task(() => UpdateTime(cts.Token), TaskCreationOptions.LongRunning);
                updateTimeRunner.Start();
            }
        }

        protected bool IsRunning()
        {
            return updateTimeRunner != null && 
                   updateTimeRunner.Status != TaskStatus.Created &&
                   !updateTimeRunner.IsCompleted;
        }

        private void UpdateTime(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                WaitForUpdateIntervalToPass(ct);
                if (!ct.IsCancellationRequested)
                    UpdateTime();
            }
        }

        protected virtual void WaitForUpdateIntervalToPass(CancellationToken ct)
        {
            SpinWait.SpinUntil(() => ct.IsCancellationRequested, updateIntervalInMilliseconds);
        }

        protected virtual void UpdateTime()
        {
            Execute();   
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
