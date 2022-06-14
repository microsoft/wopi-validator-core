// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class RenameFileRequest : WopiRequest
	{
		public RenameFileRequest(WopiRequestParam param, string guid) : base(param)
		{
			this.LockString = param.LockString;
			this.CoauthLockId = param.CoauthLockId;
			this.RequestedName = string.Format("{0}-{1}", param.RequestedName, guid);
		}

		public string LockString { get; private set; }
		public string CoauthLockId { get; private set; }
		public string RequestedName { get; private set; }
		public override string Name { get { return Constants.Requests.RenameFile; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.RenameFile; } }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			return new Dictionary<string, string>
				{
					{ Constants.Headers.RequestedName, Encoding.UTF8.GetString(Encoding.UTF7.GetBytes(RequestedName)) },
					{ Constants.Headers.Lock, LockString },
					{ Constants.Headers.CoauthLockId, CoauthLockId }
				};
		}
	}
}
