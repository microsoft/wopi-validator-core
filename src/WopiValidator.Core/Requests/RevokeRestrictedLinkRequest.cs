// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class RevokeRestrictedLinkRequest : WopiRequest
	{
		public RevokeRestrictedLinkRequest(WopiRequestParam param) : base(param)
		{
			this.RestrictedLink = param.RestrictedLink;
		}

		public string RestrictedLink { get; private set; }
		public override string Name { get { return Constants.Requests.RevokeRestrictedLink; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.RevokeRestrictedLink; } }
		protected override IEnumerable<KeyValuePair<string, string>> DefaultHeaders
		{
			get
			{
				return new Dictionary<string, string>
				{
					{Constants.Headers.RestrictedLink, RestrictedLink}
				};
			}
		}
	}
}
