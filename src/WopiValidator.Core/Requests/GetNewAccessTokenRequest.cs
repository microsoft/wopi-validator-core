// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetNewAccessTokenRequest : WopiRequest
	{
		public GetNewAccessTokenRequest(WopiRequestParam param) : base(param)
		{
			this.WopiSrc = param.WopiSrc;
		}

		public string WopiSrc { get; private set; }
		public override string Name { get { return Constants.Requests.GetNewAccessToken; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.GetNewAccessToken; } }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState)
		{
			return new Dictionary<string, string>
				{
					{ Constants.Headers.WopiSrc, savedState[Constants.StateOverrides.OriginalWopiSrc] },
				};
		}
	}
}
