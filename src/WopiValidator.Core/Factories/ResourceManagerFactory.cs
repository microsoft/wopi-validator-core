// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Office.WopiValidator.Core.Logging;
using Microsoft.Office.WopiValidator.Core.ResourceManagement;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	class ResourceManagerFactory : IResourceManagerFactory
	{
		/// <summary>
		/// Parses Resources element and instantiates IResourceManager instance with resource found in XML.
		/// </summary>
		public IResourceManager GetResourceManager(XElement definition)
		{
			IEnumerable<Resource> files = definition.Elements("File").Select(GetResource);
			return new ResourceManager(files, ConsoleLogger.Instance);
		}

		/// <summary>
		/// Parses single resource information
		/// </summary>
		private static Resource GetResource(XElement definition)
		{
			string id = (string)definition.Attribute("Id");
			string filePath = (string)definition.Attribute("FilePath");
			string fileName = (string) definition.Attribute("Name");

			return new Resource(id, filePath, fileName);
		}
	}
}
