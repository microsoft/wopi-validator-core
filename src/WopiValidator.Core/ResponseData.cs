// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core
{
	class ResponseData : IResponseData
	{
		public ResponseData(Stream responseStream,
			int statusCode,
			Dictionary<string, string> headers,
			bool isTextResponse,
			TimeSpan elapsed)
		{
			ResponseStream = responseStream;
			StatusCode = statusCode;
			Headers = new CaseInsensitiveDictionary(headers);
			IsTextResponse = isTextResponse;
			if (IsTextResponse)
				ResponseContentText = ReadContent(responseStream);
			else
				ResponseContentText = null;
			Elapsed = elapsed;
		}

		public string ResponseContentText { get; private set; }
		public Stream ResponseStream { get; internal set; }
		public int StatusCode { get; internal set; }
		public CaseInsensitiveDictionary Headers { get; internal set; }
		public bool IsTextResponse { get; internal set; }
		public TimeSpan Elapsed { get; internal set; }

		public string GetResponseContentAsString()
		{
			return ResponseContentText;
		}

		private string ReadContent(Stream stream)
		{
			try
			{
				stream.Seek(0, SeekOrigin.Begin);
				using (StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8, true, 1024, true))
				{
					return streamReader.ReadToEnd();
				}
			}
			catch (ArgumentException)
			{
				return null;
			}
			catch (IOException)
			{
				return null;
			}
		}
	}
}
