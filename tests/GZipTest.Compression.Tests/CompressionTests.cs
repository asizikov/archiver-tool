using System;
using System.Text;
using Shouldly;
using Xunit;

namespace GZipTest.Compression.Tests
{
    public class CompressionTests
    {
        private readonly IByteProcessor compressor = new Compressor();
        private readonly IByteProcessor decompressor = new Decompressor();

        [Fact]
        public void CompressedMemoryCanBeDecompressed()
        {
            const string content = "some string to be compressed";
            var buffer = Encoding.ASCII.GetBytes(content);

            var compressed = compressor.Process(buffer);
            var decompressed = decompressor.Process(compressed);
            Encoding.ASCII.GetString(decompressed).ShouldBe(content);
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

            var firstCompressed = compressor.Process(first);
            var secondCompressed = compressor.Process(second);

            var firstDecompressed = decompressor.Process(firstCompressed);
            var secondDecompressed = decompressor.Process(secondCompressed);

            var result = new byte[firstDecompressed.Length + secondDecompressed.Length];
            Array.Copy(firstDecompressed, 0, result, 0, firstDecompressed.Length);
            Array.Copy(secondDecompressed, 0, result, firstDecompressed.Length, secondDecompressed.Length);
            Encoding.ASCII.GetString(result).ShouldBe(content);
        }
    }
}