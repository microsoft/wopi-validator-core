using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Office.WopiValidator.Core.Factories;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Office.WopiValidator.Core.Tests.IncrementalFileTransfer
{
	[TestClass]
	public class BlobAllocatorUnitTests
	{
		[TestMethod]
		public void CreateBlobWithInputBytes()
		{
			byte[] bytes = { 0, 1, 2, 3, 4 };
			SpookyHash spookyHash = new SpookyHash();

			BlobAllocator allocator = new BlobAllocator();
			IBlob blob = allocator.CreateBlob(bytes);

			Assert.AreEqual((ulong)5, blob.Length);
			Assert.AreEqual(spookyHash.CalculateHash(bytes), blob.BlobId);
		}

		[TestMethod]
		public void CreateBlobWithInputStream()
		{
			byte[] bytes = { 0, 1, 2, 3, 4 };
			SpookyHash spookyHash = new SpookyHash();

			using (MemoryStream ms = new MemoryStream(bytes))
			{
				BlobAllocator allocator = new BlobAllocator();
				IBlob blob = allocator.CreateBlob(ms);

				Assert.AreEqual((ulong)5, blob.Length);
				Assert.AreEqual(spookyHash.CalculateHash(bytes), blob.BlobId);
			}
		}

		[TestMethod]
		public void CreateBlobWithInputSubStream()
		{
			byte[] bytes = { 0, 1, 2, 3, 4 };
			SpookyHash spookyHash = new SpookyHash();

			using (MemoryStream ms = new MemoryStream(bytes))
			{
				BlobAllocator allocator = new BlobAllocator();
				IBlob blob = allocator.CreateBlob(ms, 4);

				Assert.AreEqual((ulong)4, blob.Length);
				Assert.AreEqual(spookyHash.CalculateHash(new byte[] { 0, 1, 2, 3 }), blob.BlobId);
			}
		}
	}
}
