namespace GZipTest.Workflow.Context
{
    public sealed class JobContext : IJobContext
    {
        public ExecutionResult Result { get; set; }
        public string Error { get; set; }
    }
}
