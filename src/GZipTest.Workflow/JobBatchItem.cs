namespace GZipTest.Workflow
{
    public class JobBatchItem
    {
        public byte[] Buffer { get; set; }
        public long JobBatchItemId { get; set; }
        public byte[] Processed { get; set; }
        public long ElapsedTime { get; set; }
    }
}