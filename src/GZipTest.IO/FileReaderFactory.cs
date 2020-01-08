namespace GZipTest.IO
{
    public class FileReaderFactory : IFileReaderFactory
    {
        public IFileReader Create(bool compressed)
        {
            if (compressed)
            {
                return new CompressedBufferedFileReader();
            }

            return new UncompressedBufferedFileReader();
        }
    }
}