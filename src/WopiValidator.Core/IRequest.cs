// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace Microsoft.Office.WopiValidator.Core
{
	public interface IRequest
	{
		string Name { get; }
		string TargetUrl { get; }
		bool IsTextResponseExpected { get; }
		IEnumerable<KeyValuePair<string, string>> RequestHeaders { get; }
		ProofKeyOutput CurrentProofData { get; set; }
		ProofKeyOutput OldProofData { get; set; }
		IEnumerable<IValidator> Validators { get; }
		IEnumerable<IStateEntry> State { get; }

		IResponseData Execute(string endpointAddress,
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
