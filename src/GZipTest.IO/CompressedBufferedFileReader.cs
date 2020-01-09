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

            var size = binaryReader.ReadInt32();
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            var bufferSize = binaryReader.Read(buffer, 0, size);
            while (bufferSize > 0)
            {
                if (bufferSize == size)
                {
                    yield return new FileChunk(buffer, size);
                }
                else
                {
                    throw new IOException("Unexpected end of file");
                }

                if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length)
                {
                    yield break;
                }

                size = binaryReader.ReadInt32();
                buffer = ArrayPool<byte>.Shared.Rent(size);
                bufferSize = binaryReader.Read(buffer, 0, size);
            }
        }
    }
}