using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core.Tests.IncrementalFileTransfer
{
	[TestClass]
	public class MemoryBlobUnitTests
	{
		[TestMethod]
		public void CreateMemoryBlob_Succeed()
		{
			// Arrange
			byte[] bytes = new byte[100000];
			Random random = new Random((int)DateTime.Now.Millisecond);
			random.NextBytes(bytes);
			MemoryStream ms = new MemoryStream(bytes);
			SpookyHash spookyHash = new SpookyHash();

			// Act
			MemoryBlob memoryBlob = new MemoryBlob(ms);

			// Assert
			Assert.IsNotNull(memoryBlob);
			Assert.AreEqual((ulong)bytes.Length, memoryBlob.Length);
			Assert.AreEqual(spookyHash.CalculateHash(ms), memoryBlob.BlobId);
			ms.Dispose();
		}

		[TestMethod]
		public void MemoryBlob_ToArray_Succeed()
		{
			// Arrange
			byte[] bytes = new byte[100];
			Random random = new Random((int)DateTime.Now.Millisecond);
			random.NextBytes(bytes);
			MemoryStream ms = new MemoryStream(bytes);

			// Act
			MemoryBlob memoryBlob = new MemoryBlob(ms);
			byte[] bytesRead = memoryBlob.ToArray();

			// Assert
			Assert.AreEqual((ulong)bytes.Length, memoryBlob.Length);
			CollectionAssert.AreEqual(bytes, bytesRead);
			ms.Dispose();
		}

		[TestMethod]
		public void MemoryBlob_GetStream_Succeed()
		{
			// Arrange
			byte[] bytes = new byte[1000];
			Random random = new Random((int)DateTime.Now.Millisecond);
			random.NextBytes(bytes);
			MemoryStream ms = new MemoryStream(bytes);
			SpookyHash spookyHash = new SpookyHash();

			// Act
			MemoryBlob memoryBlob = new MemoryBlob(ms);
			Stream stream = memoryBlob.GetStream();

			// Assert
			Assert.IsNotNull(stream);
			Assert.AreEqual(spookyHash.CalculateHash(bytes), spookyHash.CalculateHash(stream));
			stream.Dispose();
			ms.Dispose();
		}
	}
}
