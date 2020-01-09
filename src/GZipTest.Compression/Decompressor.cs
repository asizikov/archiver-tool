using System.IO;
using System.IO.Compression;
using Microsoft.IO;

namespace GZipTest.Compression
{
    public sealed class Decompressor : IByteProcessor
    {
        private RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        public Decompressor(RecyclableMemoryStreamManager recyclableMemoryStreamManager)
        {
            this.recyclableMemoryStreamManager = recyclableMemoryStreamManager;
        }
        public byte[] Process(byte[] input, int bufferSize)
        {
            using var compressedMemoryStream = recyclableMemoryStreamManager.GetStream();
            compressedMemoryStream.Write(input, 0, bufferSize);
            compressedMemoryStream.Position = 0;
            using var zipStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress);
            using var resultStream = recyclableMemoryStreamManager.GetStream();
            zipStream.CopyTo(resultStream);
            zipStream.Flush();
            return resultStream.ToArray();
        }
    }
}