using System.Collections.Generic;
using System.IO;

namespace GZipTest.IO
{
    public class CompressedBufferedFileReader : IFileReader
    {
        public IEnumerable<byte[]> Read(FileInfo path)
        {
            using var fileStream = path.OpenRead();
            using var binaryReader = new BinaryReader(fileStream);

            var size = binaryReader.ReadInt32();
            var buffer = new byte[size];
            var bufferSize = fileStream.Read(buffer, 0, buffer.Length);
            while (bufferSize > 0)
            {
                if (bufferSize == size)
                {
                    yield return buffer;
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
                buffer = new byte[size];
                bufferSize = fileStream.Read(buffer, 0, buffer.Length);
            }
        }
    }
}