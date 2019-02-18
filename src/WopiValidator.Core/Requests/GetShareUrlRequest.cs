// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetShareUrlRequest : WopiRequest
	{
		public GetShareUrlRequest(WopiRequestParam param) : base(param)
		{
			this.UrlType = param.UrlType;
		}

		public string UrlType { get; private set; }
		public override string Name { get { return Constants.Requests.GetShareUrl; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.GetShareUrl; } }
		protected override IEnumerable<KeyValuePair<string, string>> DefaultHeaders
		{
			get
			{
				Dictionary<string, string> headers = new Dictionary<string, string>();

				if (!string.IsNullOrEmpty(this.UrlType))
				{
					headers.Add(Constants.Headers.UrlType, UrlType);
				}

				return headers;
			}
		}
	}
}
