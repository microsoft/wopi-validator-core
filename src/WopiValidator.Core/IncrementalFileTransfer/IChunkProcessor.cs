using System.Collections.Generic;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public interface IChunkProcessor
	{
		/// <summary>
		/// Take a file stream and shred it into a set of blobs, applicable to FullFile Chunking.
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="allocator"></param>
		/// <param name="blobIds"> Output of stream shredding: a collection of blobId specifying the order of blobs </param>
		/// <param name="blobs"></param>
		void StreamToBlobs(Stream inputStream, IBlobAllocator allocator, out string[] blobIds, out IReadOnlyDictionary<string, IBlob> blobs);

		/// <summary>
		/// Take a file stream and shred it into a set of blobs, applicable to Zip Chunking.
		/// </summary>
		/// <param name="resourceManager"></param>
		/// <param name="resourceId"></param>
		/// <param name="blobIds"> Output of stream shredding: a collection of blobId specifying the order of blobs </param>
		/// <param name="blobs"></param>
		void StreamToBlobs(IResourceManager resourceManager, string resourceId, out string[] blobIds, out IReadOnlyDictionary<string, IBlob> blobs);
	}
}
