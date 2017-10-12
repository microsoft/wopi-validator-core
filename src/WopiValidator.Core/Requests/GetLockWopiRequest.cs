// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetLockWopiRequest : WopiRequest
	{
		public GetLockWopiRequest(WopiRequestParam param) : base(param)
		{
		}

		public override string Name { get { return Constants.Requests.GetLock; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.GetLock; } }
	}
}
