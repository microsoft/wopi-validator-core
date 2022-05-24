// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class DelayRequest : WopiRequest
	{
		public DelayRequest(WopiRequestParam param) : base(param)
		{
			this.DelayTimeInSeconds = param.DelayTimeInSeconds;
		}

		public uint? DelayTimeInSeconds { get; private set; }
		public override string Name { get { return Constants.Requests.Delay; } }
	}
}
