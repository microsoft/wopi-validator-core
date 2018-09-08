// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using CommandLine;
using Microsoft.Office.WopiValidator.Core;

namespace Microsoft.Office.WopiValidator
{
	internal class Program
	{
		private static TestCaseExecutor GetTestCaseExecutor(TestExecutionData testExecutionData, Options options, TestCategory inputTestCategory)
		{
			TestCategory testCategory;
			if (!Enum.TryParse(testExecutionData.TestCase.Category, true /* ignoreCase */, out testCategory))
			{
				throw new Exception(string.Format(CultureInfo.InvariantCulture, "Invalid TestCategory for TestCase : {0}", testExecutionData.TestCase.Name));
			}

			string userAgent = (inputTestCategory == TestCategory.OfficeNativeClient || testCategory == TestCategory.OfficeNativeClient) ? Constants.HeaderValues.OfficeNativeClientUserAgent : null;

			return new TestCaseExecutor(testExecutionData, options.WopiEndpoint, options.AccessToken, options.AccessTokenTtl, userAgent);
		}

		private static void Main(string[] args)
		{
			// Wrapping all logic in a top-level Exception handler to ensure that exceptions are
			// logged to the console and don't cause Windows Error Reporting to kick in.
			try
			{
				var result = Parser.Default.ParseArguments<Options>(args)
					.WithParsed(options => Execute(options))
					.WithNotParsed(errors =>
					{
						Environment.ExitCode = 1;
						return;
					});

			}
			catch (Exception ex)
			{
				WriteToConsole(ex.ToString(), ConsoleColor.Red);
				Environment.ExitCode = 1;
			}

			if (Debugger.IsAttached)
			{
				WriteToConsole("Press any key to exit", ConsoleColor.White);
				Console.ReadLine();
			}
			return;
		}

		private static void Execute(Options options)
		{
			// get run configuration from XML
			IEnumerable<TestExecutionData> testData = ConfigParser.ParseExecutionData(options.RunConfigurationFilePath, options.TestCategory);

			if (!String.IsNullOrEmpty(options.TestGroup))
			{
				testData = testData.Where(d => d.TestGroupName == options.TestGroup);
			}

			IEnumerable<TestExecutionData> executionData;
			if (!String.IsNullOrWhiteSpace(options.TestName))
			{
				executionData = new TestExecutionData[] { TestExecutionData.GetDataForSpecificTest(testData, options.TestName) };
			}
			else
			{
				executionData = testData;
			}

			// Create executor groups
			var executorGroups = executionData.GroupBy(d => d.TestGroupName)
				.Select(g => new {
					Name = g.Key,
					Executors = g.Select(x => GetTestCaseExecutor(x, options, options.TestCategory))
				});

			ConsoleColor baseColor = ConsoleColor.White;
			bool atLeastOneTestFailedOrSkipped = false;
			foreach (var group in executorGroups)
			{
				WriteToConsole($"\nTest group: {group.Name}\n", ConsoleColor.White);

				// define execution query - evaluation is lazy; test cases are executed one at a time
				// as you iterate over returned collection
				var results = group.Executors.Select(x => x.Execute());

				bool hadPassFailResult = false;
				// iterate over results and print success/failure indicators into console
				foreach (TestCaseResult testCaseResult in results)
				{
					switch (testCaseResult.Status)
					{
						case ResultStatus.Pass:
							baseColor = ConsoleColor.Green;
							WriteToConsole($"Pass: {testCaseResult.Name}\n", baseColor, 1);
							hadPassFailResult = true;
							break;

						case ResultStatus.Skipped:
							baseColor = ConsoleColor.Yellow;
							if (!options.IgnoreSkipped)
							{
								atLeastOneTestFailedOrSkipped = true;
								WriteToConsole($"Skipped: {testCaseResult.Name}\n", baseColor, 1);
							}
							break;

						case ResultStatus.Fail:
						default:
							baseColor = ConsoleColor.Red;
							WriteToConsole($"Fail: {testCaseResult.Name}\n", baseColor, 1);
							hadPassFailResult = true;
							atLeastOneTestFailedOrSkipped = true;
							break;
					}

					if (testCaseResult.Status == ResultStatus.Fail ||
						(testCaseResult.Status == ResultStatus.Skipped && !options.IgnoreSkipped))
					{
						foreach (var request in testCaseResult.RequestDetails)
						{
							var responseStatus = (HttpStatusCode)request.ResponseStatusCode;
							var color = request.ValidationFailures.Count == 0 ? ConsoleColor.DarkGreen : baseColor;
							WriteToConsole($"{request.Name}, response code: {request.ResponseStatusCode} {responseStatus}\n", color, 2);
							foreach (var failure in request.ValidationFailures)
							{
								foreach (var error in failure.Errors)
									WriteToConsole($"{error}\n", baseColor, 3);
							}
						}

						WriteToConsole($"Re-run command: .\\wopivalidator.exe -n {testCaseResult.Name} -w {options.WopiEndpoint} -t {options.AccessToken} -l {options.AccessTokenTtl}\n", baseColor, 2);
						Console.WriteLine();
					}
				}

				if (!hadPassFailResult && options.IgnoreSkipped)
				{
					WriteToConsole($"All tests skipped.\n", baseColor, 1);
				}

				if (atLeastOneTestFailedOrSkipped)
				{
					Environment.ExitCode = 1;
				}
			}
		}

		private static void WriteToConsole(string message, ConsoleColor color, int indentLevel = 0)
		{
			ConsoleColor currentColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			string indent = new string(' ', indentLevel * 2);
			Console.Write(indent + message);
			Console.ForegroundColor = currentColor;
		}
	}
}
