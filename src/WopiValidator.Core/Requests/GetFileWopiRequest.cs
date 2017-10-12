// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetFileWopiRequest : WopiRequest
	{
		public GetFileWopiRequest(WopiRequestParam param) : base(param)
		{
			this.NewLockString = param.NewLockString;
		}

		public string NewLockString { get; private set; }
		public override string Name { get { return Constants.Requests.GetFile; } }
		public override bool IsTextResponseExpected { get { return false; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Get; } }
		protected override string PathOverride { get { return "/contents"; } }
		protected override IEnumerable<KeyValuePair<string, string>> DefaultHeaders
		{
			get
			{
				if (String.IsNullOrEmpty(NewLockString))
					return Enumerable.Empty<KeyValuePair<string, string>>();

				return new Dictionary<string, string>
				{
					{Constants.Headers.Lock, NewLockString}
				};
			}
		}
	}
}
