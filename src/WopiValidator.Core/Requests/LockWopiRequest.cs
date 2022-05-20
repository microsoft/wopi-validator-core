// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class LockWopiRequest : WopiRequest
	{
		public LockWopiRequest(WopiRequestParam param) : base(param)
		{
			this.LockString = param.LockString;
			this.LockUserVisible = param.LockUserVisible;
		}

		public string LockString { get; private set; }
		public override string Name { get { return Constants.Requests.Lock; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.Lock; } }
		public bool? LockUserVisible { get; private set; }
		protected override IEnumerable<KeyValuePair<string, string>> DefaultHeaders
		{
			get
			{
				Dictionary<string, string> headers = new Dictionary<string, string>
				{
					{Constants.Headers.Lock, LockString}
				};

				if (LockUserVisible.HasValue)
				{
					headers.Add(Constants.Headers.LockUserVisible, LockUserVisible.Value.ToString());
				}

				return headers;
			}
		}
	}
}
