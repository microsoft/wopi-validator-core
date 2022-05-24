using System.IO;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public class FrameProtocolStreamUtils
	{
		/// <summary>
		/// Stream.Read can completes once a single byte is availble which is not good when we have to read the whole payload. 
		/// ReadChunk guarantees reading N bytes by calling Stream.Read multiple times
		/// </summary>
		public static void ReadChunk(Stream source, byte[] buffer, int offset, int toRead)
		{
			while (toRead > 0)
			{
				int actualRead = source.Read(buffer, offset, toRead);
				if (actualRead == 0)
				{
					throw new IOException("In FrameProtocolStreamUtils.ReadChunk, actual read 0.");
				}
				toRead -= actualRead;
				offset += actualRead;
			}
		}
	}
}
