namespace OJS.Workers.SubmissionProcessors
{
    public interface IRemoteWorkersProvider
    {
        RemoteWorker GetFreeWorker();

        void FreeWorker(RemoteWorker worker);
    }
}