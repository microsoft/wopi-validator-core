using System;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	/// <summary>
	/// Abstracts the backing store behind a blob to provide flexibility
	/// as to where the blob is stored.
	/// </summary>
	public interface IBlob
	{
		/// <summary>
		/// Creates a stream around the backing store
		/// </summary>
		/// <remarks>Note, there may be limitations on the number of streams created</remarks>
		/// <returns>The created readonly stream</returns>
		Stream GetStream();

		/// <summary>
		/// Returns the blob as a byte[]. This call should be avoided except in performance
		/// critical situations. For instance, when writing the blob to a network request
		/// we want to avoid creating an entire new copy of the blob. This call allows
		/// access to the underlying buffer of the blob.
		/// </summary>
		/// <remarks>
		/// This call may be expensive. It also provides direct access to the blob data.
		/// Use <see cref="GetStream"/> to provide safe access to the blob.
		/// </remarks>
		/// <returns>A byte[] of the blob data if available. Null otherwise.</returns>
		byte[] ToArray();

		/// <summary>
		/// Length of the backing store. Allows for quick access without creating a stream.
		/// </summary>
		UInt64 Length { get; }

		/// <summary>
		/// Get the id in string format of the blob
		/// </summary>
		string BlobId { get; }
	}

	/// Implement memory version of IBlob.
	public class MemoryBlob : IBlob
	{
		private static SpookyHash _spookyHash = new SpookyHash();

		public byte[] Bytes { get; private set; }

		public ulong Length
		{
			get { return (ulong)Bytes.LongLength; }
		}

		public string BlobId { get; private set; }

		public MemoryBlob(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			using (MemoryStream ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				Bytes = ms.ToArray();
				BlobId = _spookyHash.CalculateHash(ms);
			}

		}

		public MemoryBlob(byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));

			Bytes = bytes;
			BlobId = _spookyHash.CalculateHash(bytes);
		}

		public Stream GetStream()
		{
			return new MemoryStream(Bytes, writable: false);
		}

		public byte[] ToArray()
		{
			return Bytes;
		}
	}
}
