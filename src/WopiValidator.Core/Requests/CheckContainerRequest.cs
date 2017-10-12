// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class CheckContainerRequest : WopiRequest
	{
		public CheckContainerRequest(WopiRequestParam param) : base(param)
		{
		}

		public override string Name { get { return Constants.Requests.CheckContainer; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Get; } }
		protected override string WopiOverrideValue { get { return null; } }
	}
}
