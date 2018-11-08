// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core;
using System;

namespace Microsoft.Office.WopiValidator.UnitTests
{
	public class ResponseDataMock : IResponseData
	{
		public System.IO.Stream ResponseStream { get; set; }

		public int StatusCode { get; set; }

		public CaseInsensitiveDictionary Headers { get; set; }

		public bool IsTextResponse { get; set; }

		public string ResponseContentText { get; set; }
		public string GetResponseContentAsString()
		{
			if (IsTextResponse)
				return ResponseContentText;
			else
				return null;
		}

		public TimeSpan Elapsed { get; set; }
	}
}
