using System.Collections.Generic;
using System.IO;

namespace GZipTest.IO
{
    public interface IFileReader
    {
        IEnumerable<(byte[] buffer, int size)> Read(FileInfo path);
    }

}