using Quartz;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class ProcessJob : IJob
{
    public void Execute(IJobExecutionContext context)
    {
        try
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string path = dataMap.GetString("path");

            Process.Start(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start process: {ex.Message}");
        }
    }

    Task IJob.Execute(IJobExecutionContext context)
    {
        throw new NotImplementedException();
    }
}
