// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	public interface IResourceManagerFactory
	{
		/// <summary>
		/// Creates IResourceManager instance based on configuration from provided XML content.
		/// </summary>
		/// <param name="definition"><![CDATA[<Resources>]]> element from run configuration XML file.</param>
		/// <returns>IResourceManager implementation with resources from run configuration XML.</returns>
		IResourceManager GetResourceManager(XElement definition);
	}
}
