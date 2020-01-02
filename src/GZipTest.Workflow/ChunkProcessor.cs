namespace GZipTest.Workflow
{
    public class ChunkProcessor
    {
        private readonly JobQueue jobQueue;
        public bool Done { get; private set; }

        public ChunkProcessor(JobQueue jobQueue)
        {
            this.jobQueue = jobQueue;
        }

        public void Run()
        {
            var jobBatchItem = jobQueue.Dequeue();
            if (jobBatchItem == null)
            {
                Done = true;
                return;
            }
            //compress
            //write to output buffer
        }
    }
}