using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GZipTest.Compression;
using GZipTest.IO;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace GZipTest.Workflow.IntegrationTests
{
    public sealed class FileReadersIntegrationTests : IntegrationTestBase
    {
        private readonly ITestOutputHelper outputHelper;

        public FileReadersIntegrationTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

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

        [Fact]
        public void CompressedChunksAreStoredSavedToFileAndReadBackCorrectly()
        {
            var buffer = Encoding.ASCII.GetBytes("FileReadersIntegrationTests");
            var bufferTwo = Encoding.ASCII.GetBytes("FileReadersIntegrationTests part two");
            var combinedBuffers = new byte[buffer.Length + bufferTwo.Length];
            Array.Copy(buffer, 0, combinedBuffers, 0, buffer.Length);
            Array.Copy(bufferTwo, 0, combinedBuffers, buffer.Length, bufferTwo.Length);

            var compressor = new Compressor();
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

            using (var decompressedFileStream = File.Open(decompressedFile.FullName, FileMode.Create))
            {
                using var uncompressedFileStreamWrapper = new UncompressedFileStreamWrapper(decompressedFileStream);
                foreach (var bytes in chunks)
                {
                    uncompressedFileStreamWrapper.Write(bytes);
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
            var buffers = new List<byte[]>();
            foreach (var buffer in uncompressedBufferedFileReader.Read(inputFile))
            {
                buffers.Add(buffer);
            }

            var combinedBuffer = new byte[buffers.Sum(b => b.Length)];
            var copied = 0;
            for (int i = 0; i < buffers.Count; i++)
            {
                Array.Copy(buffers[i],0,combinedBuffer, copied, buffers[i].Length);
                copied += buffers[i].Length;
            }
            outputHelper.WriteLine(Encoding.ASCII.GetString(combinedBuffer));
            outputHelper.WriteLine("buffers " + buffers.Count);
            var compressor = new Compressor();
            using (var fileStream = File.Open(outputFile.FullName, FileMode.Create))
            {
                using var compressedFileStreamWrapper = new CompressedFileStreamWrapper(fileStream);
                foreach (var buffer in buffers)
                {
                    compressedFileStreamWrapper.Write(compressor.Process(buffer));
                }
            }

            var decompressor = new Decompressor();
            var compressedBufferedFileReader = new CompressedBufferedFileReader();
            var chunks = new List<byte[]>();
            foreach (var chunk in compressedBufferedFileReader.Read(outputFile))
            {
                chunks.Add(decompressor.Process(chunk));
            }

            for (int i = 0; i < buffers.Count; i++)
            {
                buffers[i].SequenceEqual(chunks[i]).ShouldBeTrue();
            }

            using (var decompressedFileStream = File.Open(decompressedFile.FullName, FileMode.Create))
            {
                using var uncompressedFileStreamWrapper = new UncompressedFileStreamWrapper(decompressedFileStream);
                foreach (var bytes in chunks)
                {
                    uncompressedFileStreamWrapper.Write(bytes);
                }
            }

            File.ReadAllBytes(inputFile.FullName)
                .SequenceEqual(File.ReadAllBytes(decompressedFile.FullName))
                .ShouldBeTrue();
        }
    }
}