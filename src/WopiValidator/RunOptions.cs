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

		[Option('p', "ProofKey", Required = false, HelpText = "Public key used to decrypt X-WOPI-Proof HTTP header")]
		public string ProofKey { get; set; }

		[Option('o', "ProofKeyOld", Required = false, HelpText = "Public key used to decrypt X-WOPI-ProofOld HTTP header")]
		public string ProofKeyOld { get; set; }

		[Option('s', "ignore-skipped", Required = false, HelpText = "Don't output any info about skipped tests.")]
		public bool IgnoreSkipped { get; set; }

		/// <summary>
		/// A string represents the unique key-pairs which is match the Asymmetric encrypt algorithm. It is used for "X-WOPI-Proof" header.
		/// </summary>
		private const string AsymmetricEncryptKeypairsOfCurrent = @"BwIAAACkAABSU0EyAAQAAAEAAQBxwXpxCIYvyvtnFmflVBmFEYpn/hhuZCqVH1PhqnAQr/ONAVkiONeMTToP7n2kUi1wntw5MbMaoIPWoNejZOLDgIVUqfjCOH2EbXOMmp6zTN35zAYbGZ3XWgLtmVkHIU60IQGvl7rOEHnEJ4v7a7Q2s6r4IOdVqFMS1T2YpmT6r5W6lKKyvjRKtwu3RClOcoNR9cpQNlzRqP6Tsl1B2UHAlRhNOBB7jEcttjdFFr/C1M6e5+XpDIhDlJt4BMODGN1tEAYZ71eMpnXkUBpUewXyaGZSSi5H/1cSki0srmONVNj7amPdk8QdEnlz+WnhLSjjeyNBHCVhhtVYaKfxd8LLnfRqGmPxGkcLsbTTL9Ngv1fNBFGCq45NsSNq4MGp+eja+2oJSE6duSpjeSOapLQ/vPcfkVZQP3AmOvEvxKV3wHUnzIlbvaklNhbg2LAJOZ2lT7bWVHfrgQ06lcjlapAaAoSNbPBhhhSOdUPrdRy4ebAhoUiriJiXoMNy9NS6GHqploWkCU/HDdVBTYS/yjqVFOAbhQA4edSgzyIT5P1tyIWImQ7ziE+7gFMPbXosoWmDL/iSmaAyuSU3lqun3lrBIWTXuHul48OgGadg2k2c6PBX0y7fqgBfEOAOFSii+c/d1G2umh321WigSaeg0VrsPO5pH1MbFOOFS/ZjMGWmZSYZfopkIOCqXL1UbiNwsIa0OG1FxrIMI9zgQt1aGPV4NK7unMeBz/t9uO2Y9nsrztQnYuje6K0cPTK+HlOExao=";

		/// <summary>
		/// A string represents the unique key-pairs which is match the Asymmetric encrypt algorithm. It is used for "X-WOPI-ProofOld" header.
		/// </summary>
		private const string AsymmetricEncryptKeypairsOfOld = @"BwIAAACkAABSU0EyAAQAAAEAAQC3T8ExrB2fjcvpVJF7ZYbAh9yfsHsXMcqHa/0i0ncEdoejYr1s1NMbZtGbautAmDH2Q5/dUoZ6UHvymDxGh3VypfCHg7heRaPoeBLBrKyhIbG8oy2KUlpUSBGi9s2ZTb4tMyef8ZTA+f5jneAIZDC8U4DZF0mifHJtXrQHqSY9kkHv/7WdvxVsoLToq78tX3CZDR5btKGsOJD8qjwJ0Tthsq3l79rhh39kxci9YzMKVK4rQVlUSAopVtRuWXa2j8X3eOs/YEmObMpUxEPK6yZk8Rj9UYLMm7rmO+iB0vMrTAkKOI7csfDEg+XZKSM3tRmkOJHPyUlxgeLBcR7PDX+9gVPEILISnGGj+2qsHT+ywcmC7/dYiwjhj/VXgzZvl2KjDbfaa2N10CQN6MnBMXzawvsnrSA4x8UGmzLuzLiEuTlg6Ed1WuL7uv3p+Rs9NX/yuj2jPuuFWDNNWlqGb4455eERQv73YXLNFMGRc87peBib/gN2YUO5suH8J5NzyUOGA0YPEvNWHIZo0k0JcJWzY3zVLENdKxHZFjc60bfRbAM0wTl20aWEvUEUBPOikuRmeEFveJNYSGUvcscIefhtgdTzsafiOpUW/2nR+tpxoIQM4gFZXIs85838T46kNCX1M/RBLjailtbAuyjOhA8Dixjm2jL4nndpkKPRl5ZC/pmYnUZGVd/nbh7h9rKglRSLYFzc3+OnPI9Mj0UoDPy72SE4DO6e4QN7npbklrWAKSqGUF4DNT4Y7iw5pibqSw4=";

		public static ExitCode RunCommand(RunOptions options)
		{
			// get run configuration from XML
			IEnumerable <TestExecutionData> testData = ConfigParser.ParseExecutionData(options.RunConfigurationFilePath);

			// Filter the tests
			IEnumerable<TestExecutionData> executionData = testData.ApplyFilters(options);

			ConfigParser.UsingRestrictedScenario = options.UsingRestrictedScenario;
			ConfigParser.ApplicationId = options.ApplicationId;

			RSACryptoServiceProvider rsaProvider = null;
			RSACryptoServiceProvider rsaProviderOld = null;

			if (!string.IsNullOrEmpty(options.ProofKey) && !string.IsNullOrEmpty(options.ProofKeyOld))
			{
				rsaProvider = new RSACryptoServiceProvider();
				rsaProvider.ImportCspBlob(Convert.FromBase64String(AsymmetricEncryptKeypairsOfCurrent));

				rsaProviderOld = new RSACryptoServiceProvider();
				rsaProviderOld.ImportCspBlob(Convert.FromBase64String(AsymmetricEncryptKeypairsOfOld));
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
