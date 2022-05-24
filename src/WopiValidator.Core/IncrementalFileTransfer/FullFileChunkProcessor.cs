using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public class FullFileChunkProcessor : IChunkProcessor
	{
		/// <summary>
		/// Take a file stream and shred it into a set of blobs.
		/// FullFile chunking processor will only produce one blob which contains full content of inputStream.
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="allocator"></param>
		/// <param name="blobIds">Output of stream shredding: a collection of blobId specifying the order of blobs </param>
		/// <param name="blobs"></param>
		public void StreamToBlobs(Stream inputStream, IBlobAllocator allocator, out string[] blobIds, out IReadOnlyDictionary<string, IBlob> blobs)
		{
			if (inputStream == null)
			{
				throw new ArgumentException($"{nameof(inputStream)} cannot be null.");
			}
			else if (inputStream.Length == 0)
			{
				blobIds = new string[0];
				blobs = new Dictionary<string, IBlob>();
				return;
			}

			var blobsToReturn = new Dictionary<string, IBlob>();
			var blobIdsToReturn = new List<string>();

			IBlob blob = allocator.CreateBlob(inputStream);
			blobsToReturn.Add(blob.BlobId, blob);
			blobIdsToReturn.Add(blob.BlobId);

			blobs = blobsToReturn;
			blobIds = blobIdsToReturn.ToArray();
		}

		public void StreamToBlobs(IResourceManager resourceManager, string resourceId, out string[] blobIds, out IReadOnlyDictionary<string, IBlob> blobs)
		{
			throw new NotImplementedException();
		}
	}
}
