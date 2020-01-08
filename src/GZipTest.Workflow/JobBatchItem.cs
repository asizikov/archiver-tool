namespace GZipTest.Workflow
{
    public struct JobBatchItem
    {
        public byte[] Buffer { get; set; }
        public long JobBatchItemId { get; set; }
    }
}