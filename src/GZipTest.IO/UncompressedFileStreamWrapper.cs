using System;
using System.IO;

namespace GZipTest.IO
{
    public class UncompressedFileStreamWrapper : IFile
    {
        private readonly FileStream stream;

        public UncompressedFileStreamWrapper(FileStream stream)
        {
            this.stream = stream;
        }

        public void Dispose() => stream?.Dispose();

        public void Write(ReadOnlySpan<byte> buffer) => stream.Write(buffer);
    }
}