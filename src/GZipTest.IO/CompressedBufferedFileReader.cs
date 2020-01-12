using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace GZipTest.IO
{
    public class CompressedBufferedFileReader : IFileReader
    {
        public IEnumerable<FileChunk> Read(FileInfo path)
        {
            using var fileStream = path.OpenRead();
            using var binaryReader = new BinaryReader(fileStream);
            var readBytes = 0;
            do
            {
                var size = binaryReader.ReadInt32();
                var buffer = ArrayPool<byte>.Shared.Rent(size);
                var memory = new Memory<byte>(buffer, 0, size);
                readBytes = binaryReader.Read(memory.Span);
                if (readBytes == size)
                {
                    yield return new FileChunk(buffer, memory.Slice(0, readBytes));
                }
                else
                {
                    throw new IOException("Unexpected end of file");
                }
            } while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length);
        }
    }
}