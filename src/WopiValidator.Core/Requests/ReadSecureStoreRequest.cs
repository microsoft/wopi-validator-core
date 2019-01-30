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
		}

		public string ApplicationId { get; private set; }
		public override string Name { get { return Constants.Requests.ReadSecureStore; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.ReadSecureStore; } }

		protected override IEnumerable<KeyValuePair<string, string>> DefaultHeaders
		{
			get
			{
				return new Dictionary<string, string>
				{
					{Constants.Headers.ApplicationId, this.ApplicationId}
				};
			}
		}
	}
}
