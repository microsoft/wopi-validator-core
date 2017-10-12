// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core
{
	public interface IResponseData
	{
		Stream ResponseStream { get; }

		bool IsTextResponse { get; }

		int StatusCode { get; }

		CaseInsensitiveDictionary Headers { get; }

		string GetResponseContentAsString();

		TimeSpan Elapsed { get; }
	}

	public class CaseInsensitiveDictionary : Dictionary<string, string>
	{
		public CaseInsensitiveDictionary() : base(StringComparer.OrdinalIgnoreCase)
		{ }

		public CaseInsensitiveDictionary(Dictionary<string, string> dictionary)
			: base(dictionary, StringComparer.OrdinalIgnoreCase)
		{ }

		public CaseInsensitiveDictionary(int size) : base(size, StringComparer.OrdinalIgnoreCase)
		{ }
	}
}
