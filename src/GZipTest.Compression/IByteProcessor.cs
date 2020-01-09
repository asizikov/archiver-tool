namespace GZipTest.Compression
{
    public interface IByteProcessor
    {
        byte[] Process(byte[] input, int bufferSize);
    }
}