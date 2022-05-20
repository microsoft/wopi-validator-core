using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Office.WopiValidator.Core.Factories;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Office.WopiValidator.Core.Tests.Factories
{
	[TestClass]
	public class ContentStreamFactoryUnitTests
	{
		//<ContentStreams>
		//	<ContentStream ChunkingScheme = "FullFile" StreamId="MainContent" NewContent="SampleTextVersion-1" LastKnownHostContent="" />
		//</ContentStreams>
		[TestMethod]
		public void SingleContentStreamElement_FullFile()
		{
			XElement definition = new XElement("ContentStreams",
				BuildContentStreamElement(chunkingScheme: "FullFile", streamId: "MainContent", newContent: "SampleTextVersion-1", lastKnownHostContent: "")
			);

			IEnumerable<XMLContentStream> contentStreams = ContentStreamFactory.GetContentStreams(definition);

			int count = 0;
			foreach (var contentStream in contentStreams)
			{
				Assert.AreEqual(contentStream.ChunkingScheme, ChunkingScheme.FullFile);
				Assert.AreEqual(contentStream.StreamId, "MainContent");
				Assert.AreEqual(contentStream.NewContent, "SampleTextVersion-1");
				Assert.AreEqual(contentStream.LastKnownHostContent, "");
				count++;
			}

			Assert.AreEqual(count, 1);
		}

		//<ContentStreams>
		//  <ContentStream ChunkingScheme = "Zip" StreamId="MainContent" NewContentResourceId="NonZeroByteExcel_Main_V1" LastKnownHostContentResourceId="ZeroByteOfficeDocument" />
		//</ContentStreams>
		[TestMethod]
		public void SingleContentStreamElement_Zip()
		{
			XElement definition = new XElement("ContentStreams",
				BuildContentStreamElement(chunkingScheme: "Zip", streamId: "MainContent", newContentResourceId: "NonZeroByteExcel_Main_V1", lastKnownHostContentResourceId: "ZeroByteOfficeDocument")
			);

			IEnumerable<XMLContentStream> contentStreams = ContentStreamFactory.GetContentStreams(definition);

			int count = 0;
			foreach (var contentStream in contentStreams)
			{
				Assert.AreEqual(contentStream.ChunkingScheme, ChunkingScheme.Zip);
				Assert.AreEqual(contentStream.StreamId, "MainContent");
				Assert.AreEqual(contentStream.NewContentResourceId, "NonZeroByteExcel_Main_V1");
				Assert.AreEqual(contentStream.LastKnownHostContentResourceId, "ZeroByteOfficeDocument");
				count++;
			}

			Assert.AreEqual(count, 1);
		}

		private XElement BuildContentStreamElement(
			string chunkingScheme,
			string streamId,
			string newContent = null,
			string lastKnownHostContent = null,
			string newContentResourceId = null,
			string lastKnownHostContentResourceId = null)
		{
			List<XAttribute> attList = new List<XAttribute>();
			if (newContentResourceId == null)
			{
				attList = new List<XAttribute>()
				{
					new XAttribute("ChunkingScheme", chunkingScheme),
					new XAttribute("StreamId", streamId),
					new XAttribute("NewContent", newContent),
					new XAttribute("LastKnownHostContent", lastKnownHostContent),
				};
			}
			else
			{
				attList = new List<XAttribute>()
				{
					new XAttribute("ChunkingScheme", chunkingScheme),
					new XAttribute("StreamId", streamId),
					new XAttribute("NewContentResourceId", newContentResourceId),
					new XAttribute("LastKnownHostContentResourceId", lastKnownHostContentResourceId),
				};
			}

			return new XElement("ContentStreams", attList.ToArray());
		}
	}
}
