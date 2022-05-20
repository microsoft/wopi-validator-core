using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	static class ContentFilterFactory
	{
		public static IEnumerable<XMLContentFilter> GetContentFilters(XElement definition)
		{
			return definition.Elements().Select(GetContentFilter);
		}

		/// <summary>
		/// Parses a single ContentFilter element and creates an appropriate ContentFilter object to represent it.
		/// </summary>
		private static XMLContentFilter GetContentFilter(XElement definition)
		{
			string ChunkingScheme = (string)definition.Attribute("ChunkingScheme");
			string StreamId = (string)definition.Attribute("StreamId");
			string ChunksToReturn = (string)definition.Attribute("ChunksToReturn");
			string AlreadyExistingContent = (string)definition.Attribute("AlreadyExistingContent");
			string AlreadyExistingContentResourceId = (string)definition.Attribute("AlreadyExistingContentResourceId");

			return new XMLContentFilter(
				ChunkingScheme,
				StreamId,
				ChunksToReturn,
				AlreadyExistingContent,
				AlreadyExistingContentResourceId);
		}
	}
}
