// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class ReadSecureStoreRequest : WopiRequest
	{
		public ReadSecureStoreRequest(WopiRequestParam param) : base(param)
		{
		}

		public override string Name { get { return Constants.Requests.ReadSecureStore; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.ReadSecureStore; } }
	}
}
