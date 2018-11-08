// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class RenameContainerRequest : WopiRequest
	{
		public RenameContainerRequest(WopiRequestParam param) : base(param)
		{
			this.RequestedName = param.RequestedName;
		}

		public string RequestedName { get; private set; }
		public override string Name { get { return Constants.Requests.RenameContainer; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.RenameContainer; } }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			return new Dictionary<string, string>
				{
					{ Constants.Headers.RequestedName, Encoding.UTF8.GetString(Encoding.UTF7.GetBytes(RequestedName)) },
				};
		}
	}
}
