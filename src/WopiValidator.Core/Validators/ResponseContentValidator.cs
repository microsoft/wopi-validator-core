// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Validates the response content matches content of the file provided as resourceId
	/// </summary>
	class ResponseContentValidator : IValidator
	{
		public ResponseContentValidator(string resourceId)
		{
			ResourceId = resourceId;
		}

		public string ResourceId { get; private set; }

		public string Name
		{
			get { return "FileContentValidator"; }
		}

		public ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState)
		{
			MemoryStream expectedContent = resourceManager.GetContentStream(ResourceId);

			bool areEqual = StreamEquals(expectedContent, data.ResponseStream);

			if(!areEqual)
				return new ValidationResult(
					string.Format("Response stream does not match expected file content. Expected stream length: {0}, actual stream length: {1}",
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
