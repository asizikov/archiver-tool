using System;
using System.Collections.Generic;
using System.IO;

namespace GZipTest.IO
{
    public class BufferedFileReader :IFileReader
    {
        private const long SIZE = 1024 * 256;
        
        public IEnumerable<byte[]> Read(FileInfo path)
        {
            int count = 0;
            using var fileStream = path.OpenRead();
            var buffer = new byte[SIZE];
            var bufferSize = fileStream.Read(buffer, 0 , buffer.Length);
            while (bufferSize > 0)
            {
                count++;
                if(count == 10) throw new IOException("boo");
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
                bufferSize = fileStream.Read(buffer, 0, buffer.Length);
            }
        }
    }
}
