using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public interface IBlobAllocator
	{
		/// <summary>
		/// Create an IBlob with input stream.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		IBlob CreateBlob(Stream stream);

		/// <summary>
		/// Create an IBlob with a sub section of input stream.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="toRead"></param>
		/// <returns></returns>
		IBlob CreateBlob(Stream stream, int toRead);

		/// <summary>
		/// Create an IBlob with input byte array.
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		IBlob CreateBlob(byte[] bytes);
	}

	/// <summary>
	/// BlobAllocator is used to create MemoryBlob.
	/// </summary>
	public class BlobAllocator : IBlobAllocator
	{
		public IBlob CreateBlob(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			IBlob blob = new MemoryBlob(stream);

			return blob;
		}

		public IBlob CreateBlob(byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));

			IBlob blob = new MemoryBlob(bytes);

			return blob;
		}

		/// <summary>
		/// Stream.Read can completes once a single byte is availble which is not good when we have to read the whole payload. 
		/// StreamToBlob guarantees reading N bytes by calling Stream.Read multiple times
		/// </summary>
		public IBlob CreateBlob(Stream stream, int toRead)
		{
			byte[] buffer = new byte[ toRead ];

			while (toRead > 0)
			{
				int actualRead = stream.Read(buffer, offset: 0, count: toRead);
				if (actualRead == 0)
				{
					throw new IOException($"{nameof(IBlobAllocator.CreateBlob)} actual read 0.");
				}
				toRead -= actualRead;
			}

			return new MemoryBlob(buffer);
		}
	}
}
