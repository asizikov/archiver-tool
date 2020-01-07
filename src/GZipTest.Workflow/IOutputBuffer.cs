namespace GZipTest.Workflow
{
    public interface IOutputBuffer
    {
        void SubmitProcessedBatchItem(ProcessedBatchItem processedBatchItem);
        void SubmitCompleted();
    }
}