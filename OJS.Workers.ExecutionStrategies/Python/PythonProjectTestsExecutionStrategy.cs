namespace OJS.Workers.ExecutionStrategies.Python
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    using static OJS.Workers.Common.Constants;

    public class PythonProjectTestsExecutionStrategy : PythonCodeExecuteAgainstUnitTestsExecutionStrategy
    {
        private const string ZippedSubmissionName = "Submission.zip";
        private const string TestsFolderName = "tests";
        private const string InitFileName = "__init__";

        public PythonProjectTestsExecutionStrategy(
            IProcessExecutorFactory processExecutorFactory,
            string pythonExecutablePath,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(processExecutorFactory, pythonExecutablePath, baseTimeUsed, baseMemoryUsed)
        {
        }

        private string TestsDirectoryName => Path.Combine(this.WorkingDirectory, TestsFolderName);

        protected override IExecutionResult<TestResult> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
        {
            this.SaveSubmission(executionContext.FileContent);
            this.SaveTests(executionContext.Input.Tests.ToList());

            var executor = this.CreateExecutor();

            return result;
        }

        protected virtual void SaveSubmission(byte[] submissionContent)
        {
            var submissionFilePath = Path.Combine(this.WorkingDirectory, ZippedSubmissionName);
            File.WriteAllBytes(submissionFilePath, submissionContent);
            FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);
            File.Delete(submissionFilePath);
        }

        protected virtual void SaveTests(IList<TestContext> tests)
        {
            Directory.CreateDirectory(this.TestsDirectoryName);
            var initFilePath = Path.Combine(this.TestsDirectoryName, $"{InitFileName}{PythonFileExtension}");
            File.WriteAllText(initFilePath, string.Empty);

            for (var i = 0; i < tests.Count; i++)
            {
                var test = tests[i];
                var testName = $"Test_{i}{PythonFileExtension}";
                var testSavePath = Path.Combine(this.TestsDirectoryName, testName);

                File.WriteAllText(testSavePath, test.Input);
            }
        }
    }
}