using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Tests.IncrementalFileTransfer
{
	[TestClass]
	public class FrameProtocolUnitTests
	{
		[TestMethod]
		public void FrameProtocol_RoundTripOfOneMessageFrame_Success()
		{
			// Arrange
			GetChunkedFileRequestMessage request = CreateGetChunkedFileRequestMessage();

			// Act
			// Build a request stream from GetChunkedFile message frame
			FrameProtocolBuilder builder = new FrameProtocolBuilder();
			builder.AddFrame(request);
			var inputStream = builder.CreateStream();

			// Parse the request stream into WopiFrame
			List<Frame> frameList = FrameProtocolParser.ParseStream(inputStream);

			// Assert
			Assert.AreEqual(1, frameList.Count);
			Assert.IsNotNull(frameList[0].Payload);
			Assert.IsNull(frameList[0].ExtendedHeader);
			GetChunkedFileRequestMessage requestDecoded = JsonMessageSerializer.Instance.DeSerialize<GetChunkedFileRequestMessage>(new MemoryStream(frameList[0].Payload));
			Assert.AreEqual(request.ToString(), requestDecoded.ToString());
		}

		[TestMethod]
		public void FrameProtocol_RoundTripOfMultiFrames_Success()
		{
			// Arrange
			GetChunkedFileRequestMessage request = CreateGetChunkedFileRequestMessage();

			// Act
			// Build a request stream from a message frame and a chunk frame
			var builder = new FrameProtocolBuilder();
			IBlob blob1 = new MemoryBlob(Encoding.UTF8.GetBytes("Blob1"));
			IBlob blob2 = new MemoryBlob(Encoding.UTF8.GetBytes("Blob2"));
			builder.AddFrame(request);
			builder.AddFrame(blob1);
			builder.AddFrame(blob2);
			var inputStream = builder.CreateStream();

			// Parse the request stream into multiple WopiFrame
			List<Frame> frameList = FrameProtocolParser.ParseStream(inputStream);

			// Assert
			Assert.AreEqual(3, frameList.Count);
			Assert.IsNotNull(frameList[0].Payload);
			Assert.IsNull(frameList[0].ExtendedHeader);
			Assert.IsNotNull(frameList[1].Payload);
			Assert.IsNotNull(frameList[1].ExtendedHeader);
			Assert.AreEqual(blob1.BlobId, System.Convert.ToBase64String(frameList[1].ExtendedHeader));
			var spookyHash = new SpookyHash();
			Assert.AreEqual(blob1.BlobId, spookyHash.CalculateHash(frameList[1].Payload));
			Assert.IsNotNull(frameList[2].Payload);
			Assert.IsNotNull(frameList[2].ExtendedHeader);
			Assert.AreEqual(blob2.BlobId, System.Convert.ToBase64String(frameList[2].ExtendedHeader));
			Assert.AreEqual(blob2.BlobId, spookyHash.CalculateHash(new MemoryStream(frameList[2].Payload)));
		}

		private GetChunkedFileRequestMessage CreateGetChunkedFileRequestMessage()
		{
			GetChunkedFileRequestMessage request = new GetChunkedFileRequestMessage();
			request.ContentPropertiesToReturn = new string[] { "prop1", "lastocssave" };
			ContentFilter filter = new ContentFilter();
			filter.ChunkingScheme = ChunkingScheme.FullFile.ToString();
			filter.StreamId = "MainContent";
			filter.ChunksToReturn = ChunksToReturn.All.ToString();
			filter.AlreadyKnownChunks = new string[] { "ChunkId1" };
			request.ContentFilters = new ContentFilter[] { filter };

			return request;
		}
	}
}
