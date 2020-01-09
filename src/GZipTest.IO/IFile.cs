using System;

namespace GZipTest.IO
{
    public interface IFile: IDisposable
    {
        void Write(byte[] buffer, int size);
    }
}