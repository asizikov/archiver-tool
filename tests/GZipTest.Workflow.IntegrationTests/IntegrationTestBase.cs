using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace GZipTest.Workflow.IntegrationTests
{
    public abstract class IntegrationTestBase : IDisposable
    {
        private string subdirectory = "GZipTest.IntegrationTests";
        protected FileInfo inputFile;
        protected FileInfo outputFile;
        protected FileInfo decompressedFile;

        protected IntegrationTestBase()
        {
            LoadInputFile();
        }

        private void LoadInputFile()
        {
            var embeddedFileProvider = new EmbeddedFileProvider(typeof(WorkflowIntegrationTests).Assembly);
            var embeddedFile = embeddedFileProvider.GetFileInfo("test_file.bin");
            var tempPath = Path.Join(Path.GetTempPath(), subdirectory);
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            
            using var readStream = embeddedFile.CreateReadStream();
            var tempFile = Path.Combine(tempPath, $"testFile{DateTime.Now.Ticks}.bin");
            using var fileStream = File.Create(tempFile);
            readStream.CopyTo(fileStream);
            inputFile = new FileInfo(tempFile);
            outputFile = new FileInfo(tempFile + ".compressed");
            decompressedFile = new FileInfo(outputFile.FullName + ".decompressed");
        }

        public void Dispose()
        {
            File.Delete(outputFile.FullName);
            File.Delete(inputFile.FullName);
            File.Delete(decompressedFile.FullName);
        }
    }
}