using System;
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

        public ProcessedChunk Process(Memory<byte> memory)
        {
            using var compressedMemoryStream = recyclableMemoryStreamManager.GetStream();
            using var zipStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress);

            zipStream.Write(memory.Span);
            zipStream.Flush();
            compressedMemoryStream.Position = 0;
            var size = (int) compressedMemoryStream.Length;
            var bytes = ArrayPool<byte>.Shared.Rent(size);
            var outMemory = new Memory<byte>(bytes, 0, bytes.Length);
            var bytesRead = compressedMemoryStream.Read(outMemory.Span);

            return new ProcessedChunk(bytes, outMemory.Slice(0, bytesRead));
        }
    }
}