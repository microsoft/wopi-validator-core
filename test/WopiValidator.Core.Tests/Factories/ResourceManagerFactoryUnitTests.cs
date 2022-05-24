using Microsoft.Office.WopiValidator.Core.Factories;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core.Tests.Factories
{
	[TestClass]
	public class ResourceManagerFactoryUnitTests
	{
		//<Resources>
		//	<File Id = "WordBlankDocument" Name="WordBlankDocument.docx" FilePath="WordBlankDocument.docx" />
		//</Resources>
		[TestMethod]
		public void SingleNonEmptyFileElement()
		{
			XElement definition = new XElement("Resources",
				BuildFileElement(id: "WordBlankDocument", name: "WordBlankDocument.docx", filePath: "WordBlankDocument.docx")
			);

			IResourceManager resourceManager = new ResourceManagerFactory().GetResourceManager(definition);
			SpookyHash spookyHash = new SpookyHash();

			using (MemoryStream ms = new MemoryStream())
			using (StreamWriter sw = new StreamWriter(ms))
			{
				sw.Write("WordBlankDocument"+ "WordBlankDocument.docx"+ "WordBlankDocument.docx");
				sw.Flush();
				Assert.AreEqual(resourceManager.GetFileName("WordBlankDocument"), "WordBlankDocument.docx");
				Assert.AreEqual(spookyHash.CalculateHash(resourceManager.GetContentStream("WordBlankDocument")), spookyHash.CalculateHash(ms));
			}
		}

		//<Resources>
		//  <File Id = "ZeroByteFile" Name="ZeroByteFile.wopitest" FilePath="ZeroByteFile.wopitest" />
		//</Resources>
		[TestMethod]
		public void SingleEmptyFileElement()
		{
			XElement definition = new XElement("Resources",
				BuildFileElement(id: "ZeroByteFile", name: "ZeroByteFile.wopitest", filePath: "ZeroByteFile.wopitest")
			);

			IResourceManager resourceManager = new ResourceManagerFactory().GetResourceManager(definition);
			SpookyHash spookyHash = new SpookyHash();

			using (MemoryStream ms = new MemoryStream())
			using (StreamWriter sw = new StreamWriter(ms))
			{
				sw.Write(String.Empty);
				sw.Flush();
				Assert.AreEqual(resourceManager.GetFileName("ZeroByteFile"), "ZeroByteFile.wopitest");
				Assert.AreEqual(spookyHash.CalculateHash(resourceManager.GetContentStream("ZeroByteFile")), spookyHash.CalculateHash(ms));
			}
		}

		//<Resources>
		//  <File Id = "NonZeroByteExcel_Main_V1" Name="ChunkIds.txt" FilePath="Microsoft.Office.WopiValidator.Core.Resources.ZipChunking.NonZeroByteExcel.Version1.MainContent." />
		//</Resources>
		[TestMethod]
		public void SingleZipFileElement()
		{
			XElement definition = new XElement("Resources",
				BuildFileElement(id: "NonZeroByteExcel_Main_V1", name: "ChunkIds.txt", filePath: "Microsoft.Office.WopiValidator.Core.Resources.ZipChunking.NonZeroByteExcel.Version1.MainContent.")
			);

			IResourceManager resourceManager = new ResourceManagerFactory().GetResourceManager(definition);
			resourceManager.GetZipChunkingBlobs("NonZeroByteExcel_Main_V1", out string[] blobIds, out IReadOnlyDictionary<string, IBlob> blobs);

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
