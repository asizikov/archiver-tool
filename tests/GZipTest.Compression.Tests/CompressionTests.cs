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
        private readonly IByteProcessor compressor;
        private readonly IByteProcessor decompressor;

        public CompressionTests()
        {
            var recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
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

            Encoding.ASCII.GetString(decompressed.Memory.Span).ShouldBe(content);
        }

        [Fact]
        public void CompressDecompressInChunksProducesCorrectResult()
        {
            const string content =
                "some somewhat very looooooooooooooooooooooooooooooooooooooooooooooooooong string to be compressed";
            var buffer = new Memory<byte>(Encoding.ASCII.GetBytes(content));

            var first = buffer.Slice(0, buffer.Length/2);
            var second = buffer.Slice(first.Length, buffer.Length - first.Length);

            var firstCompressed = compressor.Process(first);
            var secondCompressed = compressor.Process(second);

            var firstDecompressed = decompressor.Process(firstCompressed.Memory);
            var secondDecompressed = decompressor.Process(secondCompressed.Memory);

            Span<byte> result = stackalloc byte[firstDecompressed.Memory.Length + secondDecompressed.Memory.Length];
            firstDecompressed.Memory.Span.CopyTo(result.Slice(0,firstDecompressed.Memory.Length));
            secondDecompressed.Memory.Span.CopyTo(result.Slice(firstDecompressed.Memory.Length, secondDecompressed.Memory.Length));

            Encoding.ASCII.GetString(result).ShouldBe(content);
        }
    }

}