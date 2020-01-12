using System;
using System.Buffers;

namespace GZipTest.IO
{
    public struct FileChunk
    {
        private byte[] buffer;

        public FileChunk(byte[] buffer, Memory<byte> memory)
        {
            this.buffer = buffer;
            Memory = memory;
            JobBatchItemId = 0;
        }

        public Memory<byte> Memory { get; }
        public long JobBatchItemId { get; set; }

        public void ReleaseBuffer() => ArrayPool<byte>.Shared.Return(buffer);
    }
}