namespace GZipTest.Workflow
{
    public struct ProcessedBatchItem
    {
        public long JobBatchItemId { get; set; }
        public byte[] Processed { get; set; }
    }
}