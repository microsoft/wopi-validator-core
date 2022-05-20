using System;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public interface IChunkProcessorFactory
	{
		/// <summary>
		/// Factory providing correct IChunkProcessor
		/// </summary>
		/// <param name="chunkingScheme"></param>
		/// <returns>Fullfile or Zip Chunking processor based on ChunkingScheme input</returns>
		IChunkProcessor CreateInstance(ChunkingScheme chunkingScheme);
	}

	public class ChunkProcessorFactory : IChunkProcessorFactory
	{
		private static readonly Lazy<ChunkProcessorFactory> lazy = new Lazy<ChunkProcessorFactory>(() => new ChunkProcessorFactory());
		public static IChunkProcessorFactory Instance { get { return lazy.Value; } }

		public IChunkProcessor CreateInstance(ChunkingScheme chunkingScheme)
		{
			switch (chunkingScheme)
			{
				case ChunkingScheme.FullFile:
					return new FullFileChunkProcessor();
				case ChunkingScheme.Zip:
					return new ZipChunkProcessor();
				default:
					throw new ArgumentException($"No IChunkProcessor implementation defined for ChunkingScheme: {chunkingScheme}");
			}
		}
	}
}
