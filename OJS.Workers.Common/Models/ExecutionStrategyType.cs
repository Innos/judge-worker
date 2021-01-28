﻿namespace OJS.Workers.Common.Models
{
    public enum ExecutionStrategyType
    {
        NotFound = 0,
        CompileExecuteAndCheck = 1,
        NodeJsPreprocessExecuteAndCheck = 2,
        RemoteExecution = 3,
        JavaPreprocessCompileExecuteAndCheck = 4,
        PhpCgiExecuteAndCheck = 5,
        PhpCliExecuteAndCheck = 6,
        CheckOnly = 7,
        JavaZipFileCompileExecuteAndCheck = 8,
        PythonExecuteAndCheck = 9,
        DotNetCoreTestRunner = 10,
        NodeJsPreprocessExecuteAndRunUnitTestsWithMocha = 11,
        NodeJsPreprocessExecuteAndRunJsDomUnitTests = 12,
        SqlServerLocalDbPrepareDatabaseAndRunQueries = 13,
        SqlServerLocalDbRunQueriesAndCheckDatabase = 14,
        SqlServerLocalDbRunSkeletonRunQueriesAndCheckDatabase = 15,
        MySqlPrepareDatabaseAndRunQueries = 16,
        MySqlRunQueriesAndCheckDatabase = 17,
        MySqlRunSkeletonRunQueriesAndCheckDatabase = 18,
        NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy = 19,
        NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMocha = 20,
        NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy = 21,
        NodeJsZipExecuteHtmlAndCssStrategy = 22,
        CSharpUnitTestsExecutionStrategy = 23,
        CSharpProjectTestsExecutionStrategy = 24,
        JavaProjectTestsExecutionStrategy = 25,
        CPlusPlusZipFileExecutionStrategy = 26,
        JavaUnitTestsExecutionStrategy = 27,
        CSharpAspProjectTestsExecutionStrategy = 28,
        CPlusPlusCompileExecuteAndCheckExecutionStrategy = 29,
        JavaSpringAndHibernateProjectExecutionStrategy = 30,
        CSharpPerformanceProjectTestsExecutionStrategy = 31,
        RubyExecutionStrategy = 32,
        DotNetCoreProjectExecutionStrategy = 33,
        PhpProjectExecutionStrategy = 34,
        DotNetCoreProjectTestsExecutionStrategy = 35,
        PhpProjectWithDbExecutionStrategy = 36,
        DotNetCoreCompileExecuteAndCheck = 37,
        DotNetCoreUnitTestsExecutionStrategy = 38,
        SolidityCompileDeployAndRunUnitTestsExecutionStrategy = 39,
        DoNothing = 40,
        PythonUnitTests = 41,
        PythonCodeExecuteAgainstUnitTests = 42,
        PythonProjectTests = 43,
        PythonProjectUnitTests = 44,
<<<<<<< HEAD
        SqlServerSingleDatabasePrepareDatabaseAndRunQueries = 45,
        SqlServerSingleDatabaseRunQueriesAndCheckDatabase = 46,
        SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase = 47,
<<<<<<< HEAD
        MariaDbSingleDatabasePrepareDatabaseAndRunQueries = 48,
        MariaDbSingleDatabaseRunQueriesAndCheckDatabase = 49,
        MariaDbSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase = 50,

        MariaDbInMemoryPrepareDatabaseAndRunQueries = 51,
        MariaDbInMemoryRunQueriesAndCheckDatabase = 52,
        MariaDbInMemoryRunSkeletonRunQueriesAndCheckDatabase = 53,

        MySqlInMemoryPrepareDatabaseAndRunQueries = 54,
        MySqlInMemoryRunQueriesAndCheckDatabase = 55,
        MySqlInMemoryRunSkeletonRunQueriesAndCheckDatabase = 56,
=======
        SqlServerSingleDatabasePrepareDatabaseAndRunQueries = 48,
        SqlServerSingleDatabaseRunQueriesAndCheckDatabase = 49,
        SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase = 50, 
>>>>>>> 05958b5 (Fixed ExecutionStrategyType values)
=======
>>>>>>> 04a62ae (Cleanup)
    }
}