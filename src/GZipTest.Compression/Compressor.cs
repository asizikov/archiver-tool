using System.IO;
using System.IO.Compression;

namespace GZipTest.Compression
{
    public sealed class Compressor : IByteProcessor
    {
        public byte[] Process(byte[] input)
        {
            using var compressedMemoryStream = new MemoryStream();
            using var zipStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress);
            zipStream.Write(input, 0, input.Length);
            zipStream.Close();
            return compressedMemoryStream.ToArray();
        }
    }
}