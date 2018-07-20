// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Office.WopiValidator.Core.Logging;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core.ResourceManagement
{
	class Resource
	{
		public string ResourceId { get; private set; }
		public string FilePath { get; private set; }
		public string FileName { get; private set; }

		internal Resource(string resourceId, string filePath, string fileName)
		{
			ResourceId = resourceId;
			FilePath = filePath;
			FileName = fileName;
		}

		internal MemoryStream GetContentStream(ILogger logger = null)
		{
			if (logger == null)
			{
				logger = ApplicationLogging.CreateLogger<Resource>();
			}

			try
			{
				// Use the filename as the actual content of the stream, unless the FileName is
				// "ZeroByteFile.wopitest". This way we can still write out zero-byte files.
				MemoryStream result = new MemoryStream();
				StreamWriter sw = new StreamWriter(result);
				string fileContent = FileName == "ZeroByteFile.wopitest" ? string.Empty : ResourceId + FilePath + FileName;
				sw.Write(fileContent);
				sw.Flush();
				result.Seek(0, SeekOrigin.Begin);
				return result;
			}
			catch (IOException ex)
			{
				logger.Log("IO Exception when trying to get resource content.");
				logger.Log(ex.Message);
				return null;
			}
		}
	}
}
