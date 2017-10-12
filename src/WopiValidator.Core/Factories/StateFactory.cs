// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	static class StateFactory
	{
		public static IEnumerable<IStateEntry> GetStateExpressions(XElement definition)
		{
			return definition.Elements().Select(GetStateEntry);
		}

		/// <summary>
		/// Parses a single State element and creates an appropriate StateEntry object to represent it.
		/// </summary>
		private static IStateEntry GetStateEntry(XElement definition)
		{
			string name = (string)definition.Attribute("Name");
			string source = (string)definition.Attribute("Source");

			string sourceTypeString = (string)definition.Attribute("SourceType");
			if (!String.IsNullOrEmpty(sourceTypeString))
			{
				StateSourceType sourceType;
				if (Enum.TryParse(sourceTypeString, true, out sourceType))
				{
					return new StateEntry(name, source, sourceType);
				}
			}

			return new StateEntry(name, source);
		}
	}
}
