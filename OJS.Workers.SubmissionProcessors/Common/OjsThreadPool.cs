namespace OJS.Workers.SubmissionProcessors.Common
{
    using System;

    using Qoollo.Turbo.Threading.ThreadPools;

    public class OjsThreadPool
        : IDisposable
    {
        private readonly DynamicThreadPool pool;

        public OjsThreadPool(int threadsCount)
        {
            this.pool = new DynamicThreadPool(0, threadsCount, 1024, "Remote Workers");
        }

        public void RunTask(Action task)
        {
            this.pool.Run(task);
        }

        public void Dispose()
        {
            this.pool.Dispose(
                true,
                letFinishProcess: true,
                completeAdding: true);
        }
    }
}