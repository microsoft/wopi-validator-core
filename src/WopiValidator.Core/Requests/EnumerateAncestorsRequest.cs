// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class EnumerateAncestorsRequest : WopiRequest
	{
		public EnumerateAncestorsRequest(WopiRequestParam param) : base(param)
		{
		}

		public override string Name { get { return Constants.Requests.EnumerateAncestors; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Get; } }
		protected override string PathOverride { get { return "/ancestry"; } }
	}
}
