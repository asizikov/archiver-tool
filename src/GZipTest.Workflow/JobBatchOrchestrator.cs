using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using GZipTest.IO;

namespace GZipTest.Workflow
{
    public class JobBatchOrchestrator : IJobBatchOrchestrator
    {
        private readonly IFileReader fileReader;
        private readonly IOutputBuffer outputBuffer;
        private readonly JobQueue queue;
        private readonly Thread[] chunkProcessorThreadPool;
        private Thread fileReaderThread;

        public JobBatchOrchestrator(IFileReader fileReader, IOutputBuffer outputBuffer)
        {
            this.fileReader = fileReader;
            this.outputBuffer = outputBuffer;
            queue = new JobQueue(100);
            chunkProcessorThreadPool = new Thread[10];

            var fileReaderThreadWork = new FileReaderThreadWork(fileReader, queue, new FileInfo("testFile.bin"));
            fileReaderThread = new Thread(fileReaderThreadWork.Run);
            ConfigureChunkProcessorThreads();
        }

        private void ConfigureChunkProcessorThreads()
        {
            for (int i = 0; i < chunkProcessorThreadPool.Length; i++)
            {
                var processor = new ChunkProcessor(queue);
                var chunkProcessorThread = new Thread(processor.Run);
                chunkProcessorThreadPool[i] = chunkProcessorThread;
            }
        }

        private void JobProcessorLoop()
        {
            while (completed)
            {
                LoadInputBuffer();
                ProcessBatch();
                FlushOutputBuffer();
            }
        }

        private void FlushOutputBuffer()
        {
            outputBuffer.Flush();
        }

        private void ProcessBatch()
        {
            foreach (var thread in chunkProcessorThreadPool)
            {
                thread.Start();
            }
        }

        private void LoadInputBuffer()
        {
            fileReaderThread.Start();
        }
    }

    public interface IOutputBuffer
    {
        void Flush();
    }
}
