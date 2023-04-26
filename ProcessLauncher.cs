using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        int intervalSeconds = 60; // интервал между запусками программ        

        DateTimeOffset startTime = DateTimeOffset.Now.AddSeconds(intervalSeconds);

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
                .StartAt(startTime)
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(interval * 60)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job, trigger);

            startTime = startTime.AddSeconds(intervalSeconds);
        }

        // Запускаю список задач
        this.scheduler.Start();

        // Проверяю работающие процессы из списка чтобы закрыть лишние браузеры и chromedriver
        Task.Run(async () =>
        {
            while (true)
            {
                bool allProcessesStopped = true;
                foreach (var kvp in processIntervals)
                {
                    string path = kvp.Key;
                    string lockFilePath = $"{path}.lock";
                    if (File.Exists(lockFilePath))
                    {
                        allProcessesStopped = false;
                        break;
                    }
                }

                if (allProcessesStopped)
                {
                    // Закрываю браузеры и драйверы если не один из процессов не запущен
                   await Processes.CheckRunningChromeAsync();
                   await Processes.CheckRunningChromeDriverAsync();

                    break;
                }

                // Частота проверки состояния процессов
                Task.Delay(3000).Wait();
            }
        });
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


            string lockFilePath = $"{path}.lock";

            if (File.Exists(lockFilePath))
            {
                //Console.WriteLine($"Process '{path}' is already running.");
                return;
            }

            try
            {
                File.Create(lockFilePath).Dispose();
                Process.Start(path).WaitForExit();
            }
            catch (Exception)
            {
                //Console.WriteLine($"Failed to start process '{path}': {ex.Message}");
            }
            finally
            {
                File.Delete(lockFilePath);
            }

            await Task.CompletedTask;
        }

    }

}
