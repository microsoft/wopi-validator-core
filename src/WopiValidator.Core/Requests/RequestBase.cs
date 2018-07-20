// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Office.WopiValidator.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	abstract class RequestBase : IRequest
	{
		protected ILogger logger = ApplicationLogging.CreateLogger<RequestBase>();

		/// <summary>
		/// Whether request should be made as POST or GET.
		/// </summary>
		protected virtual string RequestMethod
		{
			get { return Constants.RequestMethods.Post; }
		}

		/// <summary>
		/// Override url to use for executing requests.
		/// </summary>
		protected virtual string OverrideUrl { get; set; }

		/// <summary>
		/// Used to add "/contents" to request url for PutFile/GetFile.
		/// </summary>
		protected virtual string PathOverride
		{
			get { return null; }
		}

		/// <summary>
		/// Indicates if the response is expected to be textual.
		/// </summary>
		public virtual bool IsTextResponseExpected { get { return true; } }

		public string TargetUrl { get; private set; }
		public IEnumerable<KeyValuePair<string, string>> RequestHeaders { get; private set; }
		public ProofKeyOutput CurrentProofData { get; set; }
		public ProofKeyOutput OldProofData { get; set; }

		public abstract string Name { get; }

		public IEnumerable<IValidator> Validators { get; protected set; }

		public IEnumerable<IStateEntry> State { get; protected set; }

		/// <summary>
		/// Executes request and gathers response data.
		/// </summary>
		/// <param name="targetUri">URI request should be made against</param>
		/// <param name="headers">Set of custom headers that should be added to the request</param>
		/// <param name="content">Request content stream</param>
		/// <returns>IResponseData instance with information takes from response.</returns>
		protected IResponseData ExecuteRequest(
			RequestExecutionData executionData,
			string userAgent = null
			)
		{
			TargetUrl = executionData.TargetUri.AbsoluteUri;
			RequestHeaders = executionData.Headers.ToArray();

			HttpWebRequest request = WebRequest.CreateHttp(executionData.TargetUri);
			request.UserAgent = userAgent;

			// apply custom headers
			foreach (KeyValuePair<string, string> header in RequestHeaders)
				request.Headers.Add(header.Key, header.Value);

			request.Method = RequestMethod;

			MemoryStream content = executionData.ContentStream;
			// set proper ContentLength and content stream
			if (content != null)
			{
				request.ContentLength = content.Length;
				using (Stream requestStream = request.GetRequestStream())
				{
					content.Seek(0, SeekOrigin.Begin);
					content.CopyTo(requestStream);
				}
			}
			else
			{
				request.ContentLength = 0;
			}
			Stopwatch timer = new Stopwatch();
			try
			{
				timer.Start();
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					timer.Stop();
					return GetResponseData(response, IsTextResponseExpected, timer.Elapsed);
				}
			}
			// ProtocolErrors will have a non-null Response object so we can still get response details
			catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError)
			{
				logger.Log($"Request raised an Exception, but hasToBeSuccessful is false, so we're continuing...");
				using (HttpWebResponse response = (HttpWebResponse)ex.Response)
				{
					timer.Stop();
					return GetResponseData(response, IsTextResponseExpected, timer.Elapsed);
				}
			}
			// no response, so we wrap the exception details so they can be included in a validation failure
			catch (WebException ex)
			{
				return ExceptionHelper.WrapExceptionInResponseData(ex);
			}
		}


		/// <summary>
		/// Replaces EndpointAddress if set
		/// </summary>
		protected string GetEndpointAddressOverride(Dictionary<string, string> savedState)
		{
			if (String.IsNullOrEmpty(OverrideUrl))
				return null;

			string overrideSetting = null;
			if (OverrideUrl.StartsWith(Constants.StateOverrides.StateToken))
				overrideSetting = OverrideUrl.Substring(Constants.StateOverrides.StateToken.Length);

			string urlToUse = OverrideUrl;
			if (!String.IsNullOrEmpty(overrideSetting) &&
				!savedState.TryGetValue(overrideSetting, out urlToUse))
			{
				throw new InvalidOperationException("OverrideUrl specified in definition but not found in state dictionary.  Did it depend on a request that failed?");
			}

			if (String.IsNullOrEmpty(urlToUse))
			{
				return null;
			}
			else if (String.IsNullOrEmpty(PathOverride))
			{
				return urlToUse;
			}
			else
			{
				UriBuilder builder = new UriBuilder(urlToUse);
				builder.Path += PathOverride;
				return builder.ToString();
			}
		}

		/// <summary>
		/// Gets information from the response.
		/// </summary>
		/// <returns>IResponseData instance with information from the response.</returns>
		private static IResponseData GetResponseData(HttpWebResponse response, bool isTextResponseExpected, TimeSpan elapsed)
		{
			MemoryStream content = new MemoryStream();
			using (Stream responseStream = response.GetResponseStream())
			{
				if (responseStream != null)
					responseStream.CopyTo(content);
			}

			// just to be sure
			content.Seek(0, SeekOrigin.Begin);

			Dictionary<string, string> headers = response.Headers
				.Cast<string>()
				.Select(k => new { Key = k, Value = response.Headers[k] })
				.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

			return new ResponseData(content, (int)response.StatusCode, headers, isTextResponseExpected, elapsed);
		}

		public abstract IResponseData Execute(string endpointAddress,
			string accessToken,
			long accessTokenTtl,
			ITestCase testCase,
			Dictionary<string, string> savedState,
			IResourceManager resourceManager,
			string userAgent,
			RSACryptoServiceProvider proofKeyProviderNew,
			RSACryptoServiceProvider proofKeyProviderOld);
	}
}
