using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GZipTest.Compression;
using GZipTest.IO;
using GZipTest.Workflow.IntegrationTests.Utils;
using Microsoft.IO;
using Shouldly;
using Xunit;

namespace GZipTest.Workflow.IntegrationTests
{
    public sealed class FileReadersIntegrationTests : IntegrationTestBase
    {
        private readonly Compressor compressor;
        private readonly Decompressor decompressor;

        public FileReadersIntegrationTests()
        {
            var streamManager = new RecyclableMemoryStreamManager();
            compressor = new Compressor(streamManager);
            decompressor = new Decompressor(streamManager);
        }

        [Fact]
        public void UncompressedChunksAreStoredAndRetrievedCorrectly()
        {
            var bufferOne = Encoding.ASCII.GetBytes("FileReadersIntegrationTests").AsSpan();
            var bufferTwo = Encoding.ASCII.GetBytes("FileReadersIntegrationTests part two").AsSpan();

            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                compressedFileStreamWrapper.Write(bufferOne);
                compressedFileStreamWrapper.Write(bufferTwo);
            }

            var compressedBufferedFileReader = new CompressedBufferedFileReader();
            var chunks = new List<FileChunk>();
            foreach (var chunk in compressedBufferedFileReader.Read(outputFile))
            {
                chunks.Add(chunk);
            }

            bufferOne.SequenceShouldBeEqualTo(chunks[0]);
            bufferTwo.SequenceShouldBeEqualTo(chunks[1]);
        }

        [Fact]
        public void CompressedChunksAreStoredRetrievedAndDecompressedCorrectly()
        {
            var bufferOne = Encoding.ASCII.GetBytes("FileReadersIntegrationTests");
            var bufferTwo = Encoding.ASCII.GetBytes("FileReadersIntegrationTests part two");

            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                var processedChunk = compressor.Process(bufferOne);
                compressedFileStreamWrapper.Write(processedChunk.Memory.Span);
                processedChunk = compressor.Process(bufferTwo);
                compressedFileStreamWrapper.Write(processedChunk.Memory.Span);
            }

            var compressedBufferedFileReader = new CompressedBufferedFileReader();
            var chunks = new List<ProcessedChunk>();

            foreach (var chunk in compressedBufferedFileReader.Read(outputFile))
            {
                var processedChunk = decompressor.Process(chunk.Memory);
                chunks.Add(processedChunk);
            }

            bufferOne.SequenceShouldBeEqualTo(chunks[0]);
            bufferTwo.SequenceShouldBeEqualTo(chunks[1]);
        }

        [Fact]
        public void CompressedChunksAreStoredSavedToFileAndReadBackCorrectly()
        {
            var bufferOne = Encoding.ASCII.GetBytes("FileReadersIntegrationTests");
            var bufferTwo = Encoding.ASCII.GetBytes("FileReadersIntegrationTests part two");
            var combinedBuffers = new byte[bufferOne.Length + bufferTwo.Length];

            Array.Copy(bufferOne, 0, combinedBuffers, 0, bufferOne.Length);
            Array.Copy(bufferTwo, 0, combinedBuffers, bufferOne.Length, bufferTwo.Length);

            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                var processedChunk = compressor.Process(bufferOne);
                compressedFileStreamWrapper.Write(processedChunk.Memory.Span);
                processedChunk = compressor.Process(bufferTwo);
                compressedFileStreamWrapper.Write(processedChunk.Memory.Span);
            }

            var compressedBufferedFileReader = new CompressedBufferedFileReader();
            var chunks = new List<ProcessedChunk>();
            foreach (var chunk in compressedBufferedFileReader.Read(outputFile))
            {
                chunks.Add(decompressor.Process(chunk.Memory));
            }

            bufferOne.SequenceShouldBeEqualTo(chunks[0]);
            bufferTwo.SequenceShouldBeEqualTo(chunks[1]);

            using (var decompressedFileStream = File.Open(decompressedFile.FullName, FileMode.Create))
            {
                using var uncompressedFileStreamWrapper = new UncompressedFileStreamWrapper(decompressedFileStream);
                foreach (var bytes in chunks)
                {
                    uncompressedFileStreamWrapper.Write(bytes.Memory.Span);
                }
            }

            combinedBuffers
                .SequenceEqual(File.ReadAllBytes(decompressedFile.FullName))
                .ShouldBeTrue();
        }

        [Fact]
        public void FileCompressedChunksAreStoredSavedToFileAndReadBackCorrectly()
        {
            File.AppendAllLines(inputFile.FullName, new[]
            {
                "hello world",
                "this is test, haha",
                "adsfasdfasdfasdfasdfasdfasfasdfasdasdfasdfasdfasdfasdfasdfasdfasdfasdfffffffffffffffffffffffffaafaf"
            });

            var uncompressedBufferedFileReader = new UncompressedBufferedFileReader();
            var buffers = new List<FileChunk>();
            foreach (var buffer in uncompressedBufferedFileReader.Read(inputFile))
            {
                buffers.Add(buffer);
            }

            var combinedBuffer = new byte[buffers.Sum(b => b.Memory.Length)];
            var copied = 0;
            for (int i = 0; i < buffers.Count; i++)
            {
                Array.Copy(buffers[i].Memory.Span.ToArray(), 0, combinedBuffer, copied, buffers[i].Memory.Length);
                copied += buffers[i].Memory.Length;
            }

            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                foreach (var buffer in buffers)
                {
                    var processedChunk = compressor.Process(buffer.Memory);
                    compressedFileStreamWrapper.Write(processedChunk.Memory.Span);
                }
            }

            var compressedBufferedFileReader = new CompressedBufferedFileReader();
            var chunks = new List<ProcessedChunk>();
            foreach (var chunk in compressedBufferedFileReader.Read(outputFile))
            {
                chunks.Add(decompressor.Process(chunk.Memory));
            }

            for (int i = 0; i < buffers.Count; i++)
            {
                buffers[i].SequenceShouldBeEqualTo(chunks[i]);
            }

            using (var decompressedFileStream = File.Open(decompressedFile.FullName, FileMode.Create))
            {
                using var uncompressedFileStreamWrapper = new UncompressedFileStreamWrapper(decompressedFileStream);
                foreach (var bytes in chunks)
                {
                    uncompressedFileStreamWrapper.Write(bytes.Memory.Span);
                }
            }

            inputFile.ShouldHaveSameContentAs(decompressedFile);
        }
    }
}