// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Office.WopiValidator.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Office.WopiValidator
{
	/// <summary>
	/// Options for the list command.
	/// </summary>
	[Verb("list", HelpText = "List tests that match the filter criteria")]
	internal class ListOptions : OptionsBase
	{
		[Option('t', "tags", Required = false, HelpText = "Filter to tests with these tags")]
		public IEnumerable<string> Tags { get; set; }

		internal static ExitCode ListCommand(ListOptions options)
		{
			// get run configuration from XML
			IEnumerable<TestExecutionData> testData = ConfigParser.ParseExecutionData(options.RunConfigurationFilePath);

			// Filter the tests
			IEnumerable<TestExecutionData> executionData = testData.ApplyFilters(options);

			// Create executor groups
			var executorGroups = executionData.GroupBy(d => d.TestGroupName)
				.Select(g => new
				{
					Name = g.Key,
					TestData = g.Select(x => x)
				});


			foreach (var group in executorGroups)
			{
				Helpers.WriteToConsole($"\nTest group: {group.Name}\n", ConsoleColor.White);

				foreach(var test in group.TestData)
				{
					Helpers.WriteToConsole($"{test.TestCase.Name}\n", ConsoleColor.Blue);
				}
			}
			return ExitCode.Failure;
		}

		//private static TestCaseExecutor GetTestCaseExecutor(TestExecutionData testExecutionData, ListOptions options, TestCategory inputTestCategory)
		//{
		//	bool officeNative = inputTestCategory == TestCategory.OfficeNativeClient ||
		//		testExecutionData.TestCase.TestCategory == TestCategory.OfficeNativeClient;
		//	string userAgent = officeNative ? Constants.HeaderValues.OfficeNativeClientUserAgent : null;

		//	return new TestCaseExecutor(testExecutionData, options.WopiEndpoint, options.AccessToken, options.AccessTokenTtl, userAgent);
		//}
	}
}
