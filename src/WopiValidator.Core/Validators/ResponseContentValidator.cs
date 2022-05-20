// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Validates the response content matches content of the file provided as resourceId
	/// </summary>
	class ResponseContentValidator : IValidator
	{
		public ResponseContentValidator(string resourceId, string expectedBodyContent = null)
		{
			if (!string.IsNullOrEmpty(expectedBodyContent) && !string.IsNullOrEmpty(resourceId))
			{
				throw new ArgumentException("Both expectedBodyContent and resourceId are not null/empty.");
			}

			ResourceId = resourceId;
			ExpectedBodyContent = expectedBodyContent;
		}

		public string ResourceId { get; private set; }

		public string ExpectedBodyContent { get; private set; }

		public string Name { get { return "ResponseContentValidator"; } }

		public ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState)
		{
			MemoryStream expectedContent;
			if (!string.IsNullOrEmpty(ExpectedBodyContent))
			{
				expectedContent = new MemoryStream(Encoding.UTF8.GetBytes(ExpectedBodyContent));
			}
			else
			{
				expectedContent = resourceManager.GetContentStream(ResourceId);
			}

			expectedContent.Seek(0, SeekOrigin.Begin);
			data.ResponseStream.Seek(0, SeekOrigin.Begin);
			bool areEqual = StreamUtil.StreamEquals(expectedContent, data.ResponseStream);

			if (!areEqual)
				return new ValidationResult(
					string.Format("Response body does not match expected content. Expected stream length: {0}, actual stream length: {1}",
						expectedContent.Length, data.ResponseStream.Length));

			return new ValidationResult();
		}
	}
}
