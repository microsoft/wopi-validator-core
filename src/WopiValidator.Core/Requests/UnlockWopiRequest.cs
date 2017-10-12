// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class UnlockWopiRequest : WopiRequest
	{
		public UnlockWopiRequest(WopiRequestParam param) : base(param)
		{
			this.LockString = param.LockString;
		}

		public string LockString { get; private set; }
		public override string Name { get { return Constants.Requests.Unlock; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.Unlock; } }
		protected override IEnumerable<KeyValuePair<string, string>> DefaultHeaders
		{
			get
			{
				return new Dictionary<string, string>
				{
					{Constants.Headers.Lock, LockString}
				};
			}
		}
	}
}
