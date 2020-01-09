using System;
using System.Collections.Concurrent;
using System.Threading;
using GZipTest.Compression;
using GZipTest.Workflow.Context;
using Moq;
using Shouldly;
using Xunit;

namespace GZipTest.Workflow.Tests
{
    public sealed class ChunkProcessorTests : IDisposable
    {
        private readonly ChunkProcessor chunkProcessor;
        private readonly BlockingCollection<JobBatchItem> inputQueue;
        private readonly Mock<IOutputBuffer> outputBufferMock;
        private readonly Mock<IByteProcessor> processor;
        private readonly Mock<IJobContext> jobContextMock;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CountdownEvent countdownEvent;

        public ChunkProcessorTests()
        {
            cancellationTokenSource = new CancellationTokenSource();
            countdownEvent = new CountdownEvent(1);
            inputQueue = new BlockingCollection<JobBatchItem>();
            outputBufferMock = new Mock<IOutputBuffer>();
            processor = new Mock<IByteProcessor>();
            jobContextMock = new Mock<IJobContext>();
            chunkProcessor = new ChunkProcessor(inputQueue, outputBufferMock.Object, processor.Object,
                jobContextMock.Object, cancellationTokenSource, countdownEvent);
        }


        [Fact]
        public void StopsProcessingWhenExceptionThrown()
        {
            chunkProcessor.Start(cancellationTokenSource.Token);
            var inputThread = new Thread(() =>
            {
                countdownEvent.AddCount();
                var good = new byte[] { };
                var faulty = new byte[] { };
                processor.Setup(p => p.Process(good)).Returns(new byte[] { });
                inputQueue.Add(new JobBatchItem {Buffer = good});
                processor.Setup(p => p.Process(faulty)).Throws<Exception>();
                inputQueue.Add(new JobBatchItem {Buffer = faulty});
                inputQueue.CompleteAdding();
                countdownEvent.Signal();
            });
            inputThread.Start();
            countdownEvent.Signal();
            countdownEvent.Wait();
            jobContextMock.Verify(
                context => context.Failure(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            outputBufferMock.Verify(buffer => buffer.SubmitCompleted(), Times.Once);
            cancellationTokenSource.IsCancellationRequested.ShouldBeTrue();
        }

        [Fact]
        public void AbortsWhenCancellationRequested()
        {
            var good = new byte[] { };
            var result = new byte[] { };
            processor.Setup(p => p.Process(good)).Returns(result);
            inputQueue.Add(new JobBatchItem {Buffer = good});
            cancellationTokenSource.Cancel();

            chunkProcessor.Start(cancellationTokenSource.Token);

            countdownEvent.Signal();
            countdownEvent.Wait();
            jobContextMock.Verify(
                context => context.Failure(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            outputBufferMock.Verify(buffer => buffer.SubmitCompleted(), Times.Once);
            outputBufferMock.Verify(buffer => buffer.SubmitProcessedBatchItem(It.IsAny<ProcessedBatchItem>()), Times.Never);
        }

        public void Dispose()
        {
            inputQueue?.Dispose();
            cancellationTokenSource?.Dispose();
            countdownEvent?.Dispose();
        }
    }
}