using System;
using System.Text;
using Microsoft.IO;
using Shouldly;
using Xunit;

namespace GZipTest.Compression.Tests
{
    public class CompressionTests
    {
        private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        private readonly IByteProcessor compressor;
        private readonly IByteProcessor decompressor;

        public CompressionTests()
        {
            recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            compressor = new Compressor(recyclableMemoryStreamManager);
            decompressor = new Decompressor(recyclableMemoryStreamManager);
        }

        [Fact]
        public void CompressedMemoryCanBeDecompressed()
        {
            const string content = "some string to be compressed";
            var buffer = Encoding.ASCII.GetBytes(content);

            var compressed = compressor.Process(buffer, buffer.Length);
            var decompressed = decompressor.Process(compressed.Buffer, compressed.Size);
            Encoding.ASCII.GetString(decompressed.Buffer,0,decompressed.Size).ShouldBe(content);
        }

        [Fact]
        public void CompressDecompressInChunksProducesCorrectResult()
        {
            const string content = "some somewhat very looooooooooooooooooooooooooooooooooooooooooooooooooong string to be compressed";
            var buffer = Encoding.ASCII.GetBytes(content);
            var first = new byte[buffer.Length / 2];
            var second = new byte[buffer.Length - first.Length];
            Array.Copy(buffer, 0, first, 0, first.Length);
            Array.Copy(buffer, first.Length, second, 0, second.Length);

            var firstCompressed = compressor.Process(first, first.Length);
            var secondCompressed = compressor.Process(second, second.Length);

            var firstDecompressed = decompressor.Process(firstCompressed.Buffer, firstCompressed.Size);
            var secondDecompressed = decompressor.Process(secondCompressed.Buffer, secondCompressed.Size);

            var result = new byte[firstDecompressed.Size + secondDecompressed.Size];
            Array.Copy(firstDecompressed.Buffer, 0, result, 0, firstDecompressed.Size);
            Array.Copy(secondDecompressed.Buffer, 0, result, firstDecompressed.Size, secondDecompressed.Size);
            Encoding.ASCII.GetString(result).ShouldBe(content);
        }
    }
}