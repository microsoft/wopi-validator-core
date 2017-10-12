// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;

namespace Microsoft.Office.WopiValidator.Core
{
	public interface IResourceManager
	{
		MemoryStream GetContentStream(string resourceId);

		string GetFileName(string resourceId);
	}
}
