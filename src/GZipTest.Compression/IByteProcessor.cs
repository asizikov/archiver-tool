using System;

namespace GZipTest.Compression
{
    public interface IByteProcessor
    {
        ProcessedChunk Process(Memory<byte> memory);
    }
}