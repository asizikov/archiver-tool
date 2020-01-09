using System.Buffers;
using System.IO.Compression;
using Microsoft.IO;

namespace GZipTest.Compression
{
    public sealed class Decompressor : IByteProcessor
    {
        private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        public Decompressor(RecyclableMemoryStreamManager recyclableMemoryStreamManager)
        {
            this.recyclableMemoryStreamManager = recyclableMemoryStreamManager;
        }

        public ProcessedChunk Process(byte[] input, int bufferSize)
        {
            using var compressedMemoryStream = recyclableMemoryStreamManager.GetStream();
            compressedMemoryStream.Write(input, 0, bufferSize);
            compressedMemoryStream.Position = 0;

            using var zipStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress);
            using var resultStream = recyclableMemoryStreamManager.GetStream();
            
            zipStream.CopyTo(resultStream);
            zipStream.Flush();
            resultStream.Position = 0;
            
            var size = (int) resultStream.Length;
            var bytes = ArrayPool<byte>.Shared.Rent(size);
            
            resultStream.Read(bytes, 0, size);
            return new ProcessedChunk(bytes, size);
        }
    }
}