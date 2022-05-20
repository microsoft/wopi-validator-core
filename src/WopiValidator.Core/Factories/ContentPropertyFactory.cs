using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	public class ContentPropertyFactory
	{
		public static IEnumerable<XMLContentProperty> GetContentProperties(XElement definition)
		{
			return definition.Elements().Select(GetContentProperty);
		}

		/// <summary>
		/// Parses a single ContentProperty element and creates an appropriate ContentProperty object to represent it.
		/// </summary>
		private static XMLContentProperty GetContentProperty(XElement definition)
		{
			string Retention = (string)definition.Attribute("Retention");
			string Name = (string)definition.Attribute("Name");
			string Value = (string)definition.Attribute("Value");

			return new XMLContentProperty(Retention, Name, Value);
		}
	}
}
