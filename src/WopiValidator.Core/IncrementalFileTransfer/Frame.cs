using System;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	// The following class signatures are compliant with: wac\src\common\Server\WopiFrame.cs
	public enum FrameType
	{
		Undefined = 0,

		// EndFrame must be the last frame in payload. The data after EndFrame frame is ignored
		EndFrame = 1,

		// MessageFrameJSON must be the first frame.
		// It contains request or response body structured data serialized via JSON.
		MessageJSON = 2,

		// Chunk contains the full binary content for a given chunk.
		// ChunkId is stored in the extended header field and data bytes are stored in the frame payload.
		Chunk = 3,

		// ChunkRange contains the partial binary content for a given chunk.
		// ChunkId, offset, length and flags are stored in the extended header field.
		// Chunk data bytes are stored in the frame payload.
		ChunkRange = 4

	}

	/// <summary>
	/// A class to capture the essential fields to read or write a wopi frame object.
	/// Since Wopi is an external protocol, we make sure primitive data types are read and written in BigEndian order.
	/// </summary>
	public class FrameHeader
	{
		private const int FrameHeaderLength = Constants.FrameHeaderConstants.FrameHeaderLengthInBytes;

		public FrameType Type { get; }
		public uint ExtendedHeaderSize { get; }
		public ulong PayloadSize { get; }
		public byte[] ExtendedHeader { get; }

		public FrameHeader(
			FrameType type,
			uint extendedHeaderSize,
			ulong payloadSize,
			byte[] extendedHeader)
		{
			Type = type;
			ExtendedHeaderSize = extendedHeaderSize;
			PayloadSize = payloadSize;
			ExtendedHeader = extendedHeader;
		}

		/// <summary>
		/// Returns the size of the frame in bytes
		/// </summary>
		public ulong GetFrameSize()
		{
			return ExtendedHeaderSize + PayloadSize + FrameHeaderLength;
		}

		/// <summary>
		/// Read header (including extended header)
		/// Make sure primitive data types are read in BigEndian order.
		/// </summary>
		public static FrameHeader ReadFrameHeader(Stream inputStream)
		{
			var buffer = new byte[FrameHeaderLength];
			FrameProtocolStreamUtils.ReadChunk(inputStream, buffer, 0, FrameHeaderLength);

			// Read (uint)FrameType
			int offset = 0;
			EnsureBigEndianOrder(buffer, offset, length: sizeof(uint));
			uint frameTypeValue = BitConverter.ToUInt32(buffer, offset);
			offset += sizeof(uint);

			FrameType frameType = (FrameType)frameTypeValue;
			if (frameType == FrameType.Undefined)
			{
				throw new ArgumentException("ReadFrameHeader encountered an undefined frametype.");
			}

			// Read (uint)ExtendedHeaderSize
			// EndFrame should still have the 16 byte header set appropriately.
			EnsureBigEndianOrder(buffer, offset, length: sizeof(uint));
			uint extendedHeaderSize = BitConverter.ToUInt32(buffer, offset);
			offset += sizeof(uint);

			// Read (ulong)PayloadSize
			EnsureBigEndianOrder(buffer, offset, length: sizeof(ulong));
			ulong payloadSize = BitConverter.ToUInt64(buffer, offset);

			if (extendedHeaderSize == 0)
			{
				return new FrameHeader(frameType, 0, payloadSize, null);
			}

			// Read byte[] ExtendedHeader
			var extendedHeader = new byte[extendedHeaderSize];
			FrameProtocolStreamUtils.ReadChunk(inputStream, extendedHeader, 0, extendedHeader.Length);

			return new FrameHeader(frameType, extendedHeaderSize, payloadSize, extendedHeader);
		}

		/// <summary>
		/// Writes out the Frame object to the binary writer.
		/// Make sure primitive data types are written in BigEndian order.
		/// </summary>
		public static void WriteFrameHeader(FrameHeader header, BinaryWriter writer)
		{
			var bytes = BitConverter.GetBytes((int)header.Type);
			EnsureBigEndianOrder(bytes, 0, bytes.Length);
			writer.Write(bytes);

			bytes = BitConverter.GetBytes(header.ExtendedHeaderSize);
			EnsureBigEndianOrder(bytes, 0, bytes.Length);
			writer.Write(bytes);

			bytes = BitConverter.GetBytes(header.PayloadSize);
			EnsureBigEndianOrder(bytes, 0, bytes.Length);
			writer.Write(bytes);

			if (header.ExtendedHeaderSize != 0)
			{
				writer.Write(header.ExtendedHeader);
			}
		}

		private static void EnsureBigEndianOrder(byte[] buffer, int offset, int length)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(buffer, offset, length);
			}
		}
	}

	/// <summary>
	/// Frame is generated when parsing request or response stream.
	/// Frame can be different frame type, ExtendedHeader is null for EndFrame and MessageFrameJSON type.
	/// Payload is a stream containing the frame content.
	/// </summary>
	public class Frame
	{
		public Frame(FrameType type, byte[] payload, byte[] extendedHeader)
		{
			Type = type;
			Payload = payload;
			ExtendedHeader = extendedHeader;
		}

		public FrameType Type { get; private set; }
		public byte[] ExtendedHeader { get; private set; }
		public byte[] Payload { get; private set; }
	}
}

