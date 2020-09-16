namespace OJS.Workers.SubmissionProcessors.SubmissionProcessors
{
    using System;
    using System.Collections.Concurrent;

    using OJS.Workers.Common;
    using OJS.Workers.SubmissionProcessors.Models;

    public class LocalSubmissionProcessor<TSubmission>
        : SubmissionProcessor<TSubmission>
    {
        private readonly int portNumber;
        private readonly object sharedLockObject;

        public LocalSubmissionProcessor(
            string name,
            IDependencyContainer dependencyContainer,
            ConcurrentQueue<TSubmission> submissionsForProcessing,
            int portNumber,
            object sharedLockObject)
            : base(name, dependencyContainer, submissionsForProcessing, sharedLockObject)
        {
            this.portNumber = portNumber;
            this.sharedLockObject = sharedLockObject;
        }

        protected override void ProcessSubmission<TInput, TResult>(OjsSubmission<TInput> submission)
        {
            this.logger.Info($"Work on submission #{submission.Id} started.");

            this.BeforeExecute(submission);

            var executor = new SubmissionExecutor(this.portNumber);

            var executionResult = executor.Execute<TInput, TResult>(submission);

            this.logger.Info($"Work on submission #{submission.Id} ended.");

            this.ProcessExecutionResult(executionResult, submission);

            this.logger.Info($"Submission #{submission.Id} successfully processed.");
        }

        private void BeforeExecute(IOjsSubmission submission)
        {
            try
            {
                this.submissionProcessingStrategy.BeforeExecute((TSubmission)submission.Id);
            }
            catch (Exception ex)
            {
                submission.ProcessingComment = $"Exception before executing the submission: {ex.Message}";

                throw new Exception($"Exception in {nameof(this.submissionProcessingStrategy.BeforeExecute)}", ex);
            }
        }
    }
}
