namespace GZipTest.IO
{
    public struct FileChunk
    {
        public FileChunk(byte[] buffer, int size)
        {
            Buffer = buffer;
            Size = size;
            JobBatchItemId = 0;
        }

        public byte[] Buffer { get; set; }
        public int Size { get; set; }
        public long JobBatchItemId { get; set; }
    }
}