// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class EnumerateChildrenRequest : WopiRequest
	{
		public string FileExtensionFilterList { get; private set; }

		public EnumerateChildrenRequest(WopiRequestParam param) : base(param)
		{
			this.FileExtensionFilterList = param.FileExtensionFilterList;
		}

		public override string Name { get { return Constants.Requests.EnumerateChildren; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Get; } }
		protected override string PathOverride { get { return "/children"; } }

		protected override IEnumerable<KeyValuePair<string, string>> DefaultHeaders
		{
			get
			{
				Dictionary<string, string> headers = new Dictionary<string, string> { };

				if (!String.IsNullOrEmpty(this.FileExtensionFilterList))
					headers.Add(Constants.Headers.FileExtensionFilterList, this.FileExtensionFilterList);

				return headers;
			}
		}
	}
}
