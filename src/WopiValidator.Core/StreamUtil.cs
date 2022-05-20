using System;
using System.IO;

namespace Microsoft.Office.WopiValidator.Core
{
	public class StreamUtil
	{
		public static bool StreamEquals(Stream stream1, Stream stream2)
		{
			if (stream1 == stream2)
				return true;

			if (stream1 == null || stream2 == null)
				throw new ArgumentNullException(stream1 == null ? "stream1" : "stream2");

			if (stream1.Length != stream2.Length)
			{
				return false;
			}

			for (int i = 0; i < stream1.Length; i++)
			{
				int firstByte = stream1.ReadByte();
				int secondByte = stream2.ReadByte();
				if (firstByte.CompareTo(secondByte) != 0)
				{
					return false;
				}
			}

			return true;
		}
	}
}
