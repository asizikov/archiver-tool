using System.IO;
using System.IO.Compression;

namespace GZipTest.Compression
{
    public sealed class Decompressor : IByteProcessor
    {
        public byte[] Process(byte[] input)
        {
            using var compressedMemoryStream = new MemoryStream(input);
            using var zipStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            zipStream.CopyTo(resultStream);
            zipStream.Close();
            return resultStream.ToArray();
        }
    }
}