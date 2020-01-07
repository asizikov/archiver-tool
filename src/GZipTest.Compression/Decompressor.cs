using System.IO;
using System.IO.Compression;

namespace GZipTest.Compression
{
    public class Decompressor : IByteProcessor
    {
        public byte[] Process(byte[] input, long id)
        {
            using var decompressedMemoryStream = new MemoryStream(input);
            using (var gzipStream = new GZipStream(decompressedMemoryStream, CompressionMode.Decompress))
            {
                gzipStream.Read(input, 0, input.Length);
            }

            return decompressedMemoryStream.ToArray();
        }
    }
}