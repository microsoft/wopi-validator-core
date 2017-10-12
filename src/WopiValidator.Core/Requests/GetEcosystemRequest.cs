// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetEcosystemRequest : WopiRequest
	{
		public GetEcosystemRequest(WopiRequestParam param) : base(param)
		{
		}

		public override string Name { get { return Constants.Requests.GetEcosystem; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Get; } }
		protected override string PathOverride { get { return "/ecosystem_pointer"; } }
	}
}
