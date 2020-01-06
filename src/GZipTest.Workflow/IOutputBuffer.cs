namespace GZipTest.Workflow
{
    public interface IOutputBuffer
    {
        void SubmitProcessedBatchItem(JobBatchItem processedBatchItem);
        void SubmitCompleted();
    }
}