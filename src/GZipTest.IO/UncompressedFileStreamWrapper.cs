﻿using System.IO;

namespace GZipTest.IO
{
    public class UncompressedFileStreamWrapper : IFile
    {
        private readonly FileStream stream;

        public UncompressedFileStreamWrapper(FileStream stream)
        {
            this.stream = stream;
        }

        public void Dispose() => stream.Dispose();

        public void Write(byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}