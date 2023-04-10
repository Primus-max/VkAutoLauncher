﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

public class ProcessManager
{
    private readonly Dictionary<string, int> _tasks;
    private readonly Dictionary<string, DateTime> _lastRunTimes = new Dictionary<string, DateTime>();
    private readonly object _lockObject = new object();
    private readonly string _logFilePath;

    public ProcessManager(Dictionary<string, int> tasks)
    {
        _tasks = tasks;
        foreach (var task in tasks)
        {
            _lastRunTimes[task.Key] = DateTime.MinValue;
        }
        _logFilePath = "logGer.txt";
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
                            var sleepTime = interval - timeSinceLastRun;
                            LogDebug($"Waiting for {sleepTime.TotalSeconds} seconds before starting {path}...");
                            Thread.Sleep(sleepTime);
                        }

                        _lastRunTimes[path] = DateTime.Now;

                        LogInfo($"Starting process {path}...");
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = path,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };

                        if (!process.Start())
                        {
                            LogError($"Failed to start process {path}.");
                            continue;
                        }

                        var output = process.StandardOutput.ReadToEnd();
                        var errors = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            LogError($"Process {path} exited with code {process.ExitCode}. Output: {output}. Errors: {errors}.");
                            continue;
                        }

                        LogInfo($"Process {path} completed successfully. Output: {output}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"An exception occurred: {ex.ToString()}");
            }
            finally
            {
                Thread.Sleep(5000); // wait for 5 seconds before trying again
            }
        }
    }

    private void LogInfo(string message)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}";
        Console.WriteLine(logMessage);
        File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
    }

    private void LogDebug(string message)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DEBUG: {message}";
        Console.WriteLine(logMessage);
        File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
    }

    private void LogError(string message)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
        Console.WriteLine(logMessage);
        File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
    }
}
