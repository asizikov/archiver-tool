namespace GZipTest.Compression
{
    public struct ProcessedChunk
    {
        public ProcessedChunk(byte[] buffer, int size)
        {
            Buffer = buffer;
            Size = size;
        }

        public byte[] Buffer { get; set; }
        public int Size { get; set; }
    }
}