using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Office.WopiValidator.Core.Factories;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Office.WopiValidator.Core.Tests.Factories
{
	[TestClass]
	public class ContentFilterFactoryUnitTests
	{
		//<ContentFilters>
		//	<ContentFilter ChunkingScheme="FullFile" StreamId="MainContent" ChunksToReturn="All" AlreadyExistingContent="" />
		//</ContentFilters>
		[TestMethod]
		public void SingleContentFilterElement_FullFile()
		{
			XElement definition = new XElement("ContentFilters",
				BuildContentFilterElement(chunkingScheme: "FullFile", streamId: "MainContent", chunksToReturn: "All", alreadyExistingContent: "")
			);

			IEnumerable<XMLContentFilter> contentFilters = ContentFilterFactory.GetContentFilters(definition);

			int count = 0;
			foreach(var contentFilter in contentFilters)
			{
				Assert.AreEqual(contentFilter.ChunkingScheme, ChunkingScheme.FullFile);
				Assert.AreEqual(contentFilter.StreamId, "MainContent");
				Assert.AreEqual(contentFilter.ChunksToReturn, ChunksToReturn.All);
				Assert.AreEqual(contentFilter.AlreadyExistingContent, "");
				count++;
			}

			Assert.AreEqual(count, 1);
		}

		//<ContentFilters>
		//	<ContentFilter ChunkingScheme="Zip" StreamId="MainContent" ChunksToReturn="All" AlreadyExistingContentResourceId="ZeroByteOfficeDocument" />
		//</ContentFilters>
		[TestMethod]
		public void SingleContentFilterElement_Zip()
		{
			XElement definition = new XElement("ContentFilters",
				BuildContentFilterElement(chunkingScheme: "Zip", streamId: "MainContent", chunksToReturn: "All", alreadyExistingContentResourceId: "ZeroByteOfficeDocument")
			);

			IEnumerable<XMLContentFilter> contentFilters = ContentFilterFactory.GetContentFilters(definition);

			int count = 0;
			foreach (var contentFilter in contentFilters)
			{
				Assert.AreEqual(contentFilter.ChunkingScheme, ChunkingScheme.Zip);
				Assert.AreEqual(contentFilter.StreamId, "MainContent");
				Assert.AreEqual(contentFilter.ChunksToReturn, ChunksToReturn.All);
				Assert.AreEqual(contentFilter.AlreadyExistingContentResourceId, "ZeroByteOfficeDocument");
				count++;
			}

			Assert.AreEqual(count, 1);
		}

		private XElement BuildContentFilterElement(
			string chunkingScheme,
			string streamId,
			string chunksToReturn,
			string alreadyExistingContent = null,
			string alreadyExistingContentResourceId = null)
		{
			List<XAttribute> attList = new List<XAttribute>();
			if (alreadyExistingContentResourceId == null)
			{
				attList = new List<XAttribute>()
				{
					new XAttribute("ChunkingScheme", chunkingScheme),
					new XAttribute("StreamId", streamId),
					new XAttribute("ChunksToReturn", chunksToReturn),
					new XAttribute("AlreadyExistingContent", alreadyExistingContent),
				};

			}
			else
			{
				attList = new List<XAttribute>()
				{
					new XAttribute("ChunkingScheme", chunkingScheme),
					new XAttribute("StreamId", streamId),
					new XAttribute("ChunksToReturn", chunksToReturn),
					new XAttribute("AlreadyExistingContentResourceId", alreadyExistingContentResourceId),
				};
			}

			return new XElement("ContentFilter", attList.ToArray());
		}
	}
}
