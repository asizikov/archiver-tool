using System;
using System.Collections.Generic;
using System.IO;

namespace GZipTest.IO
{
    public class UncompressedBufferedFileReader :IFileReader
    {
        private const long SIZE = 1000 * 1024;

        public IEnumerable<byte[]> Read(FileInfo path)
        {
            using var fileStream = path.OpenRead();
            using var binaryReader = new BinaryReader(fileStream);

            do
            {
                var buffer = new byte[SIZE];
                var bufferSize = binaryReader.Read(buffer, 0, buffer.Length);
                if (bufferSize == SIZE)
                {
                    yield return buffer;
                }
                else
                {
                    var last = new byte[bufferSize];
                    Array.ConstrainedCopy(buffer, 0, last, 0, bufferSize);
                    yield return last;
                }

            } while (binaryReader.BaseStream.Length > binaryReader.BaseStream.Position);
        }
    }
}
