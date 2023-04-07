using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public class ProcessManager
{
    private readonly Dictionary<string, int> _tasks;
    private readonly Dictionary<string, bool> _isRunning = new Dictionary<string, bool>();

    public ProcessManager(Dictionary<string, int> tasks)
    {
        _tasks = tasks;

        // инициализируем _isRunning для каждого задания значением false
        foreach (var task in _tasks.Keys)
        {
            _isRunning[task] = false;
        }
    }

    public void StartProcesses()
    {
        while (true)
        {
            foreach (var task in _tasks)
            {
                var path = task.Key;
                var interval = task.Value;

                if (!_isRunning[path])
                {
                    _isRunning[path] = true; // устанавливаем флаг блокировки для данного процесса
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = path
                        }
                    };

                    if (!process.Start())
                    {
                        // Failed to start process, handle error here
                    }

                    process.WaitForExit();

                    // освобождаем блокировку после завершения процесса
                    _isRunning[path] = false;
                }
            }

            // ждем перед запуском нового цикла процессов
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }
    }
}

