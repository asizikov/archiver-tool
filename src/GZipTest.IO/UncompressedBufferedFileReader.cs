using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace GZipTest.IO
{
    public class UncompressedBufferedFileReader :IFileReader
    {
        private const int SIZE = 1024 * 1024;

        public IEnumerable<(byte[] buffer, int size)> Read(FileInfo path)
        {
            using var fileStream = path.OpenRead();
            using var binaryReader = new BinaryReader(fileStream);

            do
            {
                var buffer = ArrayPool<byte>.Shared.Rent(SIZE);
                var bufferSize = binaryReader.Read(buffer, 0, SIZE);
                yield return (buffer, bufferSize);
            } while (binaryReader.BaseStream.Length > binaryReader.BaseStream.Position);
        }
    }
}
