namespace GZipTest.Workflow.Context
{
    public interface IJobContext
    {
        ExecutionResult Result { get; set; }
        string Error { get; set; }
        long ElapsedTimeMilliseconds { get; set; }
    }
}