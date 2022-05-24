using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Office.WopiValidator.Core.Factories;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Office.WopiValidator.Core.Tests.Factories
{
	[TestClass]
	public class ContentPropertyToReturnFactoryUnitTests
	{
		//<ContentPropertiesToReturn>
		//	<ContentPropertyToReturn Value = "name1" />
		//</ContentPropertiesToReturn>
		[TestMethod]
		public void SingleContentPropertyToReturn()
		{
			XElement definition = new XElement("ContentPropertiesToReturn",
				BuildContentPropertyToReturn(value: "name1")
			);

			IEnumerable<XMLContentPropertyToReturn> contentPropertiesToReturn = ContentPropertyToReturnFactory.GetContentPropertiesToReturn(definition);

			int count = 0;
			foreach (var contentPropertyToReturn in contentPropertiesToReturn)
			{
				Assert.AreEqual(contentPropertyToReturn.Value, "name1");
				count++;
			}

			Assert.AreEqual(count, 1);
		}

		private XElement BuildContentPropertyToReturn(string value)
		{
			List<XAttribute> attList = new List<XAttribute>()
			{
				new XAttribute("Value", value)
			};

			return new XElement("ContentPropertyToReturn", attList.ToArray());
		}
	}
}
