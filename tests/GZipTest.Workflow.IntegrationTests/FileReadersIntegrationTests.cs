using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GZipTest.Compression;
using GZipTest.IO;
using Shouldly;
using Xunit;

namespace GZipTest.Workflow.IntegrationTests
{
    public sealed class FileReadersIntegrationTests : IntegrationTestBase
    {
        [Fact]
        public void UncompressedChunksAreStoredAndRetrievedCorrectly()
        {
            var buffer = Encoding.ASCII.GetBytes("FileReadersIntegrationTests");
            var bufferTwo = Encoding.ASCII.GetBytes("FileReadersIntegrationTests part two");
            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                compressedFileStreamWrapper.Write(buffer);
                compressedFileStreamWrapper.Write(bufferTwo);
            }

            var compressedBufferedFileReader = new CompressedBufferedFileReader();
            var chunks = new List<byte[]>();
            foreach (var chunk in compressedBufferedFileReader.Read(outputFile))
            {
                chunks.Add(chunk);
            }

            buffer.SequenceEqual(chunks[0]).ShouldBeTrue();
            bufferTwo.SequenceEqual(chunks[1]).ShouldBeTrue();
        }

        [Fact]
        public void CompressedChunksAreStoredRetrievedAndDecompressedCorrectly()
        {
            var compressor = new Compressor();
            var buffer = Encoding.ASCII.GetBytes("FileReadersIntegrationTests");
            var bufferTwo = Encoding.ASCII.GetBytes("FileReadersIntegrationTests part two");
            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                compressedFileStreamWrapper.Write(compressor.Process(buffer));
                compressedFileStreamWrapper.Write(compressor.Process(bufferTwo));
            }

            var decompressor = new Decompressor();
            var compressedBufferedFileReader = new CompressedBufferedFileReader();
            var chunks = new List<byte[]>();
            foreach (var chunk in compressedBufferedFileReader.Read(outputFile))
            {
                chunks.Add(decompressor.Process(chunk));
            }

            buffer.SequenceEqual(chunks[0]).ShouldBeTrue();
            bufferTwo.SequenceEqual(chunks[1]).ShouldBeTrue();
        }
    }
}