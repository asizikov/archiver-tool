namespace GZipTest.IO
{
    public interface IFileReaderFactory
    {
        IFileReader Create(bool compressed);
    }
}