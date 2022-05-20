// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetCoauthLockRequest : WopiRequest
	{
		public GetCoauthLockRequest(WopiRequestParam param) : base(param)
		{
			this.CoauthLockType = param.CoauthLockType;
			this.CoauthLockMetadata = param.CoauthLockMetadata;
			this.CoauthLockId = param.CoauthLockId;
			this.CoauthLockExpirationTimeout = param.CoauthLockExpirationTimeout;
		}

		public uint? CoauthLockExpirationTimeout { get; private set; }
		public string CoauthLockMetadata { get; private set; }
		public string CoauthLockId { get; private set; }
		public CoauthLockType? CoauthLockType { get; private set; }
		public override string Name { get { return Constants.Requests.GetCoauthLock; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Post; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.GetCoauthLock; } }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();

			if (CoauthLockType != null)
			{
				headers.Add(Constants.Headers.CoauthLockType, CoauthLockType.Value.ToString());
			}
			if (CoauthLockMetadata != null)
			{
				headers.Add(Constants.Headers.CoauthLockMetadata, CoauthLockMetadata);
			}
			if (CoauthLockId != null)
			{
				headers.Add(Constants.Headers.CoauthLockId, CoauthLockId);
			}
			if (CoauthLockExpirationTimeout != null)
			{
				headers.Add(Constants.Headers.CoauthLockExpirationTimeout, CoauthLockExpirationTimeout.Value.ToString());
			}
			return headers;
		}
	}
}
