using System.IO;

namespace GZipTest.IO
{
    public interface IFileWriter
    {
        void Write(FileInfo path);
    }
}