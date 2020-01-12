using System;
using GZipTest.Workflow.JobConfiguration;

namespace GZipTest.Workflow.Context
{
    public sealed class JobContext : IJobContext
    {
        public long ElapsedTimeMilliseconds { get; set; }
        public long SubmittedId { get; set; }
        public long ProcessedId { get; set; }

        public Operation Operation { get; set; }
        public Exception Exception { get; private set; }
        public ExecutionResult Result { get; private set; }
        public string Error { get; private set; }
        public string ReportedBy { get; private set; }

        public void Failure(Exception ex, string message, string reportedBy = default)
        {
            Result = ExecutionResult.Failure;
            Error = message;
            ReportedBy = reportedBy;
            Exception = ex;
        }
    }
}