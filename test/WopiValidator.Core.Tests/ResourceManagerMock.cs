// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Office.WopiValidator.Core;

namespace Microsoft.Office.WopiValidator.UnitTests
{
	class ResourceManagerMock : Dictionary<string, MemoryStream>, IResourceManager
	{
		public MemoryStream GetContentStream(string resourceId)
		{
			return this[resourceId];
		}

		public string GetFileName(string resourceId)
		{
			throw new NotImplementedException();
		}
	}
}
