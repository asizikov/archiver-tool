using System;
using System.Collections.Generic;
using System.IO;

namespace GZipTest.IO
{
    public class BufferedFileReader :IFileReader
    {
        public IEnumerable<byte[]> Read(FileInfo path)
        {
            using var fileStream = path.OpenRead();
            var buffer = new byte[1024 * 64];
            var bufferSize = fileStream.Read(buffer, 0 , buffer.Length);
            while (bufferSize > 0)
            {
                yield return buffer;
                bufferSize = fileStream.Read(buffer, 0, buffer.Length);
            }
        }
    }
}
