using System.IO;

namespace GZipTest.IO
{
    public interface IFileWriter
    {
        IFile OpenFile(FileInfo path, bool compressed);
    }
}