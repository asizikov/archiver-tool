using System.IO;

namespace GZipTest.IO
{
    public interface IFileWriter
    {
        IFile OpenFile(FileInfo path);
    }

    public class FileWriter : IFileWriter
    {
        public IFile OpenFile(FileInfo path)
        {
            var fileStream = File.Create(path.FullName);
           
            return new FileStreamWrapper(fileStream);
        }
    }

    public class FileStreamWrapper : IFile
    {
        private readonly FileStream stream;

        public FileStreamWrapper(FileStream stream) => this.stream = stream;
        public void Dispose() => stream?.Dispose();

        public void Write(byte[] buffer) => stream.Write(buffer, 0, buffer.Length);
    }
}