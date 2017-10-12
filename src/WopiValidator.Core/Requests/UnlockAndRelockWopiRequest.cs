// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class UnlockAndRelockWopiRequest : WopiRequest
	{
		public UnlockAndRelockWopiRequest(WopiRequestParam param) : base(param)
		{
			this.NewLockString = param.NewLockString;
			this.OldLockString = param.OldLockString;
		}

		public string NewLockString { get; private set; }
		public string OldLockString { get; set; }
		public override string Name { get { return Constants.Requests.UnlockAndRelock; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.Lock; } }
		protected override IEnumerable<KeyValuePair<string, string>> DefaultHeaders
		{
			get
			{
				return new Dictionary<string, string>
				{
					{Constants.Headers.Lock, NewLockString},
					{Constants.Headers.OldLock, OldLockString}
				};
			}
		}
	}
}
