using System.IO;
using GZipTest.IO;

namespace GZipTest.Workflow
{
    public class FileReaderThreadWork
    {
        private readonly IFileReader fileReader;
        private readonly JobQueue queue;
        private readonly FileInfo fileInfo;

        public FileReaderThreadWork(IFileReader fileReader, JobQueue queue, FileInfo fileInfo)
        {
            this.fileReader = fileReader;
            this.queue = queue;
            this.fileInfo = fileInfo;
        }

        public void Run()
        {
            foreach (var chunk in fileReader.Read(fileInfo))
            {
                if (!queue.IsFull)
                {
                    queue.Enqueue(chunk);
                }
            }
        }
    }
}