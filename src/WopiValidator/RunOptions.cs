// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Office.WopiValidator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Globalization;
using System.Text;

namespace Microsoft.Office.WopiValidator
{
	/// <summary>
	/// Represents set of command line arguments that can be used to modify behavior of the application.
	/// </summary>
	[Verb("run", HelpText = "Run tests.")]
	internal class RunOptions : OptionsBase
	{
		[Option('w', "wopisrc", Required = true, HelpText = "WopiSrc URL for a wopitest file")]
		public string WopiEndpoint { get; set; }

		[Option('t', "token", Required = true, HelpText = "WOPI access token")]
		public string AccessToken { get; set; }

		[Option('l', "token_ttl", Required = true, HelpText = "WOPI access token ttl")]
		public long AccessTokenTtl { get; set; }

		[Option("UsingRestrictedScenario", Required = false, HelpText = "Header 'X-WOPI-UsingRestrictedScenario' used Restricted scenario")]
		public string UsingRestrictedScenario { get; set; }

		[Option("ApplicationId", Required = false, HelpText = "Header 'X-WOPI-ApplicationId' indicates id of an application stored in secure store")]
		public string ApplicationId { get; set; }

		[Option("RSACryptoKeyPairValue", Required = false, HelpText = "key-pairs match the Asymmetric encrypt algorithm used for X-WOPI-Proof header")]
		public string RSACryptoKeyPairValue { get; set; }

		[Option("RSACryptoKeyPairOldValue", Required = false, HelpText = "key-pairs match the Asymmetric encrypt algorithm used for X-WOPI-ProofOld header")]
		public string RSACryptoKeyPairOldValue { get; set; }

		[Option('s', "ignore-skipped", Required = false, HelpText = "Don't output any info about skipped tests.")]
		public bool IgnoreSkipped { get; set; }

		public static ExitCode RunCommand(RunOptions options)
		{
			// get run configuration from XML
			IEnumerable <TestExecutionData> testData = ConfigParser.ParseExecutionData(options.RunConfigurationFilePath, options.ApplicationId, options.UsingRestrictedScenario);

			// Filter the tests
			IEnumerable<TestExecutionData> executionData = testData.ApplyFilters(options);

			RSACryptoServiceProvider rsaProvider = null;
			RSACryptoServiceProvider rsaProviderOld = null;

			if (!string.IsNullOrEmpty(options.RSACryptoKeyPairValue) && !string.IsNullOrEmpty(options.RSACryptoKeyPairOldValue))
			{
				rsaProvider = new RSACryptoServiceProvider();
				rsaProvider.ImportCspBlob(Convert.FromBase64String(options.RSACryptoKeyPairValue));

				rsaProviderOld = new RSACryptoServiceProvider();
				rsaProviderOld.ImportCspBlob(Convert.FromBase64String(options.RSACryptoKeyPairOldValue));
			}		

			// Create executor groups
			var executorGroups = executionData.GroupBy(d => d.TestGroupName)
				.Select(g => new
				{
					Name = g.Key,
					Executors = g.Select(x => GetTestCaseExecutor(x, options, options.TestCategory, rsaProvider, rsaProviderOld))
				});

			ConsoleColor baseColor = ConsoleColor.White;
			HashSet<ResultStatus> resultStatuses = new HashSet<ResultStatus>();
			foreach (var group in executorGroups)
			{
				Helpers.WriteToConsole($"\nTest group: {group.Name}\n", ConsoleColor.White);

				// define execution query - evaluation is lazy; test cases are executed one at a time
				// as you iterate over returned collection
				var results = group.Executors.Select(x => x.Execute());

				// iterate over results and print success/failure indicators into console
				foreach (TestCaseResult testCaseResult in results)
				{
					resultStatuses.Add(testCaseResult.Status);
					switch (testCaseResult.Status)
					{
						case ResultStatus.Pass:
							baseColor = ConsoleColor.Green;
							Helpers.WriteToConsole($"Pass: {testCaseResult.Name}\n", baseColor, 1);
							break;

						case ResultStatus.Skipped:
							baseColor = ConsoleColor.Yellow;
							if (!options.IgnoreSkipped)
							{
								Helpers.WriteToConsole($"Skipped: {testCaseResult.Name}\n", baseColor, 1);
							}
							break;

						case ResultStatus.Fail:
						default:
							baseColor = ConsoleColor.Red;
							Helpers.WriteToConsole($"Fail: {testCaseResult.Name}\n", baseColor, 1);
							break;
					}

					if (testCaseResult.Status == ResultStatus.Fail ||
						(testCaseResult.Status == ResultStatus.Skipped && !options.IgnoreSkipped))
					{
						foreach (var request in testCaseResult.RequestDetails)
						{
							var responseStatus = (HttpStatusCode)request.ResponseStatusCode;
							var color = request.ValidationFailures.Count == 0 ? ConsoleColor.DarkGreen : baseColor;
							Helpers.WriteToConsole($"{request.Name}, response code: {request.ResponseStatusCode} {responseStatus}\n", color, 2);
							foreach (var failure in request.ValidationFailures)
							{
								foreach (var error in failure.Errors)
									Helpers.WriteToConsole($"{error.StripNewLines()}\n", baseColor, 3);
							}
						}

						Helpers.WriteToConsole($"Re-run command: .\\wopivalidator.exe -n {testCaseResult.Name} -w {options.WopiEndpoint} -t {options.AccessToken} -l {options.AccessTokenTtl}\n", baseColor, 2);
						Console.WriteLine();
					}
				}

				if (options.IgnoreSkipped && !resultStatuses.ContainsAny(ResultStatus.Pass, ResultStatus.Fail))
				{
					Helpers.WriteToConsole($"All tests skipped.\n", baseColor, 1);
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

		private static TestCaseExecutor GetTestCaseExecutor(TestExecutionData testExecutionData, RunOptions options, TestCategory inputTestCategory, RSACryptoServiceProvider rsaProvider, RSACryptoServiceProvider rsaProviderOld)
		{
			bool officeNative = inputTestCategory == TestCategory.OfficeNativeClient ||
				testExecutionData.TestCase.TestCategory == TestCategory.OfficeNativeClient;
			string userAgent = officeNative ? Constants.HeaderValues.OfficeNativeClientUserAgent : null;

			return new TestCaseExecutor(testExecutionData, options.WopiEndpoint, options.AccessToken, options.AccessTokenTtl, userAgent, rsaProvider, rsaProviderOld);
		}
	}
}
