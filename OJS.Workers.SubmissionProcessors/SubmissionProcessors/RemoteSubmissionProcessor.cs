namespace OJS.Workers.SubmissionProcessors.SubmissionProcessors
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.SubmissionProcessors.Common;
    using OJS.Workers.SubmissionProcessors.Models;

    public class RemoteSubmissionProcessor<TSubmission>
        : SubmissionProcessor<TSubmission>
    {
        private readonly ISet<ExecutionStrategyType> remoteWorkerExecutionStrategyTypes = new HashSet<ExecutionStrategyType>
        {
            ExecutionStrategyType.CompileExecuteAndCheck,
            ExecutionStrategyType.DotNetCoreCompileExecuteAndCheck,
            ExecutionStrategyType.PythonExecuteAndCheck,
            ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck,
            ExecutionStrategyType.CPlusPlusCompileExecuteAndCheckExecutionStrategy,
            ExecutionStrategyType.PhpCliExecuteAndCheck,
            ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck,
        };

        private readonly OjsThreadPool threadPool;
        private IRemoteWorkersProvider remoteWorkersProvider;

        public RemoteSubmissionProcessor(
            string name,
            IDependencyContainer dependencyContainer,
            ConcurrentQueue<TSubmission> submissionsForProcessing,
            IEnumerable<string> remoteWorkerEndpoints,
            object sharedLockObject)
            : base(name, dependencyContainer, submissionsForProcessing, sharedLockObject)
        {
            this.remoteWorkersProvider = new RemoteWorkersProvider(remoteWorkerEndpoints);
            this.threadPool = new OjsThreadPool(remoteWorkerEndpoints.Count());
        }

        protected override IOjsSubmission GetSubmissionForProcessing()
        {
            var submission = base.GetSubmissionForProcessing();

            if (!(submission is OjsSubmission<TestsInputModel>))
            {
                return null;
            }

            return this.remoteWorkerExecutionStrategyTypes.Contains(submission.ExecutionStrategyType)
                ? submission
                : null;
        }

        protected override void ProcessSubmission<TInput, TResult>(OjsSubmission<TInput> submission)
            => this.threadPool.RunTask(() =>
            {
                var worker = this.remoteWorkersProvider.GetFreeWorker();
                var result = worker.RunSubmission<TResult>(submission as OjsSubmission<TestsInputModel>);
                this.remoteWorkersProvider.FreeWorker(worker);
                this.ProcessExecutionResult(result, submission);
            });
    }
}
