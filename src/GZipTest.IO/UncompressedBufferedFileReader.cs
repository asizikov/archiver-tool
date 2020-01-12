using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace GZipTest.IO
{
    public class UncompressedBufferedFileReader : IFileReader
    {
        private const int SIZE = 1024 * 1024;

        public IEnumerable<FileChunk> Read(FileInfo path)
        {
            using var fileStream = path.OpenRead();
            var readBytes = 0;
            do
            {
                var buffer = ArrayPool<byte>.Shared.Rent(SIZE);
                var memory = new Memory<byte>(buffer, 0, buffer.Length);

                readBytes = fileStream.Read(memory.Span);
                yield return new FileChunk(buffer, memory.Slice(0, readBytes));
            } while (readBytes != 0);
        }
    }
}