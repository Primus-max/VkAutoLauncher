using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class QuartzProcessLauncher
{
    private IScheduler scheduler;

    public QuartzProcessLauncher()
    {
        this.scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
    }

    public void Start(Dictionary<string, int> processIntervals)
    {
        foreach (var kvp in processIntervals)
        {
            string path = kvp.Key;
            int interval = kvp.Value;

            var job = JobBuilder.Create<ProcessJob>()
                .WithIdentity(path, "process")
                .UsingJobData("Path", path)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity(path, "process")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(interval * 60)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job, trigger);
        }

        this.scheduler.Start();
    }



    public void Stop()
    {
        this.scheduler.Shutdown();
    }

    [DisallowConcurrentExecution]
    private class ProcessJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            string? path = context?.JobDetail.JobDataMap.GetString("Path");

            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start process '{path}': {ex.Message}");
            }

            await Task.CompletedTask;
        }
    }

}
