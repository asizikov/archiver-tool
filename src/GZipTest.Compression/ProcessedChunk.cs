using System;
using System.Buffers;

namespace GZipTest.Compression
{
    public struct ProcessedChunk
    {
        private byte[] buffer;
        public ProcessedChunk(byte[] buffer, in Memory<byte> memory)
        {
            this.buffer = buffer;
            Memory = memory;
        }

        public Memory<byte> Memory { get; }
        public void ReturnBuffer() => ArrayPool<byte>.Shared.Return(buffer);
    }
}