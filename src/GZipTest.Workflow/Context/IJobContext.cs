using System;
using System.Runtime.CompilerServices;
using GZipTest.Workflow.JobConfiguration;

namespace GZipTest.Workflow.Context
{
    public interface IJobContext
    {
        long ElapsedTimeMilliseconds { get; set; }
        long SubmittedId { get; set; }
        long ProcessedId { get; set; }
        Operation Operation { get; set; }
        Exception Exception { get; }
        ExecutionResult Result { get; }
        string Error { get; }
        string ReportedBy { get; }

        void Failure(Exception ex, string message, [CallerFilePath] string reportedBy = default);
    }
}