using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	public class ContentPropertyToReturnFactory
	{
		public static IEnumerable<XMLContentPropertyToReturn> GetContentPropertiesToReturn(XElement definition)
		{
			return definition.Elements().Select(GetContentPropertyToReturn);
		}

		/// <summary>
		/// Parses a single ContentPropertyToReturn element and creates an appropriate ContentPropertyToReturn object to represent it.
		/// </summary>
		private static XMLContentPropertyToReturn GetContentPropertyToReturn(XElement definition)
		{
			string Value = (string)definition.Attribute("Value");

			return new XMLContentPropertyToReturn(Value);
		}
	}
}
