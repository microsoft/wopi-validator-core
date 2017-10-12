// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class RenameFileRequest : WopiRequest
	{
		public RenameFileRequest(WopiRequestParam param) : base(param)
		{
			this.LockString = param.LockString;
			this.RequestedName = param.RequestedName;
		}

		public string LockString { get; private set; }
		public string RequestedName { get; private set; }
		public override string Name { get { return Constants.Requests.RenameFile; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.RenameFile; } }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState)
		{
			return new Dictionary<string, string>
				{
					{ Constants.Headers.RequestedName, Encoding.UTF8.GetString(Encoding.UTF7.GetBytes(RequestedName)) },
					{ Constants.Headers.Lock, LockString }
				};
		}
	}
}
