// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class UnlockCoauthLockRequest : WopiRequest
	{
		public UnlockCoauthLockRequest(WopiRequestParam param) : base(param)
		{
			this.CoauthLockId = param.CoauthLockId;
		}

		public string CoauthLockId { get; private set; }
		public override string Name { get { return Constants.Requests.UnlockCoauthLock; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Post; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.UnlockCoauthLock; } }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			if (CoauthLockId != null)
			{
				headers.Add(Constants.Headers.CoauthLockId, CoauthLockId);
			}
			return headers;
		}
	}
}

