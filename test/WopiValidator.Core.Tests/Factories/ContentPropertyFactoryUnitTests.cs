using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Office.WopiValidator.Core.Factories;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Office.WopiValidator.Core.Tests.Factories
{
	[TestClass]
	public class ContentPropertyFactoryUnitTests
	{
		//<ContentProperties>
		//  <ContentProperty Name = "name1" Value="value1" Retention="KeepOnContentChange"/>
		//</ContentProperties>
		[TestMethod]
		public void SingleContentPropertyElement()
		{
			XElement definition = new XElement("ContentProperties",
				BuildContentPropertyElement(name: "name1", value: "value1", retention: "KeepOnContentChange")
			);

			IEnumerable<XMLContentProperty> contentProperties = ContentPropertyFactory.GetContentProperties(definition);

			int count = 0;
			foreach (var contentProperty in contentProperties)
			{
				Assert.AreEqual(contentProperty.Name, "name1");
				Assert.AreEqual(contentProperty.Value, "value1");
				Assert.AreEqual(contentProperty.Retention, ContentPropertyRetention.KeepOnContentChange);
				count++;
			}

			Assert.AreEqual(count, 1);
		}

		private XElement BuildContentPropertyElement(string name, string value, string retention)
		{
			List<XAttribute> attList = new List<XAttribute>()
			{
				new XAttribute("Name", name),
				new XAttribute("Value", value),
				new XAttribute("Retention", retention)
			};

			return new XElement("ContentProperties", attList.ToArray());
		}
	}
}
