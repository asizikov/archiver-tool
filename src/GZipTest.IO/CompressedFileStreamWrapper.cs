using System.IO;

namespace GZipTest.IO
{
    public class CompressedFileStreamWrapper : IFile
    {
        private readonly Stream stream;
        private readonly BinaryWriter writer;

        public CompressedFileStreamWrapper(Stream stream)
        {
            this.stream = stream;
            this.writer = new BinaryWriter(stream);
        }

        public void Dispose()
        {
            writer.Dispose();
        }

        public void Write(byte[] buffer, int size)
        {
            writer.Write(size);
            writer.Write(buffer, 0, size);
        }
    }
}