//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Threading;

//public class ProcessManager
//{

//    private readonly Dictionary<string, int> _tasks;
//    private readonly Dictionary<string, DateTime> _lastRunTimes = new Dictionary<string, DateTime>();
//    private readonly Dictionary<string, bool> _isRunning = new Dictionary<string, bool>();
//    private readonly object _lockObject = new object();
//    private readonly string _logFilePath;
//    private readonly Thread _thread;

//    public ProcessManager(Dictionary<string, int> tasks)
//    {
//        _tasks = tasks;
//        foreach (var task in tasks)
//        {
//            _lastRunTimes[task.Key] = DateTime.MinValue;
//            _isRunning[task.Key] = false;
//        }
//        _logFilePath = "logGer.txt";

//        _thread = new Thread(StartProcesses);
//        _thread.Start();
//    }

//    public void StopProcesses()
//    {
//        _thread.Abort();
//    }

//    public void StartProcesses()
//    {
//        while (true)
//        {
//            try
//            {
//                foreach (var task in _tasks)
//                {
//                    var path = task.Key;
//                    var interval = TimeSpan.FromHours(task.Value);

//                    lock (_lockObject)
//                    {
//                        var lastRunTime = _lastRunTimes[path];
//                        var timeSinceLastRun = DateTime.Now - lastRunTime;

//                        if (timeSinceLastRun < interval)
//                        {
//                            continue;
//                        }

//                        if (!_isRunning[path])
//                        {
//                            _isRunning[path] = true;
//                            _lastRunTimes[path] = DateTime.Now;

//                            var processThread = new Thread(() =>
//                            {
//                                try
//                                {
//                                    LogInfo($"Starting process {path}...");
//                                    var process = new Process
//                                    {
//                                        StartInfo = new ProcessStartInfo
//                                        {
//                                            FileName = path,
//                                            RedirectStandardOutput = true,
//                                            RedirectStandardError = true,
//                                            UseShellExecute = false,
//                                            CreateNoWindow = true
//                                        }
//                                    };

//                                    if (!process.Start())
//                                    {
//                                        LogError($"Failed to start process {path}.");
//                                        return;
//                                    }

//                                    var output = process.StandardOutput.ReadToEnd();
//                                    var errors = process.StandardError.ReadToEnd();
//                                    process.WaitForExit();

//                                    if (process.ExitCode != 0)
//                                    {
//                                        LogError($"Process {path} exited with code {process.ExitCode}. Output: {output}. Errors: {errors}.");
//                                        return;
//                                    }

//                                    LogInfo($"Process {path} completed successfully. Output: {output}");
//                                }
//                                finally
//                                {
//                                    lock (_lockObject)
//                                    {
//                                        _isRunning[path] = false;
//                                    }
//                                }
//                            });
//                            processThread.Start();
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                LogError($"An exception occurred: {ex.ToString()}");
//            }
//            finally
//            {
//                Thread.Sleep(5000); // wait for 5 seconds before trying again
//            }
//        }
//    }





//    private void LogInfo(string message)
//    {
//        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}";
//        Console.WriteLine(logMessage);
//        lock (_lockObject)
//        {
//            File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
//        }
//    }

//    private void LogDebug(string message)
//    {
//        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DEBUG: {message}";
//        Console.WriteLine(logMessage);
//        lock (_lockObject)
//        {
//            File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
//        }
//    }

//    private void LogError(string message)
//    {
//        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
//        Console.WriteLine(logMessage);
//        lock (_lockObject)
//        {
//            File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
//        }
//    }
//}
