using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public class StreamBuilder
	{
		/// <summary>
		/// Take a set of blobs and compose them into a stream.
		/// </summary>
		public static byte[] BlobsToBytes(string[] hostBlobIds, IReadOnlyDictionary<string, IBlob> hostRevisionBlobs)
		{
			if (hostBlobIds == null)
			{
				throw new ArgumentNullException($"{nameof(hostBlobIds)} cannot be null.");
			}

			if (hostRevisionBlobs == null)
			{
				throw new ArgumentNullException($"{nameof(hostRevisionBlobs)} cannot be null.");
			}

			// Empty file
			if (hostBlobIds.Length == 0 && hostRevisionBlobs.Count == 0)
			{
				return new byte[0];
			}

			MemoryStream outputStream = new MemoryStream();
			foreach (string hostBlobId in hostBlobIds)
			{
				using (Stream blobStream = hostRevisionBlobs[ hostBlobId ].GetStream())
				{
					blobStream.CopyTo(outputStream);
				}
			}

			outputStream.Seek(0, SeekOrigin.Begin);

			return outputStream.ToArray();
		}
	}
}
