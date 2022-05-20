using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Microsoft.Office.WopiValidator.Core.Tests.IncrementalFileTransfer
{
	[TestClass]
	public class JsonDataContractsUnitTests
	{
		[TestMethod]
		public void SerializeGetChunkedFileRequest()
		{
			GetChunkedFileRequestMessage request = new GetChunkedFileRequestMessage();
			request.ContentPropertiesToReturn = new string[] { "prop1", "lastocssave" };
			ContentFilter filter = new ContentFilter();
			filter.ChunkingScheme = ChunkingScheme.FullFile.ToString();
			filter.StreamId = "MainContent";
			filter.ChunksToReturn = ChunksToReturn.All.ToString();
			filter.AlreadyKnownChunks = new string[] { "ChunkId1", "ChunkId2" };
			request.ContentFilters = new ContentFilter[] { filter };

			using (MemoryStream ms = new MemoryStream())
			{
				DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(GetChunkedFileRequestMessage));
				ser.WriteObject(ms, request);
				ms.Seek(0, SeekOrigin.Begin);
				GetChunkedFileRequestMessage requestNew = (GetChunkedFileRequestMessage)ser.ReadObject(ms);

				// Assert
				Assert.AreEqual(2, requestNew.ContentPropertiesToReturn.Length);
				Assert.AreEqual(request.ContentPropertiesToReturn[0], requestNew.ContentPropertiesToReturn[0]);
				Assert.AreEqual(request.ContentPropertiesToReturn[1], requestNew.ContentPropertiesToReturn[1]);

				Assert.AreEqual(request.ToString(), requestNew.ToString());
				Assert.AreEqual(1, requestNew.ContentFilters.Length);
				Assert.AreEqual(request.ContentFilters[0].ChunkingScheme, requestNew.ContentFilters[0].ChunkingScheme);
				Assert.AreEqual(request.ContentFilters[0].StreamId, requestNew.ContentFilters[0].StreamId);
				Assert.AreEqual(request.ContentFilters[0].ChunksToReturn, requestNew.ContentFilters[0].ChunksToReturn);
				Assert.AreEqual(request.ContentFilters[0].AlreadyKnownChunks.Length, requestNew.ContentFilters[0].AlreadyKnownChunks.Length);
				Assert.AreEqual(request.ContentFilters[0].AlreadyKnownChunks[0], requestNew.ContentFilters[0].AlreadyKnownChunks[0]);
				Assert.AreEqual(request.ContentFilters[0].AlreadyKnownChunks[1], requestNew.ContentFilters[0].AlreadyKnownChunks[1]);

			}
		}

		[TestMethod]
		public void SerializeGetChunkedFileResponseMessage()
		{
			GetChunkedFileResponseMessage response = new GetChunkedFileResponseMessage();
			ContentProperty contentProperty1 = new ContentProperty();
			contentProperty1.Name = "Prop1";
			contentProperty1.Value = "TestValue";
			contentProperty1.Retention = ContentPropertyRetention.KeepOnContentChange.ToString();

			ContentProperty contentProperty2 = new ContentProperty();
			contentProperty2.Name = "last ocs save";
			contentProperty2.Value = "TestValue2";
			contentProperty2.Retention = ContentPropertyRetention.DeleteOnContentChange.ToString();

			response.ContentProperties = new ContentProperty[] { contentProperty1, contentProperty2 };

			ChunkSignature chunk1 = new ChunkSignature();
			chunk1.ChunkId = "Chunk1";
			chunk1.Length = 334;

			ChunkSignature chunk2 = new ChunkSignature();
			chunk2.ChunkId = "Chunk2";
			chunk2.Length = 111334;

			ChunkSignature chunk3 = new ChunkSignature();
			chunk3.ChunkId = "Chunk3";
			chunk3.Length = 4;

			ChunkSignature chunk4 = new ChunkSignature();
			chunk4.ChunkId = "Chunk4";
			chunk4.Length = Int64.MaxValue;


			StreamSignature signature = new StreamSignature();
			signature.ChunkingScheme = ChunkingScheme.FullFile.ToString();
			signature.StreamId = "AlternateStream1";
			signature.ChunkSignatures = new ChunkSignature[4] { chunk1, chunk2, chunk3, chunk4 };
			StreamSignature[] streamSignatures = new StreamSignature[1] { signature };

			response.Signatures = streamSignatures;

			using (MemoryStream ms = new MemoryStream())
			{
				DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(GetChunkedFileResponseMessage));
				ser.WriteObject(ms, response);
				ms.Seek(0, SeekOrigin.Begin);
				GetChunkedFileResponseMessage responseNew = (GetChunkedFileResponseMessage)ser.ReadObject(ms);

				// Assert
				Assert.AreEqual(response.ToString(), responseNew.ToString());

			}
		}

		[TestMethod]
		public void SerializePutChunkedFileRequestMessage()
		{
			PutChunkedFileRequestMessage request = new PutChunkedFileRequestMessage();
			ContentProperty contentProperty1 = new ContentProperty();
			contentProperty1.Name = "Prop1";
			contentProperty1.Value = "TestValue";
			contentProperty1.Retention = ContentPropertyRetention.KeepOnContentChange.ToString();

			ContentProperty contentProperty2 = new ContentProperty();
			contentProperty2.Name = "last ocs save";
			contentProperty2.Value = "TestValue2";
			contentProperty2.Retention = ContentPropertyRetention.DeleteOnContentChange.ToString();

			request.ContentProperties = new ContentProperty[] { contentProperty1, contentProperty2 };

			ChunkSignature chunk1 = new ChunkSignature();
			chunk1.ChunkId = "Chunk1";
			chunk1.Length = 334;

			ChunkSignature chunk2 = new ChunkSignature();
			chunk2.ChunkId = "Chunk2";
			chunk2.Length = 111334;

			ChunkSignature chunk3 = new ChunkSignature();
			chunk3.ChunkId = "Chunk3";
			chunk3.Length = 4;

			ChunkSignature chunk4 = new ChunkSignature();
			chunk4.ChunkId = "Chunk4";
			chunk4.Length = 554;


			StreamSignature signature = new StreamSignature();
			signature.ChunkingScheme = ChunkingScheme.FullFile.ToString();
			signature.StreamId = "AlternateStream1";
			signature.ChunkSignatures = new ChunkSignature[4] { chunk1, chunk2, chunk3, chunk4 };
			StreamSignature[] streamSignatures = new StreamSignature[1] { signature };

			request.Signatures = streamSignatures;

			request.UploadSessionTokenToCommit = "3DFCF2D5 - DABA - 4BDD - 88EC - 69823EDCF585";

			using (MemoryStream ms = new MemoryStream())
			{
				DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(PutChunkedFileRequestMessage));
				ser.WriteObject(ms, request);
				ms.Seek(0, SeekOrigin.Begin);
				PutChunkedFileRequestMessage requestNew = (PutChunkedFileRequestMessage)ser.ReadObject(ms);

				// Assert
				Assert.AreEqual(request.ToString(), requestNew.ToString());
			}
		}
	}
}
