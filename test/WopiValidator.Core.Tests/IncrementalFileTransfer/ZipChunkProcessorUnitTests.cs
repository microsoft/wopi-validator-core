using Microsoft.Office.WopiValidator.Core.Factories;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core.Tests.IncrementalFileTransfer
{
	[TestClass]
	public class ZipChunkProcessorUnitTests
	{
		[TestMethod]
		public void StreamToBlobs()
		{
			XElement definition = new XElement("Resources",
				BuildFileElement(id: "NonZeroByteExcel_Main_V1", name: "ChunkIds.txt", filePath: "Microsoft.Office.WopiValidator.Core.Resources.ZipChunking.NonZeroByteExcel.Version1.MainContent.")
			);

			IResourceManager resourceManager = new ResourceManagerFactory().GetResourceManager(definition);
			new ZipChunkProcessor().StreamToBlobs(resourceManager, "NonZeroByteExcel_Main_V1", out string[] blobIds, out IReadOnlyDictionary<string, IBlob> blobs);

			Assert.AreEqual(21, blobIds.Length);
			Assert.AreEqual(21, blobs.Count);
		}

		private XElement BuildFileElement(string id, string name, string filePath)
		{
			List<XAttribute> attList = new List<XAttribute>()
			{
				new XAttribute("Id", id),
				new XAttribute("Name", name),
				new XAttribute("FilePath", filePath),
			};

			return new XElement("File", attList.ToArray());
		}
	}
}
