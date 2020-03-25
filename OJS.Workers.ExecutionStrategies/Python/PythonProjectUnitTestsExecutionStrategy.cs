namespace OJS.Workers.ExecutionStrategies.Python
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OJS.Workers.Common;
    using OJS.Workers.ExecutionStrategies.Helpers;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    using static OJS.Workers.Common.Constants;
    using static OJS.Workers.ExecutionStrategies.Python.PythonConstants;

    public class PythonProjectUnitTestsExecutionStrategy : PythonUnitTestsExecutionStrategy
    {
        private const string ProjectFolderName = "project";
        private const string ProjectFilesCountPlaceholder = "# project_files_count ";
        private const string ClassNameRegexPattern = @"^class\s+([a-zA-z0-9]+)";
        private const string UpperCaseSplitRegexPattern = @"(?<!^)(?=[A-Z])";

        private const string ProjectFilesNotCapturedCorrectlyErrorMessageTemplate =
            "There should be {0} classes in test #{1}, but found {2}. Ensure the test is correct";

        private readonly string projectFilesCountRegexPattern = $@"^{ProjectFilesCountPlaceholder}\s*([0-9])\s*$";
        private readonly string projectFilesRegexPattern =
            $@"(?:^from\s+[\s\S]+?)?{ClassNameRegexPattern}[\s\S]+?(?=^from|^class)";

        private readonly string projectFilesCountNotSpecifiedInSolutionSkeletonErrorMessage =
            $"Expecting \"{ProjectFilesCountPlaceholder}\" in solution skeleton followed by the number of files that the project has";

        private int? expectedProjectFilesCount;

        public PythonProjectUnitTestsExecutionStrategy(
            IProcessExecutorFactory processExecutorFactory,
            string pythonExecutablePath,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(processExecutorFactory, pythonExecutablePath, baseTimeUsed, baseMemoryUsed)
        {
        }

        protected override IEnumerable<string> ExecutionArguments
            => new[]
            {
                IgnorePythonEnvVarsFlag,
                DontAddUserSiteDirectoryFlag,
                ModuleFlag,
                UnitTestModuleName,
                DiscoverTestsCommandName,
            };

        private string ProjectDirectoryPath => Path.Combine(this.WorkingDirectory, ProjectFolderName);

        protected override IExecutionResult<TestResult> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
        {
            this.SaveZipSubmission(executionContext.FileContent, this.WorkingDirectory);

            var executor = this.CreateExecutor();
            var checker = executionContext.Input.GetChecker();

            Directory.CreateDirectory(this.ProjectDirectoryPath);
            PythonStrategiesHelper.CreateInitFile(this.ProjectDirectoryPath);

            return this.RunTests(string.Empty, executor, checker, executionContext, result);
        }

        protected override TestResult RunIndividualUnitTest(
            ref int originalTestsPassed,
            string codeSavePath,
            IExecutor executor,
            IChecker checker,
            IExecutionContext<TestsInputModel> executionContext,
            TestContext test,
            bool isFirstRun)
        {
            this.expectedProjectFilesCount = this.expectedProjectFilesCount
                ?? (this.expectedProjectFilesCount = this.GetExpectedProjectFilesCount(
                    executionContext.Input.TaskSkeletonAsString));

            this.SaveTestProjectFiles(this.expectedProjectFilesCount.Value, test);

            var processExecutionResult = this.Execute(
                executionContext,
                executor,
                codeSavePath,
                string.Empty,
                this.WorkingDirectory);

            return this.GetUnitTestsResultFromExecutionResult(
                ref originalTestsPassed,
                checker,
                processExecutionResult,
                test,
                isFirstRun);
        }

        /// <summary>
        /// Gets a file name with extension for the provided class. The convention is SampleClass -> sample_class.py
        /// </summary>
        /// <param name="className">The python class name</param>
        /// <returns>File name for the python class</returns>
        private static string GetFileNameWithExtensionForClass(string className)
            => string.Join(
                "_",
                Regex
                    .Split(className, UpperCaseSplitRegexPattern)
                    .Select(x => x.ToLower()))
                + PythonFileExtension;

        /// <summary>
        /// Generates and saves all python files that are being tested by the user.
        /// Files are extracted and generated by the test input, which contains all file contents in a single string.
        /// </summary>
        /// <param name="expectedFilesCount">Predefined value that acts as a validity check</param>
        /// <param name="test">The test on which the operation is performed</param>
        /// <exception cref="ArgumentException">Thrown if the expected files count does not match the captured files from the test</exception>
        private void SaveTestProjectFiles(int expectedFilesCount, TestContext test)
        {
            var projectFilesToBeCreated = this.GetProjectFilesToBeCreated(test);

            if (projectFilesToBeCreated.Count != expectedFilesCount)
            {
                throw new ArgumentException(string.Format(
                    ProjectFilesNotCapturedCorrectlyErrorMessageTemplate,
                    expectedFilesCount,
                    test.Id,
                    projectFilesToBeCreated.Count));
            }

            foreach (var projectFile in projectFilesToBeCreated)
            {
                File.WriteAllText(
                    Path.Combine(this.ProjectDirectoryPath, projectFile.Key),
                    projectFile.Value);
            }
        }

        /// <summary>
        /// Gets the predefined count of the files that need to be generated and put in the project directory
        /// </summary>
        /// <param name="solutionSkeleton">The skeleton in which this count is written upon task creation</param>
        /// <returns>Number of files that need to be extracted from every test input and saved in the working directory</returns>
        /// <exception cref="ArgumentException">Exception thrown if the count is not given as expected</exception>
        private int GetExpectedProjectFilesCount(string solutionSkeleton)
        {
            solutionSkeleton = solutionSkeleton ?? string.Empty;

            var projectFilesCountRegex = new Regex(this.projectFilesCountRegexPattern);
            var projectFilesCountAsString = projectFilesCountRegex.Match(solutionSkeleton).Groups[1].Value;

            if (int.TryParse(projectFilesCountAsString, out var projectFilesCount))
            {
                return projectFilesCount;
            }

            throw new ArgumentException(this.projectFilesCountNotSpecifiedInSolutionSkeletonErrorMessage);
        }

        /// <summary>
        /// Gets files to be created in a project directory, by extracting all classes from the test input
        /// The test input contains multiple classes, that have to be extracted and put in separate files
        /// </summary>
        /// <param name="test">The test on with the operation is performed</param>
        /// <returns>A dictionary containing file name as a key and file content as a value</returns>
        private Dictionary<string, string> GetProjectFilesToBeCreated(TestContext test)
        {
            var testInput = test.Input;

            var filesRegex = new Regex(this.projectFilesRegexPattern, RegexOptions.Multiline);
            var classNameRegex = new Regex(ClassNameRegexPattern, RegexOptions.Multiline);

            var projectFilesToBeCreated = filesRegex.Matches(testInput)
                .Cast<Match>()
                .ToDictionary(
                    m => GetFileNameWithExtensionForClass(m.Groups[1].Value),
                    m => m.Value.Trim());

            // removing all matches and leaving the last/only one, which the regex does not capture
            var lastFileContent = filesRegex.Replace(testInput, string.Empty).Trim();
            var lastClassName = classNameRegex.Match(lastFileContent).Groups[1].Value;
            var lastFileName = GetFileNameWithExtensionForClass(lastClassName);

            projectFilesToBeCreated.Add(lastFileName, lastFileContent);

            return projectFilesToBeCreated;
        }
    }
}