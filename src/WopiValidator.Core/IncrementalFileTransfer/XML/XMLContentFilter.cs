using System;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public class XMLContentFilter
	{
		public ChunkingScheme ChunkingScheme { get; private set; }
		public string StreamId { get; private set; }
		public ChunksToReturn ChunksToReturn { get; private set; }
		public string AlreadyExistingContent { get; private set; }
		public string AlreadyExistingContentResourceId { get; private set; }

		public XMLContentFilter(
			string ChunkingScheme,
			string StreamId,
			string ChunksToReturn,
			string AlreadyExistingContent,
			string AlreadyExistingContentResourceId)
		{
			ValidateAndParseParameters(
				ChunkingScheme,
				ChunksToReturn,
				StreamId,
				AlreadyExistingContent,
				AlreadyExistingContentResourceId,
				out ChunkingScheme ParsedChunkingScheme,
				out ChunksToReturn ParsedChunksToReturn);

			this.ChunkingScheme = ParsedChunkingScheme;
			this.StreamId = StreamId;
			this.ChunksToReturn = ParsedChunksToReturn;
			this.AlreadyExistingContent = AlreadyExistingContent;
			this.AlreadyExistingContentResourceId = AlreadyExistingContentResourceId;
		}

		private void ValidateAndParseParameters(
			string ChunkingSchemeInString,
			string ChunksToReturnInString,
			string StreamId,
			string AlreadyExistingContent,
			string AlreadyExistingContentResourceId,
			out ChunkingScheme OutputParsedChunkingScheme,
			out ChunksToReturn OutputParsedChunksToReturn)
		{
			if (String.IsNullOrWhiteSpace(ChunkingSchemeInString))
				throw new ArgumentException($"GetChunkedFileContentFilter '{nameof(ChunkingSchemeInString)}' parameter is invalid.");

			if (!Enum.TryParse(ChunkingSchemeInString, true, out ChunkingScheme ParsedChunkingScheme))
				throw new ArgumentException($"GetChunkedFileContentFilter '{nameof(ChunkingSchemeInString)}' parameter can not be parsed to enum.");

			if (String.IsNullOrWhiteSpace(ChunksToReturnInString))
				throw new ArgumentException($"GetChunkedFileContentFilter '{nameof(ChunksToReturnInString)}' parameter is invalid.");

			if (!Enum.TryParse(ChunksToReturnInString, true, out ChunksToReturn ParsedChunksToReturn))
				throw new ArgumentException($"GetChunkedFileContentFilter '{nameof(ChunksToReturnInString)}' parameter is invalid.");

			if (String.IsNullOrWhiteSpace(StreamId))
				throw new ArgumentException($"GetChunkedFileContentFilter '{nameof(StreamId)}' parameter is invalid.");

			OutputParsedChunkingScheme = ParsedChunkingScheme;
			OutputParsedChunksToReturn = ParsedChunksToReturn;

			if (ParsedChunkingScheme == ChunkingScheme.FullFile)
			{
				if (AlreadyExistingContent == null)
					throw new ArgumentException($"GetChunkedFileContentFilter '{nameof(AlreadyExistingContent)}' parameter is invalid.");

				if (AlreadyExistingContentResourceId != null)
					throw new ArgumentException($"GetChunkedFileContentFilter '{nameof(AlreadyExistingContentResourceId)}' parameter shouldn't be populated.");
			}

			if (ParsedChunkingScheme == ChunkingScheme.Zip)
			{
				if (AlreadyExistingContentResourceId == null)
					throw new ArgumentException($"GetChunkedFileContentFilter '{nameof(AlreadyExistingContentResourceId)}' parameter is invalid.");

				if (AlreadyExistingContent != null)
					throw new ArgumentException($"GetChunkedFileContentFilter '{nameof(AlreadyExistingContent)}' parameter shouldn't be populated.");
			}
		}
	}
}
