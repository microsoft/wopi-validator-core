using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetSequenceNumberRequest : WopiRequest
	{
		public GetSequenceNumberRequest(WopiRequestParam param) : base(param)
		{
		}

		public override string Name { get { return Constants.Requests.GetSequenceNumber; } }
		public override bool IsTextResponseExpected { get { return false; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.GetSequenceNumber; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Post; } }
	}
}
