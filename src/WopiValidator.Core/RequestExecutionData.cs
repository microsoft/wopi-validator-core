// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core
{
	/// <summary>
	/// Class to hold information required to execute an HTTP request to Wopi hosts.
	/// </summary>
	public class RequestExecutionData
	{
		public RequestExecutionData(Uri targetUri, IEnumerable<KeyValuePair<string, string>> headers, MemoryStream contentStream)
		{
			TargetUri = targetUri;
			Headers = headers;
			ContentStream = contentStream;
		}

		public Uri TargetUri { get; private set; }

		public IEnumerable<KeyValuePair<string, string>> Headers { get; private set; }

		public MemoryStream ContentStream { get; private set; }
	}
}
