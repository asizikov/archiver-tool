using System;
using System.IO;
using System.Linq;
using GZipTest.Compression;
using GZipTest.IO;
using Shouldly;

namespace GZipTest.Workflow.IntegrationTests.Utils
{
    public static class ByteArrayExtensions
    {
        public static void SequenceShouldBeEqualTo(this Span<byte> span, FileChunk chunk)
            => span.SequenceEqual(chunk.Memory.Span).ShouldBeTrue();

        public static void SequenceShouldBeEqualTo(this byte[] array, ProcessedChunk chunk)
            => array.AsSpan().SequenceEqual(chunk.Memory.Span).ShouldBeTrue();

        public static void SequenceShouldBeEqualTo(this FileChunk fileChunk, ProcessedChunk processedChunk)
            => fileChunk.Memory.Span.SequenceEqual(processedChunk.Memory.Span)
                .ShouldBeTrue();

        public static void ShouldHaveSameContentAs(this FileInfo fileOne, FileInfo fileTwo)
            => File.ReadAllBytes(fileOne.FullName).SequenceEqual(File.ReadAllBytes(fileTwo.FullName))
                .ShouldBeTrue();
    }
}