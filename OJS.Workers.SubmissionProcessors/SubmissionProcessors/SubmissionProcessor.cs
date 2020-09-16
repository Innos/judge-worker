namespace OJS.Workers.SubmissionProcessors.SubmissionProcessors
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using log4net;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.SubmissionProcessors.Models;
    using OJS.Workers.SubmissionProcessors.SubmissionProcessingStrategies;

    public abstract class SubmissionProcessor<TSubmission> : ISubmissionProcessor
    {
        protected readonly IDependencyContainer dependencyContainer;
        protected readonly ConcurrentQueue<TSubmission> submissionsForProcessing;
        protected readonly ILog logger;
        protected ISubmissionProcessingStrategy<TSubmission> submissionProcessingStrategy;

        private readonly object sharedLockObject;
        private bool stopping;

        public SubmissionProcessor(
            string name,
            IDependencyContainer dependencyContainer,
            ConcurrentQueue<TSubmission> submissionsForProcessing,
            object sharedLockObject)
        {
            this.Name = name;

            this.logger = LogManager.GetLogger(typeof(SubmissionProcessor<TSubmission>));
            this.logger.Info($"{nameof(SubmissionProcessor<TSubmission>)} initializing...");

            this.stopping = false;

            this.dependencyContainer = dependencyContainer;
            this.submissionsForProcessing = submissionsForProcessing;
            this.sharedLockObject = sharedLockObject;

            this.logger.Info($"{nameof(SubmissionProcessor<TSubmission>)} initialized.");
        }

        public string Name { get; set; }

        public void Start()
        {
            this.logger.Info($"{nameof(SubmissionProcessor<TSubmission>)} starting...");

            while (!this.stopping)
            {
                using (this.dependencyContainer.BeginDefaultScope())
                {
                    this.submissionProcessingStrategy = this.GetSubmissionProcessingStrategyInstance();
                    var submission = this.GetSubmissionForProcessing();

                    if (submission != null)
                    {
                        this.ProcessSubmission(submission);
                    }
                    else
                    {
                        Thread.Sleep(this.submissionProcessingStrategy.JobLoopWaitTimeInMilliseconds);
                    }
                }
            }

            this.logger.Info($"{nameof(SubmissionProcessor<TSubmission>)} stopped.");
        }

        public void Stop()
        {
            this.stopping = true;
        }

        protected virtual IOjsSubmission GetSubmissionForProcessing()
        {
            try
            {
                return this.submissionProcessingStrategy.RetrieveSubmission();
            }
            catch (Exception ex)
            {
                this.logger.Fatal("Unable to get submission for processing.", ex);
                throw;
            }
        }

        protected abstract void ProcessSubmission<TInput, TResult>(OjsSubmission<TInput> submission)
            where TResult : ISingleCodeRunResult, new();

        protected void ProcessExecutionResult<TOutput>(
            IExecutionResult<TOutput> executionResult,
            IOjsSubmission submission)
            where TOutput : ISingleCodeRunResult, new()
        {
            try
            {
                using (this.dependencyContainer.BeginDefaultScope())
                {
                    this.submissionProcessingStrategy = this.GetSubmissionProcessingStrategyInstance();
                    this.submissionProcessingStrategy.ProcessExecutionResult(executionResult, (TSubmission)submission.Id);
                }
            }
            catch (Exception ex)
            {
                submission.ProcessingComment = $"Exception in processing execution result: {ex.Message}";

                throw new Exception($"Exception in {nameof(this.ProcessExecutionResult)}", ex);
            }
        }

        protected ISubmissionProcessingStrategy<TSubmission> GetSubmissionProcessingStrategyInstance()
        {
            try
            {
                var processingStrategy = this.dependencyContainer
                    .GetInstance<ISubmissionProcessingStrategy<TSubmission>>();

                processingStrategy.Initialize(
                    this.logger,
                    this.submissionsForProcessing,
                    this.sharedLockObject);

                return processingStrategy;
            }
            catch (Exception ex)
            {
                this.logger.Fatal("Unable to initialize submission processing strategy.", ex);
                throw;
            }
        }

        // Overload accepting IOjsSubmission and doing cast, because upon getting the submission,
    // TInput is not known and no specific type could be given to the generic ProcessSubmission<>
    private void ProcessSubmission(IOjsSubmission submission)
        {
            try
            {
                switch (submission.ExecutionType)
                {
                    case ExecutionType.TestsExecution:
                        var testsSubmission = (OjsSubmission<TestsInputModel>)submission;
                        this.ProcessSubmission<TestsInputModel, TestResult>(testsSubmission);
                        break;

                    case ExecutionType.SimpleExecution:
                        var simpleSubmission = (OjsSubmission<string>)submission;
                        this.ProcessSubmission<string, OutputResult>(simpleSubmission);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(submission.ExecutionType),
                            "Invalid execution type!");
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(
                    $"{nameof(this.ProcessSubmission)} on submission #{submission.Id} has thrown an exception:",
                    ex);

                this.submissionProcessingStrategy.OnError(submission);
            }
        }
    }
}
