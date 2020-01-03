using GZipTest.Workflow.JobConfiguration;

namespace GZipTest.Workflow
{
    public interface IJobBatchOrchestrator
    {
        void StartProcess(JobDescription jobDescription);
    }
}