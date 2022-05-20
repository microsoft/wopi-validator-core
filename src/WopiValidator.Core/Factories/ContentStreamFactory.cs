using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	static class ContentStreamFactory
	{
		public static IEnumerable<XMLContentStream> GetContentStreams(XElement definition)
		{
			return definition.Elements().Select(GetContentStream);
		}

		/// <summary>
		/// Parses a single StreamSignature element and creates an appropriate StreamSignature object to represent it.
		/// </summary>
		private static XMLContentStream GetContentStream(XElement definition)
		{
			string ChunkingScheme = (string)definition.Attribute("ChunkingScheme");
			string StreamId = (string)definition.Attribute("StreamId");
			string NewContent = (string)definition.Attribute("NewContent");
			string LastKnownHostContent = (string)definition.Attribute("LastKnownHostContent");
			string NewContentResourceId = (string)definition.Attribute("NewContentResourceId");
			string LastKnownHostContentResourceId = (string)definition.Attribute("LastKnownHostContentResourceId");

			return new XMLContentStream(
				ChunkingScheme,
				StreamId,
				NewContent,
				LastKnownHostContent,
				NewContentResourceId,
				LastKnownHostContentResourceId);
		}
	}
}
