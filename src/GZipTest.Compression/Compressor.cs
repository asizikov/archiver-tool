using System.Buffers;
using System.IO.Compression;
using Microsoft.IO;

namespace GZipTest.Compression
{
    public sealed class Compressor : IByteProcessor
    {
        private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        public Compressor(RecyclableMemoryStreamManager recyclableMemoryStreamManager)
        {
            this.recyclableMemoryStreamManager = recyclableMemoryStreamManager;
        }

        public ProcessedChunk Process(byte[] input, int bufferSize)
        {
            using var compressedMemoryStream = recyclableMemoryStreamManager.GetStream();
            using var zipStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress);

            zipStream.Write(input, 0, bufferSize);
            zipStream.Flush();
            compressedMemoryStream.Position = 0;

            var size = (int)compressedMemoryStream.Length;
            var bytes = ArrayPool<byte>.Shared.Rent(size);
            
            compressedMemoryStream.Read(bytes, 0, size);
            return new ProcessedChunk(bytes,size);
        }
    }
}