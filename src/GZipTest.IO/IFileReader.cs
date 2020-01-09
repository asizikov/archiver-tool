using System.Collections.Generic;
using System.IO;

namespace GZipTest.IO
{
    public interface IFileReader
    {
        IEnumerable<FileChunk> Read(FileInfo path);
    }
}