// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Office.WopiValidator.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Office.WopiValidator.Core.ResourceManagement
{
	/// <summary>
	/// Provides access to resources.
	/// </summary>
	class ResourceManager : IResourceManager
	{
		private readonly ILogger logger;
		private readonly Dictionary<string, Resource> _resources;

		public ResourceManager(IEnumerable<Resource> files)
		{
			logger = ApplicationLogging.CreateLogger<ResourceManager>();
			_resources = files.ToDictionary(x => x.ResourceId);
		}

		public MemoryStream GetContentStream(string resourceId)
		{
			Resource resource;
			if (TryGetResource(resourceId, out resource))
				return resource.GetContentStream(logger);

			throw new ArgumentException(string.Format("Resource with resourceId '{0}' doesn't exist.", resourceId), "resourceId");
		}

		public string GetFileName(string resourceId)
		{
			Resource resource;
			if (TryGetResource(resourceId, out resource))
				return resource.FileName;

			throw new ArgumentException(string.Format("Resource with resourceId '{0}' doesn't exist.", resourceId), "resourceId");

		}

		private bool TryGetResource(string resourceId, out Resource resource)
		{
			if (resourceId == null)
				throw new ArgumentNullException("resourceId");
			if (string.IsNullOrEmpty(resourceId))
				throw new ArgumentException("ResourceId cannot be empty", "resourceId");

			if (!_resources.TryGetValue(resourceId, out resource))
			{
				return false;
			}

			return true;
		}
	}
}
