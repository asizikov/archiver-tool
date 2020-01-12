using System;
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
            writer?.Dispose();
            stream?.Dispose();
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
            writer.Write(buffer.Length);
            writer.Write(buffer);
        }
    }
}