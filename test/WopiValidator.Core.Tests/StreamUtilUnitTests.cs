using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Office.WopiValidator.Core.Factories;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Office.WopiValidator.Core.Tests
{
	[TestClass]
	public class StreamUtilUnitTests
	{
		[TestMethod]
		public void CompareNullWithNull()
		{
			Assert.IsTrue(StreamUtil.StreamEquals(null, null));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CompareNullWithNonNull()
		{
			Assert.IsFalse(StreamUtil.StreamEquals(null, new MemoryStream(new byte[0])));
		}

		[TestMethod]
		public void CompareDifferentLength()
		{
			Assert.IsFalse(StreamUtil.StreamEquals(new MemoryStream(new byte[ 0 ]), new MemoryStream(new byte[ 1 ])));
		}

		[TestMethod]
		public void CompareSameLengthDifferentContent()
		{
			Assert.IsFalse(StreamUtil.StreamEquals(new MemoryStream(new byte[] { 0, 1, 2} ), new MemoryStream(new byte[] { 0, 1, 3})));
		}
	}
}
