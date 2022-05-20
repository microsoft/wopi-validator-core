using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Tests.IncrementalFileTransfer
{
	[TestClass]
	public class FullFileChunkProcessorUnitTests
	{
		[TestMethod]
		public void FullFileChunkProcessor_StreamToBlobs_SucceedOnNonEmptyStream()
		{
			// Arrange
			string alreadyExistingContent = "SomeRandomText";
			byte[] bytes = Encoding.UTF8.GetBytes(alreadyExistingContent);

			SpookyHash spookyHash = new SpookyHash();
			string hashString = spookyHash.CalculateHash(bytes);

			using (MemoryStream ms = new MemoryStream(bytes))
			{
				string[] blobIds;
				IReadOnlyDictionary<string, IBlob> blobs = new Dictionary<string, IBlob>();
				IChunkProcessor chunkProcessor = CreateFullFileChunkProcessor();
				IBlobAllocator allocator = CreateBlobAllocator();

				// Act
				chunkProcessor.StreamToBlobs(ms, allocator, out blobIds, out blobs);

				// Assert
				Assert.AreEqual(1, blobIds.Length);
				Assert.AreEqual(1, blobs.Count);
				Assert.AreEqual((ulong)bytes.Length, blobs[blobIds[0]].Length);
				Assert.AreEqual(hashString, blobs[blobIds[0]].BlobId);
			}
		}

		[TestMethod]
		public void FullFileChunkProcessor_StreamToBlobs_SuccessOnEmptyStream()
		{
			// Arrange
			using (MemoryStream ms = new MemoryStream(new byte[0]))
			{
				string[] blobIds;
				IReadOnlyDictionary<string, IBlob> blobs = new Dictionary<string, IBlob>();
				IChunkProcessor chunkProcessor = CreateFullFileChunkProcessor();
				IBlobAllocator allocator = CreateBlobAllocator();

				// Act
				chunkProcessor.StreamToBlobs(ms, allocator, out blobIds, out blobs);

				// Assert
				Assert.AreEqual(0, blobIds.Length);
				Assert.AreEqual(0, blobs.Count);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void FullFileChunkProcessor_StreamToBlobs_ThrowOnNullStream()
		{
			// Arrange
			Stream stream = null;
			string[] blobIds;
			IReadOnlyDictionary<string, IBlob> blobs = new Dictionary<string, IBlob>();
			IChunkProcessor chunkProcessor = CreateFullFileChunkProcessor();
			IBlobAllocator allocator = CreateBlobAllocator();

			// Act
			chunkProcessor.StreamToBlobs(stream, allocator, out blobIds, out blobs);
		}

		private IChunkProcessor CreateFullFileChunkProcessor()
		{
			IChunkProcessor chunkProcessor = ChunkProcessorFactory.Instance.CreateInstance(ChunkingScheme.FullFile);
			return chunkProcessor;
		}

		private IBlobAllocator CreateBlobAllocator()
		{
			IBlobAllocator allocator = new BlobAllocator();
			return allocator;
		}
	}
}
