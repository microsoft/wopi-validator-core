using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public class ZipChunkProcessor : IChunkProcessor
	{
		/// <summary>
		/// Zip Chunking: Take a file stream and shred it into a set of blobs. 
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="allocator"></param>
		/// <param name="blobIds">Output of stream shredding: a collection of blobId specifying the order of blobs </param>
		/// <param name="blobs"></param>
		public void StreamToBlobs(Stream inputStream, IBlobAllocator allocator, out string[] blobIds, out IReadOnlyDictionary<string, IBlob> blobs)
		{
			throw new NotImplementedException();
		}

		public void StreamToBlobs(IResourceManager resourceManager, string resourceId, out string[] blobIds, out IReadOnlyDictionary<string, IBlob> blobs)
		{
			resourceManager.GetZipChunkingBlobs(resourceId, out blobIds, out blobs);
		}
	}
}
