using System;

namespace GZipTest.IO
{
    public interface IFile : IDisposable
    {
        void Write(ReadOnlySpan<byte> buffer);
    }
}