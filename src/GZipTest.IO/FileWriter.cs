using System.IO;

namespace GZipTest.IO
{
    public class FileWriter : IFileWriter
    {
        public IFile OpenFile(FileInfo path, bool  compressed)
        {
            var fileStream = File.Create(path.FullName);
            if (compressed)
            {
                return new UncompressedFileStreamWrapper(fileStream);
            }
            return new CompressedFileStreamWrapper(fileStream);
        }
    }
}