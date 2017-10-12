// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class DeleteContainerRequest : WopiRequest
	{
		public DeleteContainerRequest(WopiRequestParam param) : base(param)
		{
		}

		public override string Name { get { return Constants.Requests.DeleteContainer; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.DeleteContainer; } }
	}
}
