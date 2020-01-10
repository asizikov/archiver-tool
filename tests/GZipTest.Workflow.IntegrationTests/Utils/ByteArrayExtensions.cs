using System.IO;
using System.Linq;
using GZipTest.Compression;
using GZipTest.IO;
using Shouldly;

namespace GZipTest.Workflow.IntegrationTests.Utils
{
    public static class ByteArrayExtensions
    {
        public static void SequenceShouldBeEqualTo(this byte[] array, FileChunk chunk)
            => array.SequenceEqual(chunk.Buffer.Take(chunk.Size)).ShouldBeTrue();

        public static void SequenceShouldBeEqualTo(this byte[] array, ProcessedChunk chunk)
            => array.SequenceEqual(chunk.Buffer.Take(chunk.Size)).ShouldBeTrue();

        public static void SequenceShouldBeEqualTo(this FileChunk fileChunk, ProcessedChunk processedChunk)
            => fileChunk.Buffer.Take(fileChunk.Size).SequenceEqual(processedChunk.Buffer.Take(processedChunk.Size))
                .ShouldBeTrue();

        public static void ShouldHaveSameContentAs(this FileInfo fileOne, FileInfo fileTwo)
            => File.ReadAllBytes(fileOne.FullName).SequenceEqual(File.ReadAllBytes(fileTwo.FullName))
                .ShouldBeTrue();
    }
}