// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class CheckFolderInfoRequest : WopiRequest
	{
		public CheckFolderInfoRequest(WopiRequestParam param) : base(param)
		{
		}

		public override string Name { get { return Constants.Requests.CheckFolderInfo; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Get; } }
		protected override string WopiOverrideValue { get { return null; } }
	}
}
