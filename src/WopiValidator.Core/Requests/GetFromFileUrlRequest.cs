// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetFromFileUrlRequest : RequestBase
	{
		public GetFromFileUrlRequest(WopiRequestParam param)
		{
			this.OverrideUrl = param.OverrideUrl;
			this.Validators = (param.Validators ?? Enumerable.Empty<IValidator>()).ToArray();
			this.State = (param.StateSavers ?? Enumerable.Empty<IStateEntry>()).ToArray();
		}

		protected override string RequestMethod {
			get { return "Get"; }
		}

		protected override sealed string OverrideUrl { get; set; }

		public override string Name { get { return "GetFromFileUrl"; } }

		public override IResponseData Execute(
			string endpointAddress,
			string accessToken,
			long accessTokenTtl,
			ITestCase testCase,
			Dictionary<string, string> savedState,
			IResourceManager resourceManager,
			string userAgent,
			RSACryptoServiceProvider proofKeyProviderNew,
			RSACryptoServiceProvider proofKeyProviderOld)
		{
			RequestExecutionData executionData = new RequestExecutionData(
				new Uri(GetEndpointAddressOverride(savedState)),
				Enumerable.Empty<KeyValuePair<string, string>>(),
				null);

			return ExecuteRequest(executionData);
		}

		public override async Task<IResponseData> ExecuteAsync(
			string endpointAddress,
			string accessToken,
			long accessTokenTtl,
			ITestCase testCase,
			Dictionary<string, string> savedState,
			IResourceManager resourceManager,
			string userAgent,
			RSACryptoServiceProvider proofKeyProviderNew,
			RSACryptoServiceProvider proofKeyProviderOld)
		{
			RequestExecutionData executionData = new RequestExecutionData(
				new Uri(GetEndpointAddressOverride(savedState)),
				Enumerable.Empty<KeyValuePair<string, string>>(),
				null);

			return await ExecuteRequestAsync(executionData);
		}
	}
}
