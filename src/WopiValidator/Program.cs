// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Office.WopiValidator.Core;
using Microsoft.Office.WopiValidator.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace Microsoft.Office.WopiValidator
{
	internal enum ExitCode
	{
		Success = 0,
		Failure = 1,
	}

	internal class Program
	{
		private static readonly ILogger logger = ApplicationLogging.CreateLogger<Program>();

		private static TestCaseExecutor GetTestCaseExecutor(TestExecutionData testExecutionData, Options options, TestCategory inputTestCategory, ILogger logger)
		{
			TestCategory testCategory;
			if (!Enum.TryParse(testExecutionData.TestCase.Category, true /* ignoreCase */, out testCategory))
			{
				throw new Exception(string.Format(CultureInfo.InvariantCulture, "Invalid TestCategory for TestCase : {0}", testExecutionData.TestCase.Name));
			}

			string userAgent = (inputTestCategory == TestCategory.OfficeNativeClient || testCategory == TestCategory.OfficeNativeClient) ? Constants.HeaderValues.OfficeNativeClientUserAgent : null;

			return new TestCaseExecutor(testExecutionData, options.WopiEndpoint, options.AccessToken, options.AccessTokenTtl, userAgent);
		}

		private static int Main(string[] args)
		{
			// Wrapping all logic in a top-level Exception handler to ensure that exceptions are
			// logged to the console and don't cause Windows Error Reporting to kick in.
			ExitCode exitCode = ExitCode.Success;
			try
			{
				exitCode = Parser.Default.ParseArguments<Options>(args)
					.MapResult(
						(Options options) => Execute(options),
						parseErrors => ExitCode.Failure);
			}
			catch (Exception ex)
			{
				ConsoleWriter.Write(ex.ToString(), ConsoleColor.Red);
				exitCode = ExitCode.Failure;
			}

			if (Debugger.IsAttached)
			{
				ConsoleWriter.Write("Press any key to exit", ConsoleColor.White);
				Console.ReadLine();
			}
			return (int)exitCode;
		}

		private static ExitCode Execute(Options options)
		{
			// Configure logging
			var minLevel = options.MinLogLevel;
			if (minLevel != LogLevel.None)
			{
				if (options.VerboseLogging)
				{
					ConsoleWriter.Write("Verbose is being ignored since --level was specified.", ConsoleColor.Yellow);
				}
			}
			else if (options.VerboseLogging)
			{
				minLevel = LogLevel.Debug;
			}

			if (minLevel != LogLevel.None)
			{
				ApplicationLogging.LoggerFactory.AddConsole(minLevel);
			}

			logger.LogInformation("Logger initialized.");

			// get run configuration from XML
			IEnumerable<TestExecutionData> testData = ConfigParser.ParseExecutionData(options.RunConfigurationFilePath, options.TestCategory, ApplicationLogging.LoggerFactory);

			if (!String.IsNullOrEmpty(options.TestGroup))
			{
				logger.Log($"Restricting to tests in group '{options.TestGroup}'.");
				testData = testData.Where(d => d.TestGroupName == options.TestGroup);
			}

			IEnumerable<TestExecutionData> executionData;
			if (!String.IsNullOrWhiteSpace(options.TestName))
			{
				logger.Log($"Restricting to tests with name '{options.TestName}'.");
				executionData = new TestExecutionData[] { TestExecutionData.GetDataForSpecificTest(testData, options.TestName) };
			}
			else
			{
				executionData = testData;
			}

			// Create executor groups
			var executorGroups = executionData.GroupBy(d => d.TestGroupName)
				.Select(g => new
				{
					Name = g.Key,
					Executors = g.Select(x => GetTestCaseExecutor(x, options, options.TestCategory, logger))
				});

			ConsoleColor baseColor = ConsoleColor.White;
			HashSet<ResultStatus> resultStatuses = new HashSet<ResultStatus>();
			foreach (var group in executorGroups)
			{
				if (!String.IsNullOrWhiteSpace(options.TestName))
				{
					ConsoleWriter.Write($"\nTest group: {group.Name}", ConsoleColor.White);
				}

				// define execution query - evaluation is lazy; test cases are executed one at a time
				// as you iterate over returned collection
				var results = group.Executors.Select(x => x.Execute(logger));

				using (logger.BeginScope(2))
				{
					// iterate over results and print success/failure indicators into console
					foreach (TestCaseResult testCaseResult in results)
					{
						resultStatuses.Add(testCaseResult.Status);
						switch (testCaseResult.Status)
						{
							case ResultStatus.Pass:
								baseColor = ConsoleColor.Green;
								ConsoleWriter.Write($"Pass: {testCaseResult.Name}", baseColor, 1);
								break;

							case ResultStatus.Skipped:
								baseColor = ConsoleColor.Yellow;
								if (!options.IgnoreSkipped)
								{
									ConsoleWriter.Write($"Skipped: {testCaseResult.Name}", baseColor, 1);
								}
								break;

							case ResultStatus.Fail:
							default:
								baseColor = ConsoleColor.Red;
								ConsoleWriter.Write($"Fail: {testCaseResult.Name}", baseColor, 1);
								break;
						}

						if (testCaseResult.Status == ResultStatus.Fail ||
							(testCaseResult.Status == ResultStatus.Skipped && !options.IgnoreSkipped))
						{
							foreach (var request in testCaseResult.RequestDetails)
							{
								var responseStatus = (HttpStatusCode)request.ResponseStatusCode;
								var color = request.ValidationFailures.Count == 0 ? ConsoleColor.DarkGreen : baseColor;
								ConsoleWriter.Write($"{request.Name}, response code: {request.ResponseStatusCode} {responseStatus}", color, 2);
								foreach (var failure in request.ValidationFailures)
								{
									foreach (var error in failure.Errors)
										ConsoleWriter.Write($"{error}", baseColor, 3);
								}
							}

							ConsoleWriter.Write($"Re-run command: .\\wopivalidator.exe -n {testCaseResult.Name} -w {options.WopiEndpoint} -t {options.AccessToken} -l {options.AccessTokenTtl}", baseColor, 2);
							Console.WriteLine();
						}
					}
				}

				if (options.IgnoreSkipped && !resultStatuses.ContainsAny(ResultStatus.Pass, ResultStatus.Fail))
				{
					ConsoleWriter.Write($"All tests skipped.", baseColor, 1);
				}
			}

			// If skipped tests are ignored, don't consider them when determining whether the test run passed or failed
			if (options.IgnoreSkipped)
			{
				if (resultStatuses.Contains(ResultStatus.Fail))
				{
					return ExitCode.Failure;
				}
			}
			// Otherwise consider skipped tests as failures
			else if (resultStatuses.ContainsAny(ResultStatus.Skipped, ResultStatus.Fail))
			{
				return ExitCode.Failure;
			}
			return ExitCode.Success;
		}
	}

	internal static class ExtensionMethods
	{
		internal static bool ContainsAny<T>(this HashSet<T> set, params T[] items)
		{
			return set.Intersect(items).Any();
		}

		internal static string StripNewLines(this string str)
		{
			StringBuilder sb = new StringBuilder(str);
			bool newLineAtStart = str.StartsWith(Environment.NewLine);
			bool newLineAtEnd = str.EndsWith(Environment.NewLine);
			sb.Replace(Environment.NewLine, " ");

			if (newLineAtStart)
			{
				sb.Insert(0, Environment.NewLine);
			}

			if (newLineAtEnd)
			{
				sb.Append(Environment.NewLine);
			}
			return sb.ToString();
		}
	}
}
