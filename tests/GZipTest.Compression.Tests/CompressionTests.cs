using System;
using System.Linq;
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
            var memory = new Memory<byte>(buffer);
            var compressed = compressor.Process(memory);
            var decompressed = decompressor.Process(compressed.Memory);
            Encoding.ASCII.GetString(decompressed.Memory.ToArray(), 0, decompressed.Memory.Length).ShouldBe(content);
        }

        [Fact]
        public void CompressDecompressInChunksProducesCorrectResult()
        {
            const string content =
                "some somewhat very looooooooooooooooooooooooooooooooooooooooooooooooooong string to be compressed";
            var buffer = Encoding.ASCII.GetBytes(content);
            var first = new byte[buffer.Length / 2];
            var second = new byte[buffer.Length - first.Length];
            Array.Copy(buffer, 0, first, 0, first.Length);
            Array.Copy(buffer, first.Length, second, 0, second.Length);

            var firstCompressed = compressor.Process(new Memory<byte>(first));
            var secondCompressed = compressor.Process(new Memory<byte>(second));

            var firstDecompressed = decompressor.Process(firstCompressed.Memory);
            var secondDecompressed = decompressor.Process(secondCompressed.Memory);

            var result = new byte[firstDecompressed.Memory.Length + secondDecompressed.Memory.Length];
            Array.Copy(firstDecompressed.Memory.ToArray(), 0, result, 0, firstDecompressed.Memory.Length);
            Array.Copy(secondDecompressed.Memory.ToArray(), 0, result, firstDecompressed.Memory.Length, secondDecompressed.Memory.Length);
            Encoding.ASCII.GetString(result).ShouldBe(content);
        }
    }
}