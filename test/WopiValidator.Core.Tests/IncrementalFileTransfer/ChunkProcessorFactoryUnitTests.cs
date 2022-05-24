using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Office.WopiValidator.Core.Tests.IncrementalFileTransfer
{
	[TestClass]
	public class ChunkProcessorFactoryUnitTests
	{
		[TestMethod]
		public void ChunkProcessorFactory_CreateInstance_ReturnFullFileChunkingProcessor()
		{
			// Act
			IChunkProcessor chunkProcessor = ChunkProcessorFactory.Instance.CreateInstance(ChunkingScheme.FullFile);

			// Assert
			Type chunkProcessorType = chunkProcessor.GetType();
			Assert.IsNotNull(chunkProcessor);
			Assert.AreEqual(typeof(FullFileChunkProcessor), chunkProcessorType);
		}
	}
}
