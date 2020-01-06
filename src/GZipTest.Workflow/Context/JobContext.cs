namespace GZipTest.Workflow.Context
{
    public sealed class JobContext : IJobContext
    {
        public ExecutionResult Result { get; set; }
        public string Error { get; set; }
        public long ElapsedTimeMilliseconds { get; set; }
        public long SubmittedId { get; set; }
        public long ProcessedId { get; set; }
    }
}
