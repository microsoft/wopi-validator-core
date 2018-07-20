// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Office.WopiValidator.Core.Logging;
using Microsoft.Office.WopiValidator.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Microsoft.Office.WopiValidator.Core
{
	/// <summary>
	/// Test Case execution driver.
	/// </summary>
	public class TestCaseExecutor
	{
		private static readonly IEnumerable<IValidator> MandatoryValidators = new List<IValidator>(1) { new ContentLengthValidator() };
		private static readonly ILogger logger = ApplicationLogging.CreateLogger<TestCaseExecutor>();

		public TestCaseExecutor(
			TestExecutionData executionData,
			string wopiEndpoint,
			string accessToken,
			long accessTokenTtl,
			string userAgent,
			RSACryptoServiceProvider proofKeyProviderNew = null,
			RSACryptoServiceProvider proofKeyProviderOld = null)
		{
			TestCase = executionData.TestCase;
			PrereqCases = executionData.PrereqCases;
			ResourceManager = executionData.ResourceManager;
			WopiEndpoint = wopiEndpoint;
			AccessToken = accessToken;
			AccessTokenTtl = accessTokenTtl;
			UserAgent = userAgent;
			ProofKeyProviderNew = proofKeyProviderNew;
			ProofKeyProviderOld = proofKeyProviderOld;

			logger.LogInformation("Inside TestCaseExecutor constructor.");
		}

		public ITestCase TestCase { get; private set; }
		public IEnumerable<ITestCase> PrereqCases { get; private set; }
		public IResourceManager ResourceManager { get; private set; }
		public string WopiEndpoint { get; private set; }
		public string AccessToken { get; private set; }
		public long AccessTokenTtl { get; private set; }
		public string UserAgent { get; private set; }
		public RSACryptoServiceProvider ProofKeyProviderNew { get; private set; }
		public RSACryptoServiceProvider ProofKeyProviderOld { get; private set; }

		public TestCaseResult Execute(ILogger logger)
		{
			IEnumerable<TestCaseResult> prereqResults = from testCase in PrereqCases select ExecuteTestCase(testCase, logger);
			//PrereqCases.Select(ExecuteTestCase);

			// Although multiple prereq tests may fail, we are surfacing details for only the first one.
			TestCaseResult failure = prereqResults.FirstOrDefault(r => r.Status != ResultStatus.Pass);

			if (failure != null)
				return new TestCaseResult(TestCase.Name, failure.RequestDetails, "Prerequisites failed", failure.Errors, ResultStatus.Skipped);

			return ExecuteTestCase(TestCase, logger);
		}

		/// <summary>
		/// Executes single TestCase:
		/// - for each of the WOPI requests defined for that test case
		/// --- executes the requests
		/// --- runs the validations
		/// </summary>
		private TestCaseResult ExecuteTestCase(ITestCase testCase, ILogger logger)
		{
			IList<RequestInfo> requestDetails = new List<RequestInfo>();
			Dictionary<string, string> savedState = new Dictionary<string, string>()
			{
				{ Constants.StateOverrides.OriginalAccessToken, AccessToken },
				{ Constants.StateOverrides.OriginalWopiSrc, WopiEndpoint },
			};

			TestCaseResult finalTestResult = null;

			try
			{
				foreach (IRequest request in testCase.Requests)
				{
					IResponseData responseData;

					try
					{
						responseData = request.Execute(WopiEndpoint,
							AccessToken,
							AccessTokenTtl,
							testCase,
							savedState,
							ResourceManager,
							UserAgent,
							ProofKeyProviderNew,
							ProofKeyProviderOld);
					}
					catch (ProofKeySigningException ex)
					{
						responseData = ExceptionHelper.WrapExceptionInResponseData(ex);
					}

					IEnumerable<IValidator> validators = MandatoryValidators.Concat(request.Validators);
					List<ValidationResult> validationResults = validators.Select(validator => validator.Validate(responseData, ResourceManager, savedState)).ToList();
					List<ValidationResult> validationFailures = validationResults.Where(r => r.HasFailures).ToList();

					string responseContent = GetResponseContentForClient(responseData);

					RequestInfo requestInfo = new RequestInfo(
						request.Name,
						request.TargetUrl,
						request.RequestHeaders,
						responseData.StatusCode,
						responseData.Headers,
						responseContent,
						validationFailures,
						responseData.Elapsed,
						request.CurrentProofData,
						request.OldProofData);

					requestDetails.Add(requestInfo);

					// return on the first request that fails
					if (validationFailures.Any())
					{
						string testCaseResultMessage = !String.IsNullOrWhiteSpace(testCase.FailMessage)
							? testCase.FailMessage
							: string.Format("{0} request failed.", request.Name);

						finalTestResult = new TestCaseResult(
							testCase.Name,
							requestDetails,
							testCaseResultMessage,
							validationFailures.SelectMany(x => x.Errors),
							ResultStatus.Fail);
						break;
					}

					// Save any state that was requested
					foreach (IStateEntry stateSaver in request.State)
					{
						savedState[stateSaver.Name] = stateSaver.GetValue(responseData);
					}
				}
			}
			finally
			{
				// run the cleanup cases if there were any
				RunCleanupRequests(testCase, savedState, requestDetails, logger);
			}

			if (finalTestResult == null)
			{
				// if we're here, there were no errors.
				finalTestResult = new TestCaseResult(testCase.Name, requestDetails, ResultStatus.Pass);
			}

			return finalTestResult;
		}

		private string GetResponseContentForClient(IResponseData responseData)
		{
			string responseContent = "No content";
			if (ShouldGetResponseContentString(responseData))
			{
				if (responseData.IsTextResponse)
				{
					string responseContentString = responseData.GetResponseContentAsString();
					if (responseContentString != null)
					{
						if (responseContentString.Length > 8000)
						{
							responseContent = responseContentString.Substring(0, 8000);
						}
						else
						{
							responseContent = responseContentString;
						}
					}
				}
				else
				{
					responseContent = "Non text content";
				}
			}
			return responseContent;
		}

		private bool ShouldGetResponseContentString(IResponseData responseData)
		{
			string contentLength;
			string transferEncoding;
			if ((responseData.Headers.TryGetValue("Content-Length", out contentLength) && contentLength != "0")
				|| (responseData.Headers.TryGetValue("Transfer-Encoding", out transferEncoding) && transferEncoding == "chunked"))
			{
				return true;
			}
			return false;
		}

		private void RunCleanupRequests(ITestCase testCase, Dictionary<string, string> savedState, IList<RequestInfo> requestDetails, ILogger logger)
		{
			if (testCase.CleanupRequests == null)
				return;

			foreach (IRequest request in testCase.CleanupRequests)
			{
				try
				{
					IResponseData responseData = request.Execute(WopiEndpoint,
						AccessToken,
						AccessTokenTtl,
						testCase,
						savedState,
						ResourceManager,
						UserAgent,
						ProofKeyProviderNew,
						ProofKeyProviderOld);

					// No validators needed, they're just cleanup and we don't care if they worked or not.

					string responseContent = GetResponseContentForClient(responseData);

					RequestInfo requestInfo = new RequestInfo(
						request.Name,
						request.TargetUrl,
						request.RequestHeaders,
						responseData.StatusCode,
						responseData.Headers,
						responseContent,
						Enumerable.Empty<ValidationResult>().ToList(),
						responseData.Elapsed,
						request.CurrentProofData,
						request.OldProofData);

					requestDetails.Add(requestInfo);
				}
				catch (Exception)
				{
					// Swallow all exceptions from the cleanup requests - we don't care if they work and
					// some will fail by design if the test's requests failed.

					RequestInfo requestInfo = new RequestInfo(
						request.Name,
						request.TargetUrl,
						request.RequestHeaders,
						0,
						Enumerable.Empty<KeyValuePair<string, string>>(),
						"no content: request failed",
						Enumerable.Empty<ValidationResult>().ToList(),
						TimeSpan.Zero,
						request.CurrentProofData,
						request.OldProofData);

					requestDetails.Add(requestInfo);
				}
			}
		}
	}
}
