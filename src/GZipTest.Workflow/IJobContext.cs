namespace GZipTest.Workflow
{
    public interface IJobContext
    {
        ExecutionResult Result { get; set; }
        string Error { get; set; }
    }
}