// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class RevokeRestrictedLinkRequest : WopiRequest
	{
		public RevokeRestrictedLinkRequest(WopiRequestParam param) : base(param)
		{
			this.RestrictedLinkType = param.RestrictedLinkType;
			this.UsingRestrictedScenario = param.UsingRestrictedScenario;
		}

		public string RestrictedLinkType { get; private set; }
		public string UsingRestrictedScenario { get; private set; }
		public override string Name { get { return Constants.Requests.RevokeRestrictedLink; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.RevokeRestrictedLink; } }
		protected override IEnumerable<KeyValuePair<string, string>> DefaultHeaders
		{
			get
			{
				Dictionary<string, string> headers = new Dictionary<string, string>();
				headers.Add(Constants.Headers.RestrictedLink, RestrictedLinkType);

				if (!string.IsNullOrEmpty(UsingRestrictedScenario))
				{
					headers.Add(Constants.Headers.UsingRestrictedScenario, UsingRestrictedScenario);
				}

				return headers;
			}
		}
	}
}
