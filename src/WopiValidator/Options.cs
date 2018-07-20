// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Office.WopiValidator.Core;

namespace Microsoft.Office.WopiValidator
{
	/// <summary>
	/// Represents set of command line arguments that can be used to modify behavior of the application.
	/// </summary>
	internal class Options
	{
		[Option('w', "wopisrc", Required = true, HelpText = "WopiSrc URL for a wopitest file")]
		public string WopiEndpoint { get; set; }

		[Option('t', "token", Required = true, HelpText = "WOPI access token")]
		public string AccessToken { get; set; }

		[Option('l', "token_ttl", Required = true, HelpText = "WOPI access token ttl")]
		public long AccessTokenTtl { get; set; }

		[Option('c', "config", Required = false, Default = "TestCases.xml", HelpText = "Path to XML file with test definitions")]
		public string RunConfigurationFilePath { get; set; }

		[Option('g', "testgroup", Required = false, HelpText = "Run only the tests in the specified group (cannot be used with testname)")]
		public string TestGroup { get; set; }

		[Option('n', "testname", Required = false, HelpText = "Run only the test specified (cannot be used with testgroup)")]
		public string TestName { get; set; }

		[Option('e', "testcategory", Required = false, Default = TestCategory.All, HelpText = "Run only the tests in the specified category")]
		public TestCategory TestCategory { get; set; }

		[Option('s', "ignore-skipped", Required = false, HelpText = "Don't output any info about skipped tests.")]
		public bool IgnoreSkipped { get; set; }

		[Option('v', "verbose", Required = false, HelpText = "Enable verbose logging to the console. Equivalent to --level debug.")]
		public bool VerboseLogging { get; set; }

		[Option("level", Required = false, Default = LogLevel.None, HelpText = "The minimum log level to log to the console.")]
		public LogLevel MinLogLevel { get; set; }
	}
}
