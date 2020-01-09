using GZipTest.Compression;

namespace GZipTest.Workflow
{
    public struct ProcessedBatchItem
    {
        public long JobBatchItemId { get; set; }
        public ProcessedChunk Processed { get; set; }
    }
}