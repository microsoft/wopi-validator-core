// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;

namespace Microsoft.Office.WopiValidator.Core
{
	public interface IResourceManager
	{
		MemoryStream GetContentStream(string resourceId);

		string GetFileName(string resourceId);

		void GetZipChunkingBlobs(string resourceId, out string[] blobIds, out IReadOnlyDictionary<string, IBlob> blobs);

		Stream GetZipChunkingResourceStream(string resourceId);
	}
}
