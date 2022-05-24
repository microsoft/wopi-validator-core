using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core.Tests.IncrementalFileTransfer
{
	[TestClass]
	public class FrameUnitTests
	{
		[TestMethod]
		public void FrameHeader_ReadWriteFrameHeader_SucceedOnEmptyExtendedHeader()
		{
			// Arrange
			FrameHeader frameHeader = new FrameHeader(FrameType.MessageJSON, payloadSize: 20, extendedHeaderSize: 0, extendedHeader: null);
			using (MemoryStream memoryStream = new MemoryStream())
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				// Action
				FrameHeader.WriteFrameHeader(frameHeader, binaryWriter);
				binaryWriter.Flush();
				binaryWriter.Seek(0, SeekOrigin.Begin);

				FrameHeader parsedFrameHeader = FrameHeader.ReadFrameHeader(memoryStream);

				// Assert
				Assert.AreEqual(FrameType.MessageJSON, parsedFrameHeader.Type);
				Assert.AreEqual((ulong)20, parsedFrameHeader.PayloadSize);
				Assert.AreEqual((uint)0, parsedFrameHeader.ExtendedHeaderSize);
				Assert.IsNull(parsedFrameHeader.ExtendedHeader);
			}
		}

		[TestMethod]
		public void FrameHeader_ReadWriteFrameHeader_SucceedOnNonEmptyExtendedHeader()
		{
			// Arrange
			byte[] extendedHeader = new byte[3] { 0, 1, 2 };
			FrameHeader frameHeader = new FrameHeader(FrameType.MessageJSON, payloadSize: 20, extendedHeaderSize:3, extendedHeader:extendedHeader);
			using (MemoryStream memoryStream = new MemoryStream())
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				// Action
				FrameHeader.WriteFrameHeader(frameHeader, binaryWriter);
				binaryWriter.Flush();
				binaryWriter.Seek(0, SeekOrigin.Begin);

				FrameHeader parsedFrameHeader = FrameHeader.ReadFrameHeader(memoryStream);

				// Assert
				Assert.AreEqual(FrameType.MessageJSON, parsedFrameHeader.Type);
				Assert.AreEqual((ulong)20, parsedFrameHeader.PayloadSize);
				Assert.AreEqual((uint)3, parsedFrameHeader.ExtendedHeaderSize);
				CollectionAssert.AreEqual(frameHeader.ExtendedHeader, parsedFrameHeader.ExtendedHeader);
			}
		}
	}
}
