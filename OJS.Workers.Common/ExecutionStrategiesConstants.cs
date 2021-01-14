namespace OJS.Workers.Common
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Workers.Common.Models;

    public static class ExecutionStrategiesConstants
    {
        public static class ExecutionStrategyNames
        {
            // .NET
            public const string CsharpCode = "csharp-code";

            // .NET Core
            public const string CsharpDotNetCoreCode = "csharp-dot-net-core-code";
            public const string CSharpDotNetCoreProjectTests = "dot-net-core-project-tests";

            // Java
            public const string JavaCode = "java-code";
            public const string JavaProjectTests = "java-project-tests";
            public const string JavaUnitTests = "java-unit-tests";
            public const string JavaZipFileCode = "java-zip-file-code";

            // JavaScript
            public const string JavaScriptCode = "javascript-code";
            public const string JavaScriptJsDomUnitTests = "javascript-js-dom-unit-tests";
            public const string JavaScriptUnitTestsWithMocha = "javascript-unit-tests-with-mocha";
            public const string JavaScriptAsyncJsDomTestsWithReact = "javascript-async-js-dom-tests-with-react";

            public const string JavaScriptCodeAgainstUnitTestsWithMocha =
                "javascript-code-against-unit-tests-with-mocha";

            // Python
            public const string PythonCode = "python-code";
            public const string PythonProjectTests = "python-project-tests";
            public const string PythonProjectUnitTests = "python-project-unit-tests";

            // Php
            public const string PhpCode = "php-code";

            // HTML and CSS
            public const string HtmlAndCssZipFile = "html-and-css-zip-file";

            // C++
            public const string CppCode = "cpp-code";

            // Plain text
            public const string PlainText = "plaintext";
        }

        public static class NameMappings
        {
            public static readonly IDictionary<string, ExecutionStrategyType> NameToExecutionStrategyMappings =
                new Dictionary<string, ExecutionStrategyType>
                {
                    // .Net Core
                    { ExecutionStrategyNames.CsharpDotNetCoreCode, ExecutionStrategyType.DotNetCoreCompileExecuteAndCheck },

                    // Python
                    { ExecutionStrategyNames.PythonCode, ExecutionStrategyType.PythonExecuteAndCheck },
                    { ExecutionStrategyNames.PythonProjectTests, ExecutionStrategyType.PythonProjectTests },
                    { ExecutionStrategyNames.PythonProjectUnitTests, ExecutionStrategyType.PythonProjectUnitTests },

                    // HTML
                    { ExecutionStrategyNames.HtmlAndCssZipFile, ExecutionStrategyType.NodeJsZipExecuteHtmlAndCssStrategy },

                    // C++
                    { ExecutionStrategyNames.CppCode, ExecutionStrategyType.CPlusPlusCompileExecuteAndCheckExecutionStrategy },

                    // JavaScript
                    { ExecutionStrategyNames.JavaScriptCode, ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck },
                    { ExecutionStrategyNames.JavaScriptUnitTestsWithMocha, ExecutionStrategyType.NodeJsPreprocessExecuteAndRunUnitTestsWithMocha },
                    { ExecutionStrategyNames.JavaScriptJsDomUnitTests, ExecutionStrategyType.NodeJsPreprocessExecuteAndRunJsDomUnitTests },
                    { ExecutionStrategyNames.JavaScriptAsyncJsDomTestsWithReact, ExecutionStrategyType.NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy },
                    { ExecutionStrategyNames.JavaScriptCodeAgainstUnitTestsWithMocha, ExecutionStrategyType.NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy },

                    // Java
                    { ExecutionStrategyNames.JavaCode, ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck },
                    { ExecutionStrategyNames.JavaProjectTests, ExecutionStrategyType.JavaProjectTestsExecutionStrategy },
                    { ExecutionStrategyNames.JavaZipFileCode, ExecutionStrategyType.JavaZipFileCompileExecuteAndCheck },
                    { ExecutionStrategyNames.JavaUnitTests, ExecutionStrategyType.JavaUnitTestsExecutionStrategy },

                    // Plain text
                    { ExecutionStrategyNames.PlainText, ExecutionStrategyType.CheckOnly },

                    // .Net Core
                    // { ExecutionStrategyNames.CSharpDotNetCoreProjectTests, ExecutionStrategyType.DotNetCoreProjectTestsExecutionStrategy },
                    // .Net
                    // { ExecutionStrategyNames.CsharpCode, ExecutionStrategyType.CompileExecuteAndCheck },
                    // Php
                    // { ExecutionStrategyNames.PhpCode, ExecutionStrategyType.PhpCliExecuteAndCheck },
                };

            public static readonly IDictionary<ExecutionStrategyType, string> ExecutionStrategyToNameMappings =
                NameToExecutionStrategyMappings.ToDictionary(x => x.Value, y => y.Key);

            public static readonly ISet<ExecutionStrategyType> RemoteWorkerSupportedStrategies =
                new HashSet<ExecutionStrategyType>(NameToExecutionStrategyMappings.Values);
        }
    }
}