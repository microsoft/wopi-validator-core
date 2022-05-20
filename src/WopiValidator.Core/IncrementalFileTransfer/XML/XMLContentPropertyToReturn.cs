using System;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public class XMLContentPropertyToReturn
	{
		public string Value { get; private set; }
		public XMLContentPropertyToReturn(string Value)
		{
			ValidateParameter(Value);

			this.Value = Value;
		}

		private void ValidateParameter(string Value)
		{
			if (string.IsNullOrWhiteSpace(Value))
				throw new ArgumentException("GetChunkedFile ContentPropertyToReturn 'Value' parameter is invalid.");
		}
	}
}
