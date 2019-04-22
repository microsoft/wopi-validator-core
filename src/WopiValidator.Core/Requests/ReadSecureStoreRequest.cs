// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class ReadSecureStoreRequest : WopiRequest
	{
		public ReadSecureStoreRequest(WopiRequestParam param) : base(param)
		{
			this.ApplicationId = param.ApplicationId;
			this.PerfTraceRequested = param.PerfTraceRequested;
		}

		public string ApplicationId { get; private set; }
		public bool PerfTraceRequested { get; private set; }
		public override string Name { get { return Constants.Requests.ReadSecureStore; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.ReadSecureStore; } }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			if (string.IsNullOrEmpty(this.ApplicationId))
			{
				throw new System.Exception("No value provided for header 'X-WOPI-ApplicationId' in ReadSecureStore request! \n Provide value for header 'X-WOPI-ApplicationId' by using command line argument '--ApplicationId'.");
			}

			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add(Constants.Headers.ApplicationId, this.ApplicationId);

			if (this.PerfTraceRequested)
			{
				headers.Add(Constants.Headers.PerfTraceRequested, System.Boolean.TrueString);
			}

			return headers;
		}
	}
}
