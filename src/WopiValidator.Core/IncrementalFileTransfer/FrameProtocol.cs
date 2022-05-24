using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	/// <summary>
	/// Provides a way for caller to build wopi request stream as sequence of frames.
	/// A caller should call one or more AddFrame call followed by CreateSteam.
	/// It is responsibility of caller to dispose result Stream.
	/// </summary>
	public class FrameProtocolBuilder
	{
		private struct InputFrame
		{
			public FrameHeader Header;
			public Stream PayloadStream;
		}

		private List<InputFrame> _inputFrames = new List<InputFrame>();

		public int FrameCount => _inputFrames.Count;

		/// <summary>
		/// The size in bytes for all frames being built in the FrameProtocolBuilder
		/// </summary>
		public ulong FrameSizeInBytes
		{
			get
			{
				ulong frameSizeInBytes = 0;
				foreach (var frame in _inputFrames)
				{
					frameSizeInBytes += frame.Header.GetFrameSize();
				}

				return frameSizeInBytes;
			}
		}

		/// <summary>
		/// Add Chunk Frame.
		/// ExtendedHeader is the 128-bit spookyHash of the chunk payload.
		/// </summary>
		/// <param name="blob"></param>
		public void AddFrame(IBlob blob)
		{
			byte[] extendedHeader = System.Convert.FromBase64String(blob.BlobId);
			_inputFrames.Add(new InputFrame()
			{
				Header = new FrameHeader(FrameType.Chunk, (uint)extendedHeader.Length, (ulong)blob.Length, extendedHeader),
				PayloadStream = blob.GetStream()
			});
		}

		/// <summary>
		/// Add Message Frame. Message object will be serialized to a memory stream.
		/// ExtendedHeader is null for message frame.
		/// </summary>
		/// <param name="message"></param>
		public void AddFrame<TMessagePayload>(TMessagePayload message)
		{
			Stream messageStream = JsonMessageSerializer.Instance.Serialize(message);
			_inputFrames.Add(new InputFrame()
			{
				Header = new FrameHeader(FrameType.MessageJSON, 0, (ulong)messageStream.Length, null),
				PayloadStream = messageStream
			});
		}

		/// <summary>
		/// Returns stream which includes all frames data and headers with EndFrame at the end.
		/// Note: caller is responsible for Disposing stream
		/// </summary>
		public Stream CreateStream()
		{
			if (_inputFrames.Count == 0)
				throw new ArgumentException("Wopi Protocol: Cannot create request stream with empty inputFrame list.");

			// Add EndFrame at the end so we do not have to special case last frame
			_inputFrames.Add(new InputFrame()
			{
				Header = new FrameHeader(FrameType.EndFrame, 0, 0, null)
			});

			MemoryStream outputStream = new MemoryStream();
			using (BinaryWriter binaryWriter = new BinaryWriter(output:outputStream, encoding:Encoding.UTF8, leaveOpen:true))
			{
				foreach (InputFrame frame in _inputFrames)
				{
					FrameHeader frameHeader = frame.Header;
					Stream payload = frame.PayloadStream;

					// Write frame header to stream
					FrameHeader.WriteFrameHeader(frameHeader, binaryWriter);

					// Write frame payload to stream
					if (frame.PayloadStream != null)
					{
						frame.PayloadStream.Seek(0, SeekOrigin.Begin);
						using (MemoryStream ms = new MemoryStream())
						{
							frame.PayloadStream.CopyTo(ms);
							binaryWriter.Write(ms.ToArray());
						}
					}
				}
			}

			outputStream.Seek(0, SeekOrigin.Begin);
			return outputStream;
		}
	}

	/// <summary>
	/// Provides a way for caller to build a sequence of Frame from response stream.
	/// EndFrame is not included in the returned list.
	/// </summary>
	public class FrameProtocolParser
	{
		/// <summary>
		/// parses input stream and returns list of Frame (not including EndFrame)
		/// <param name="inputStream">the input stream over the wire</param>
		/// </summary>
		public static List<Frame> ParseStream(Stream inputStream)
		{
			List<Frame> frames = new List<Frame>();

			for (; ;)
			{
				FrameHeader frameHeader = FrameHeader.ReadFrameHeader(inputStream);

				// We do not include EndFrame in the result
				if (frameHeader.Type == FrameType.EndFrame)
				{
					break;
				}

				byte[] payloadBuffer = new byte[frameHeader.PayloadSize];
				FrameProtocolStreamUtils.ReadChunk(inputStream, payloadBuffer, 0, payloadBuffer.Length);

				frames.Add(new Frame(
					type: frameHeader.Type,
					extendedHeader: frameHeader.ExtendedHeader,
					payload: payloadBuffer));
			}

			return frames;
		}
	}
}
