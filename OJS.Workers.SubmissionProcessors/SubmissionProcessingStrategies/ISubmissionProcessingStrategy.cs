namespace OJS.Workers.SubmissionProcessors.SubmissionProcessingStrategies
{
    using System.Collections.Concurrent;

    using log4net;

    using OJS.Workers.Common;

    public interface ISubmissionProcessingStrategy<TSubmission>
    {
        int JobLoopWaitTimeInMilliseconds { get; }

        void Initialize(
            ILog logger,
            ConcurrentQueue<TSubmission> submissionsForProcessing,
            object sharedLockObject);

        IOjsSubmission RetrieveSubmission();

        void BeforeExecute(TSubmission submissionId);

        void ProcessExecutionResult<TResult>(IExecutionResult<TResult> executionResult, TSubmission submissionId)
            where TResult : ISingleCodeRunResult, new();

        void OnError(IOjsSubmission submission);
    }
}
