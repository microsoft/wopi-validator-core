// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Office.WopiValidator.Core
{
	/// <summary>
	/// Class to hold information pertinent to a WOPI request to hosts and its response.
	/// </summary>
	public class RequestInfo
	{
		public string Name { get; private set; }
		public string TargetUrl { get; private set; }
		public bool HasToBeSuccessful { get; private set; }
		public IEnumerable<KeyValuePair<string, string>> RequestHeaders { get; private set; }
		public int ResponseStatusCode { get; private set; }
		public IEnumerable<KeyValuePair<string, string>> ResponseHeaders { get; private set; }
		public string ResponseDataForClient { get; private set; }
		public List<ValidationResult> ValidationFailures { get; set; }
		public TimeSpan Elapsed { get; set; }
		public ProofKeyOutput CurrentProofData { get; private set; }
		public ProofKeyOutput OldProofData { get; private set; }

		public RequestInfo(
			string name,
			string targetUrl,
			bool hasToBeSuccessful,
			IEnumerable<KeyValuePair<string, string>> requestHeaders,
			int responseStatusCode,
			IEnumerable<KeyValuePair<string, string>> responseHeaders,
			string responseData,
			List<ValidationResult> validationFailures,
			TimeSpan elapsed,
			ProofKeyOutput currentProofData,
			ProofKeyOutput oldProofData)
		{
			Name = name;
			TargetUrl = targetUrl;
			HasToBeSuccessful = hasToBeSuccessful;
			RequestHeaders = requestHeaders ?? Enumerable.Empty<KeyValuePair<string, string>>();
			ResponseStatusCode = responseStatusCode;
			ResponseHeaders = responseHeaders ?? Enumerable.Empty<KeyValuePair<string, string>>();
			ResponseDataForClient = responseData;
			ValidationFailures = validationFailures ?? new List<ValidationResult>();
			Elapsed = elapsed;
			CurrentProofData = currentProofData;
			OldProofData = oldProofData;
		}
	}
}
