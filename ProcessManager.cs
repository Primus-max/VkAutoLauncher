using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NLog;

public class ProcessManager
{
    private readonly Dictionary<string, int> _tasks;
    private readonly Dictionary<string, DateTime> _lastRunTimes = new Dictionary<string, DateTime>();
    private readonly object _lockObject = new object();

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public ProcessManager(Dictionary<string, int> tasks)
    {
        _tasks = tasks;
        foreach (var task in tasks)
        {
            _lastRunTimes[task.Key] = DateTime.MinValue;
        }
    }

    public void StartProcesses()
    {
        while (true)
        {
            try
            {
                foreach (var task in _tasks)
                {
                    var path = task.Key;
                    var interval = TimeSpan.FromMinutes(task.Value);

                    lock (_lockObject)
                    {
                        var lastRunTime = _lastRunTimes[path];
                        var timeSinceLastRun = DateTime.Now - lastRunTime;

                        if (timeSinceLastRun < interval)
                        {
                            // ждем до окончания периода
                            Thread.Sleep(interval - timeSinceLastRun);
                        }

                        _lastRunTimes[path] = DateTime.Now;

                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = path
                            }
                        };

                        if (!process.Start())
                        {
                            Logger.Error($"Failed to start process {path}");
                        }
                        else
                        {
                            Logger.Info($"Started process {path}");
                        }

                        process.WaitForExit();

                        Logger.Info($"Process {path} finished with exit code {process.ExitCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unhandled exception in ProcessManager");
            }
        }
    }
}
