using System.IO;
using System.IO.Compression;

namespace GZipTest.Compression
{
    public class Compressor : IByteProcessor
    {
        public byte[] Process(byte[] input, long id)
        {
            using var compressedMemoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress))
            {
                gzipStream.Write(input, 0, input.Length);
            }

            return compressedMemoryStream.ToArray();
        }
    }
}