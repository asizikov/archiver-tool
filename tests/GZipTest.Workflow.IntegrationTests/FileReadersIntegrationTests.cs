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
            var bufferOne = Encoding.ASCII.GetBytes("FileReadersIntegrationTests");
            var bufferTwo = Encoding.ASCII.GetBytes("FileReadersIntegrationTests part two");

            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                compressedFileStreamWrapper.Write(bufferOne, bufferOne.Length);
                compressedFileStreamWrapper.Write(bufferTwo, bufferTwo.Length);
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
                var processedChunk = compressor.Process(bufferOne, bufferOne.Length);
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
                var processedChunk = compressor.Process(bufferOne, bufferOne.Length);
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

            bufferOne.SequenceShouldBeEqualTo(chunks[0]);
            bufferTwo.SequenceShouldBeEqualTo(chunks[1]);

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
                buffers[i].SequenceShouldBeEqualTo(chunks[i]);
            }

            using (var decompressedFileStream = File.Open(decompressedFile.FullName, FileMode.Create))
            {
                using var uncompressedFileStreamWrapper = new UncompressedFileStreamWrapper(decompressedFileStream);
                foreach (var bytes in chunks)
                {
                    uncompressedFileStreamWrapper.Write(bytes.Buffer, bytes.Size);
                }
            }

            inputFile.ShouldHaveSameContentAs(decompressedFile);
        }
    }
}