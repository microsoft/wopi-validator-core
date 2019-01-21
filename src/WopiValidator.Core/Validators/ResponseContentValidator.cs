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
				string setting = null;
				if (ExpectedBodyContent.StartsWith(Constants.StateOverrides.StateToken))
					setting = ExpectedBodyContent.Substring(Constants.StateOverrides.StateToken.Length);

				string expectedBodyContent = ExpectedBodyContent;
				if (!String.IsNullOrEmpty(setting) &&
					!savedState.TryGetValue(setting, out expectedBodyContent))
				{
					throw new InvalidOperationException("OverrideUrl specified in definition but not found in state dictionary.  Did it depend on a request that failed?");
				}

				expectedContent = new MemoryStream(Encoding.UTF8.GetBytes(expectedBodyContent));
			}
			else
			{
				expectedContent = resourceManager.GetContentStream(ResourceId);
			}

			bool areEqual = StreamEquals(expectedContent, data.ResponseStream);

			if (!areEqual)
				return new ValidationResult(
					string.Format("Response body does not match expected content. Expected stream length: {0}, actual stream length: {1}",
						expectedContent.Length, data.ResponseStream.Length));

			return new ValidationResult();
		}

		static bool StreamEquals(Stream stream1, Stream stream2)
		{
			stream1.Seek(0, SeekOrigin.Begin);
			stream2.Seek(0, SeekOrigin.Begin);

			const int bufferSize = 2048;
			byte[] buffer1 = new byte[bufferSize]; //buffer size
			byte[] buffer2 = new byte[bufferSize];
			while (true)
			{
				int count1 = stream1.Read(buffer1, 0, bufferSize);
				int count2 = stream2.Read(buffer2, 0, bufferSize);

				if (count1 != count2)
					return false;

				if (count1 == 0)
					return true;

				if (!buffer1.Take(count1).SequenceEqual(buffer2.Take(count2)))
					return false;
			}
		}
	}
}
