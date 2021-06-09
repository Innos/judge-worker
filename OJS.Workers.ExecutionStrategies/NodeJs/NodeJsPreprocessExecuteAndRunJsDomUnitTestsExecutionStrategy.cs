﻿namespace OJS.Workers.ExecutionStrategies.NodeJs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    public class NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy
        : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy
    {
        public NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
            IProcessExecutorFactory processExecutorFactory,
            string nodeJsExecutablePath,
            string mochaModulePath,
            string chaiModulePath,
            string jsdomModulePath,
            string jqueryModulePath,
            string handlebarsModulePath,
            string sinonModulePath,
            string sinonChaiModulePath,
            string underscoreModulePath,
            int baseTimeUsed,
            int baseMemoryUsed) // TODO: make this modular by getting requires from test
            : base(
                processExecutorFactory,
                nodeJsExecutablePath,
                mochaModulePath,
                chaiModulePath,
                sinonModulePath,
                sinonChaiModulePath,
                underscoreModulePath,
                baseTimeUsed,
                baseMemoryUsed)
        {
            if (!Directory.Exists(jsdomModulePath))
            {
                throw new ArgumentException(
                    $"jsDom not found in: {jsdomModulePath}",
                    nameof(jsdomModulePath));
            }

            if (!Directory.Exists(jqueryModulePath))
            {
                throw new ArgumentException(
                    $"jQuery not found in: {jqueryModulePath}",
                    nameof(jqueryModulePath));
            }

            if (!Directory.Exists(handlebarsModulePath))
            {
                throw new ArgumentException(
                    $"Handlebars not found in: {handlebarsModulePath}",
                    nameof(handlebarsModulePath));
            }

            this.JsDomModulePath = FileHelpers.ProcessModulePath(jsdomModulePath);
            this.JQueryModulePath = FileHelpers.ProcessModulePath(jqueryModulePath);
            this.HandlebarsModulePath = FileHelpers.ProcessModulePath(handlebarsModulePath);
        }

        protected string JsDomModulePath { get; }

        protected string JQueryModulePath { get; }

        protected string HandlebarsModulePath { get; }

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    jsdom = require('" + this.JsDomModulePath + @"'),
    jq = require('" + this.JQueryModulePath + @"'),
    sinon = require('" + this.SinonModulePath + @"'),
    sinonChai = require('" + this.SinonChaiModulePath + @"'),
    handlebars = require('" + this.HandlebarsModulePath + @"')";

        protected override string JsCodePreevaulationCode => @"
chai.use(sinonChai);
const { JSDOM } = jsdom;

describe('TestDOMScope', function() {
    let bgCoderConsole = {};
    before(function() {
        const window  = (new JSDOM('...')).window;

        // define innerText manually to work as textContent, as it is not supported in jsdom but used in judge
        Object.defineProperty(window.Element.prototype, 'innerText', {
            get() { return this.textContent; },
            set(value) { this.textContent = value; }
        });

        // Add jsdom's window, document and other libs to the global object
        global.window = window;
        global.document = window.document;
        global.$ = jq(window);
        global.handlebars = handlebars;

        // Set specific HTML Element constructors that are globally available in the browser
        global.Option = window.Option;
        global.Audio = window.Audio;
        global.Image = window.Image;
        
        // Attach other HTML properties to the global scope so they are directly available
        Object.getOwnPropertyNames(window)
            .filter(function (prop) {
                return prop.toLowerCase().indexOf('html') >= 0;
            }).forEach(function (prop) {
                global[prop] = window[prop];
            });

        // Store and redefine console functions so the process output cannot be poluted 
        Object.keys(console)
            .forEach(function (prop) {
                bgCoderConsole[prop] = console[prop];
                console[prop] = new Function('');
            });
    });

    after(function() {
        Object.keys(bgCoderConsole)
            .forEach(function (prop) {
                console[prop] = bgCoderConsole[prop];
            });
    });";

        protected override string JsCodeEvaluation => TestsPlaceholder;

        protected override string TestFuncVariables => base.TestFuncVariables + ", '_'";

        protected virtual string BuildTests(IEnumerable<TestContext> tests)
        {
            var testsCode = string.Empty;
            var testsCount = 1;
            foreach (var test in tests)
            {
                var code = Regex.Replace(test.Input, "([\\\\`])", "\\$1");

                testsCode +=
                    $@"
it('Test{testsCount++}', function(done) {{
    let content = `{code}`;
    let inputData = content.trim();
    let code = {{
        run: {UserInputPlaceholder}
    }};
    let testFunc = new Function('result', {this.TestFuncVariables}, inputData);
    testFunc.call({{}}, code.run, {this.TestFuncVariables.Replace("'", string.Empty)});
    done();
}});";
            }

            return testsCode;
        }

        protected override List<TestResult> ProcessTests(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutor executor,
            IChecker checker,
            string codeSavePath)
        {
            var testResults = new List<TestResult>();
            var arguments = new List<string>();
            arguments.Add(this.MochaModulePath);
            arguments.Add(codeSavePath);
            arguments.AddRange(this.AdditionalExecutionArguments);

            var processExecutionResult = executor.Execute(
                this.NodeJsExecutablePath,
                string.Empty,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                arguments);

            var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);
            var currentTest = 0;
            foreach (var test in executionContext.Input.Tests)
            {
                var message = "yes";
                if (!string.IsNullOrEmpty(mochaResult.Error))
                {
                    message = mochaResult.Error;
                }
                else if (mochaResult.TestErrors[currentTest] != null)
                {
                    message = $"Unexpected error: {mochaResult.TestErrors[currentTest]}";
                }

                var testResult = this.CheckAndGetTestResult(
                    test,
                    processExecutionResult,
                    checker,
                    message);
                currentTest++;
                testResults.Add(testResult);
            }

            return testResults;
        }

        protected override string PreprocessJsSubmission<TInput>(string template, IExecutionContext<TInput> context)
        {
            var code = context.Code.Trim(';');
            var processedCode = template
                .Replace(RequiredModules, this.JsCodeRequiredModules)
                .Replace(PreevaluationPlaceholder, this.JsCodePreevaulationCode)
                .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
                .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
                .Replace(NodeDisablePlaceholder, this.JsNodeDisableCode)
                .Replace(TestsPlaceholder, this.BuildTests((context.Input as TestsInputModel)?.Tests))
                .Replace(UserInputPlaceholder, code);
            return processedCode;
        }
    }
}
