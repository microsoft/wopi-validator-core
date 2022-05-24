using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	public class JsonMessageSerializer
	{
		private static readonly Lazy<JsonMessageSerializer> _instance = new Lazy<JsonMessageSerializer>();

		public static JsonMessageSerializer Instance
		{
			get { return _instance.Value; }
		}

		public T DeSerialize<T>(Stream payload)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
			var request = (T)ser.ReadObject(payload);

			return request;
		}

		public Stream Serialize<T>(T t)
		{
			MemoryStream ms = new MemoryStream();
			DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
			ser.WriteObject(ms, t);

			return ms;
		}
	}
}
