namespace GZipTest.Workflow.JobConfiguration
{
    public class Batching
    {
        public int ParallelWorkers { get; set; }
        public int OutputQueueMultiplier { get; set; }
        public int InputQueueMultiplier { get; set; }
    }
}