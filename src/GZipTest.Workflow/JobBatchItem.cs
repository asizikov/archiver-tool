namespace GZipTest.Workflow
{
    public struct JobBatchItem
    {
        public byte[] Buffer { get; set; }
        public long JobBatchItemId { get; set; }
    }


    public struct ProcessedBatchItem
    {
        public long JobBatchItemId { get; set; }
        public byte[] Processed { get; set; }
    }
}