// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
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
		private readonly ILogger _logger;
		private readonly Dictionary<string, Resource> _resources;

		public ResourceManager(IEnumerable<Resource> files, ILogger logger)
		{
			_logger = logger;
			_resources = files.ToDictionary(x => x.ResourceId);
		}

		public MemoryStream GetContentStream(string resourceId)
		{
			Resource resource;
			if (TryGetResource(resourceId, out resource))
				return resource.GetContentStream(_logger);

			throw new ArgumentException(string.Format("Resource with resourceId '{0}' doesn't exist.", resourceId), "resourceId");
		}

		public void GetZipChunkingBlobs(string resourceId, out string[] blobIds, out IReadOnlyDictionary<string, IBlob> blobs)
		{
			Resource resource;
			if (TryGetResource(resourceId, out resource))
			{
				resource.GetZipChunkingBlobs(_logger, out blobIds, out blobs);
				return;
			}

			throw new ArgumentException($"{nameof(ResourceManager.GetZipChunkingBlobs)} Resource with resourceId '{resourceId}' doesn't exist.");
		}

		public Stream GetZipChunkingResourceStream(string resourceId)
		{
			Resource resource;
			if (TryGetResource(resourceId, out resource))
				return resource.GetZipChunkingResourceStream(_logger);

			throw new ArgumentException($"{nameof(ResourceManager.GetZipChunkingResourceStream)} Resource with resourceId '{resourceId}' doesn't exist.");
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
