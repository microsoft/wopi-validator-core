// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Office.WopiValidator.Core
{
	enum StateSourceType
	{
		JsonBody,
		Header
	}

	class StateEntry : IStateEntry
	{
		public string Name { get; private set; }
		public string Source { get; private set; }
		public StateSourceType SourceType { get; private set; }

		public StateEntry(string name, string source, StateSourceType sourceType)
		{
			Name = name;
			Source = source;
			SourceType = sourceType;
		}

		public StateEntry(string name, string source) : this(name, source, StateSourceType.JsonBody)
		{
		}

		public string GetValue(IResponseData data)
		{
			switch (SourceType)
			{
				case StateSourceType.Header:
					string returnValue = GetValueFromHeader(data);
					if (String.IsNullOrEmpty(returnValue))
					{
						return null;
					}
					return returnValue;

				case StateSourceType.JsonBody:
				default:
					string responseContentString = data.GetResponseContentAsString();
					if (!data.IsTextResponse || String.IsNullOrEmpty(responseContentString))
					{
						return null;
					}

					return GetValueFromJson(responseContentString);
			}
		}

		private string GetValueFromJson(string jsonString)
		{
			try
			{
				JObject jObject = jsonString.ParseJObject();
				JToken token = jObject.SelectToken(Source);
				return token.Value<string>();
			}
			catch
			{
				return null;
			}
		}

		private string GetValueFromHeader(IResponseData data)
		{
			try
			{
				return data.Headers[Source];
			}
			catch
			{
				return null;
			}
		}
	}
}
