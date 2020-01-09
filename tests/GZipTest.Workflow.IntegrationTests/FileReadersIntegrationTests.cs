using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GZipTest.Compression;
using GZipTest.IO;
using Microsoft.IO;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace GZipTest.Workflow.IntegrationTests
{
    public sealed class FileReadersIntegrationTests : IntegrationTestBase
    {
        private readonly ITestOutputHelper outputHelper;
        private readonly Compressor compressor;
        private readonly Decompressor decompressor;

        public FileReadersIntegrationTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
            var streamManager = new RecyclableMemoryStreamManager();
            compressor = new Compressor(streamManager);
            decompressor = new Decompressor(streamManager);
        }

        [Fact]
        public void UncompressedChunksAreStoredAndRetrievedCorrectly()
        {
            var buffer = Encoding.ASCII.GetBytes("FileReadersIntegrationTests");
            var bufferTwo = Encoding.ASCII.GetBytes("FileReadersIntegrationTests part two");
            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                compressedFileStreamWrapper.Write(buffer, buffer.Length);
                compressedFileStreamWrapper.Write(bufferTwo, bufferTwo.Length);
            }

            var compressedBufferedFileReader = new CompressedBufferedFileReader();
            var chunks = new List<FileChunk>();
            foreach (var chunk in compressedBufferedFileReader.Read(outputFile))
            {
                chunks.Add(chunk);
            }

            buffer.SequenceEqual(chunks[0].Buffer.Take(chunks[0].Size)).ShouldBeTrue();
            bufferTwo.SequenceEqual(chunks[1].Buffer.Take(chunks[1].Size)).ShouldBeTrue();
        }

        [Fact]
        public void CompressedChunksAreStoredRetrievedAndDecompressedCorrectly()
        {
            var buffer = Encoding.ASCII.GetBytes("FileReadersIntegrationTests");
            var bufferTwo = Encoding.ASCII.GetBytes("FileReadersIntegrationTests part two");
            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                var processedChunk = compressor.Process(buffer, buffer.Length);
                compressedFileStreamWrapper.Write(processedChunk.Buffer, processedChunk.Size);
                processedChunk = compressor.Process(bufferTwo, bufferTwo.Length);
                compressedFileStreamWrapper.Write(processedChunk.Buffer, processedChunk.Size);
            }

            var compressedBufferedFileReader = new CompressedBufferedFileReader();
            var chunks = new List<ProcessedChunk>();
            foreach (var chunk in compressedBufferedFileReader.Read(outputFile))
            {
                var processedChunk = decompressor.Process(chunk.Buffer, chunk.Size);
                chunks.Add(processedChunk);
            }

            buffer.SequenceEqual(chunks[0].Buffer.Take(chunks[0].Size)).ShouldBeTrue();
            bufferTwo.SequenceEqual(chunks[1].Buffer.Take(chunks[1].Size)).ShouldBeTrue();
        }

        [Fact]
        public void CompressedChunksAreStoredSavedToFileAndReadBackCorrectly()
        {
            var buffer = Encoding.ASCII.GetBytes("FileReadersIntegrationTests");
            var bufferTwo = Encoding.ASCII.GetBytes("FileReadersIntegrationTests part two");
            var combinedBuffers = new byte[buffer.Length + bufferTwo.Length];
            Array.Copy(buffer, 0, combinedBuffers, 0, buffer.Length);
            Array.Copy(bufferTwo, 0, combinedBuffers, buffer.Length, bufferTwo.Length);

            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                var processedChunk = compressor.Process(buffer, buffer.Length);
                compressedFileStreamWrapper.Write(processedChunk.Buffer, processedChunk.Size);
                processedChunk = compressor.Process(bufferTwo, bufferTwo.Length);
                compressedFileStreamWrapper.Write(processedChunk.Buffer, processedChunk.Size);
            }

            var compressedBufferedFileReader = new CompressedBufferedFileReader();
            var chunks = new List<ProcessedChunk>();
            foreach (var chunk in compressedBufferedFileReader.Read(outputFile))
            {
                chunks.Add(decompressor.Process(chunk.Buffer, chunk.Size));
            }

            buffer.SequenceEqual(chunks[0].Buffer.Take(chunks[0].Size)).ShouldBeTrue();
            bufferTwo.SequenceEqual(chunks[1].Buffer.Take(chunks[1].Size)).ShouldBeTrue();

            using (var decompressedFileStream = File.Open(decompressedFile.FullName, FileMode.Create))
            {
                using var uncompressedFileStreamWrapper = new UncompressedFileStreamWrapper(decompressedFileStream);
                foreach (var bytes in chunks)
                {
                    uncompressedFileStreamWrapper.Write(bytes.Buffer, bytes.Size);
                }
            }

            combinedBuffers
                .SequenceEqual(File.ReadAllBytes(decompressedFile.FullName))
                .ShouldBeTrue();
        }

        [Fact]
        public void FileCompressedChunksAreStoredSavedToFileAndReadBackCorrectly()
        {
            File.AppendAllLines(inputFile.FullName, new []
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

            var combinedBuffer = new byte[buffers.Sum(b => b.Size)];
            var copied = 0;
            for (int i = 0; i < buffers.Count; i++)
            {
                Array.Copy(buffers[i].Buffer,0,combinedBuffer, copied, buffers[i].Size);
                copied += buffers[i].Size;
            }
            outputHelper.WriteLine(Encoding.ASCII.GetString(combinedBuffer));
            outputHelper.WriteLine("buffers " + buffers.Count);
            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                foreach (var buffer in buffers)
                {
                    var processedChunk = compressor.Process(buffer.Buffer, buffer.Size);
                    compressedFileStreamWrapper.Write(processedChunk.Buffer, processedChunk.Size);
                }
            }

            var compressedBufferedFileReader = new CompressedBufferedFileReader();
            var chunks = new List<ProcessedChunk>();
            foreach (var chunk in compressedBufferedFileReader.Read(outputFile))
            {
                chunks.Add(decompressor.Process(chunk.Buffer, chunk.Size));
            }

            for (int i = 0; i < buffers.Count; i++)
            {
                buffers[i].Buffer.Take(buffers[i].Size).SequenceEqual(chunks[i].Buffer.Take(chunks[i].Size)).ShouldBeTrue();
            }

            using (var decompressedFileStream = File.Open(decompressedFile.FullName, FileMode.Create))
            {
                using var uncompressedFileStreamWrapper = new UncompressedFileStreamWrapper(decompressedFileStream);
                foreach (var bytes in chunks)
                {
                    uncompressedFileStreamWrapper.Write(bytes.Buffer, bytes.Size);
                }
            }

            File.ReadAllBytes(inputFile.FullName)
                .SequenceEqual(File.ReadAllBytes(decompressedFile.FullName))
                .ShouldBeTrue();
        }
    }
}