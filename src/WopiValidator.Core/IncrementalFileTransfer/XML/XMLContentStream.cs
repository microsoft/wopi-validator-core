using System;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public class XMLContentStream
	{
		public ChunkingScheme ChunkingScheme { get; private set; }
		public string StreamId { get; private set; }
		public string NewContent { get; private set; }
		public string LastKnownHostContent { get; private set; }
		public string NewContentResourceId { get; private set; }
		public string LastKnownHostContentResourceId { get; private set; }

		public XMLContentStream(
			string ChunkingScheme,
			string StreamId,
			string NewContent,
			string LastKnownHostContent,
			string NewContentResourceId,
			string LastKnownHostContentResourceId)
		{
			ValidateAndParseParameters(
				ChunkingScheme,
				StreamId,
				NewContent,
				LastKnownHostContent,
				NewContentResourceId,
				LastKnownHostContentResourceId,
				out ChunkingScheme ParsedChunkingScheme);

			this.StreamId = StreamId;
			this.NewContent = NewContent;
			this.LastKnownHostContent = LastKnownHostContent;
			this.NewContentResourceId = NewContentResourceId;
			this.LastKnownHostContentResourceId = LastKnownHostContentResourceId;
			this.ChunkingScheme = ParsedChunkingScheme;
		}

		private void ValidateAndParseParameters(
			string ChunkingSchemeInString,
			string StreamId,
			string NewContent,
			string LastKnownHostContent,
			string NewContentResourceId,
			string LastKnownHostContentResourceId,
			out ChunkingScheme OutputParsedChunkingScheme)
		{
			if (String.IsNullOrWhiteSpace(ChunkingSchemeInString))
				throw new ArgumentException($"PutChunkedFileStream '${nameof(ChunkingSchemeInString)}' parameter is invalid.");

			if (!Enum.TryParse(ChunkingSchemeInString, true, out ChunkingScheme ParsedChunkingScheme))
				throw new ArgumentException($"PutChunkedFileStream '{nameof(ChunkingSchemeInString)}' parameter can not be parsed to enum.");

			if (String.IsNullOrWhiteSpace(StreamId))
				throw new ArgumentException($"PutChunkedFileStream '{nameof(StreamId)}' parameter is invalid.");

			OutputParsedChunkingScheme = ParsedChunkingScheme;

			if (ParsedChunkingScheme == ChunkingScheme.FullFile)
			{
				if (NewContent == null)
					throw new ArgumentException($"PutChunkedFileStream '{nameof(NewContent)}' parameter is invalid.");

				if (LastKnownHostContent == null)
					throw new ArgumentException($"PutChunkedFileStream '{nameof(LastKnownHostContent)}' parameter is invalid.");

				if (NewContentResourceId != null)
					throw new ArgumentException($"PutChunkedFileStream '{nameof(NewContentResourceId)}' parameter shouldn't be populated.");

				if (LastKnownHostContentResourceId != null)
					throw new ArgumentException($"PutChunkedFileStream '{nameof(LastKnownHostContentResourceId)}' parameter shouldn't be populated.");
			}

			if (ParsedChunkingScheme == ChunkingScheme.Zip)
			{
				if (NewContentResourceId == null)
					throw new ArgumentException($"PutChunkedFileStream '{nameof(NewContentResourceId)}' parameter is invalid.");

				if (LastKnownHostContentResourceId == null)
					throw new ArgumentException($"PutChunkedFileStream '{nameof(LastKnownHostContentResourceId)}' parameter is invalid.");

				if (NewContent != null)
					throw new ArgumentException($"PutChunkedFileStream '{nameof(NewContent)}' parameter shouldn't be populated.");

				if (LastKnownHostContent != null)
					throw new ArgumentException($"PutChunkedFileStream '{nameof(LastKnownHostContent)}' parameter shouldn't be populated.");
			}
		}
	}
}
