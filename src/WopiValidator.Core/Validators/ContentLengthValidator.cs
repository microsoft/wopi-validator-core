// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Validate that response content-length header actually matches the length of the content.
	/// </summary>
	class ContentLengthValidator : IValidator
	{
		public string Name
		{
			get { return "ContentLengthValidator"; }
		}

		public ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState)
		{
			long actualContentLength = data.ResponseStream.Length;
			string expectedContentLengthString;
			data.Headers.TryGetValue("Content-Length", out expectedContentLengthString);
			if (expectedContentLengthString == null)
			{
				// Content-Length header is optional, so pass if it's not specified.
				return new ValidationResult();
			}
			else
			{
				int expectedContentLength = Int32.Parse(expectedContentLengthString);
				if (actualContentLength == expectedContentLength)
					return new ValidationResult();
				else
				{
					return new ValidationResult(string.Format("Actual content length '{0}' of response does not match value in header content-length '{1}'",
							actualContentLength, expectedContentLength));
				}
			}
		}
	}
}
