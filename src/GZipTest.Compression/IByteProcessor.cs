namespace GZipTest.Compression
{
    public interface IByteProcessor
    {
        ProcessedChunk Process(byte[] input, int bufferSize);
    }
}