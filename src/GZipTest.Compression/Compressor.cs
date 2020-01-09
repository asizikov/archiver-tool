using System.IO.Compression;
using Microsoft.IO;

namespace GZipTest.Compression
{
    public sealed class Compressor : IByteProcessor
    {
        private RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        public Compressor(RecyclableMemoryStreamManager recyclableMemoryStreamManager)
        {
            this.recyclableMemoryStreamManager = recyclableMemoryStreamManager;
        }

        public byte[] Process(byte[] input, int bufferSize)
        {
            using var compressedMemoryStream = recyclableMemoryStreamManager.GetStream();
            using var zipStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress);
            zipStream.Write(input, 0, bufferSize);
            zipStream.Flush();
            return compressedMemoryStream.ToArray();
        }
    }
}