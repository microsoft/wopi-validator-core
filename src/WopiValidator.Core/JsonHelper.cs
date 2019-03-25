// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Microsoft.Office.WopiValidator
{
	internal static class JsonHelper
	{
		internal static JObject ParseJObject(this string jsonString)
		{
			JsonReader reader = new JsonTextReader(new StringReader(jsonString))
			{
				DateParseHandling = DateParseHandling.None
			};
			JObject jObject = JObject.Load(reader);
			return jObject;
		}
	}
}
