using System;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public class XMLContentProperty
	{
		public ContentPropertyRetention Retention { get; private set; }
		public string Name { get; private set; }
		public string Value { get; private set; }

		public XMLContentProperty(string Retention, string Name, string Value)
		{
			ValidateAndParseParameters(Retention, Name, Value, out ContentPropertyRetention ParsedRetention);

			this.Retention = ParsedRetention;
			this.Name = Name;
			this.Value = Value;
		}

		private void ValidateAndParseParameters(string Retention, string Name, string Value, out ContentPropertyRetention OutputParsedRetention)
		{
			if (String.IsNullOrWhiteSpace(Retention))
				throw new ArgumentException("PutChunkedFileContentProperty 'Retention' parameter is invalid!");

			if (String.IsNullOrWhiteSpace(Name))
				throw new ArgumentException("PutChunkedFileContentProperty 'Name' parameter is invalid!");

			if (string.IsNullOrWhiteSpace(Value))
				throw new ArgumentException("PutChunkedFileContentProperty 'Value' parameter is invalid!");

			if (!Enum.TryParse(Retention, true, out ContentPropertyRetention ParsedRetention))
			{
				throw new ArgumentException("PutChunkedFileContentProperty 'Retention' parameter can not be parsed to enum!");
			}

			OutputParsedRetention = ParsedRetention;
		}
	}
}
