namespace OJS.Workers.SubmissionProcessors
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Workers.SubmissionProcessors.Formatters;

    public class RemoteWorkersProvider
        : IRemoteWorkersProvider
    {
        private readonly HashSet<RemoteWorker> freeRemoteWorkers;
        private readonly HashSet<RemoteWorker> busyRemoteWorkers;
        private IFormatterServiceFactory formatterServicesFactory;

        public RemoteWorkersProvider(IEnumerable<string> remoteWorkerEndpoints)
        {
            this.formatterServicesFactory = new FormatterServiceFactory();
            this.freeRemoteWorkers = new HashSet<RemoteWorker>(
                remoteWorkerEndpoints.Select(endpoint => new RemoteWorker(endpoint, this.formatterServicesFactory)));

            this.busyRemoteWorkers = new HashSet<RemoteWorker>();
        }

        public RemoteWorker GetFreeWorker()
        {
            lock (this.freeRemoteWorkers)
            lock (this.busyRemoteWorkers)
            {
                var worker = this.freeRemoteWorkers.FirstOrDefault();
                this.freeRemoteWorkers.Remove(worker);
                this.busyRemoteWorkers.Add(worker);
                return worker;
            }
        }

        public void FreeWorker(RemoteWorker worker)
        {
            lock (this.freeRemoteWorkers)
            lock (this.busyRemoteWorkers)
            {
                this.freeRemoteWorkers.Add(worker);
                this.busyRemoteWorkers.Remove(worker);
            }
        }
    }
}