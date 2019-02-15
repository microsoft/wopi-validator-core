// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetShareUrlRequest : WopiRequest
	{
		public GetShareUrlRequest(WopiRequestParam param) : base(param)
		{
			this.UrlType = param.UrlType;
		}

		public string UrlType { get; private set; }
		public override string Name { get { return Constants.Requests.GetShareUrl; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.GetShareUrl; } }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			if (!string.IsNullOrEmpty(this.UrlType) && this.UrlType.StartsWith(Constants.StateOverrides.StateToken))
			{
				string setting = this.UrlType.Substring(Constants.StateOverrides.StateToken.Length);

				string urlTypes;
				savedState.TryGetValue(setting, out urlTypes);
				JArray array = JArray.Parse(urlTypes);

				int index = new System.Random().Next(0, array.Count);
				this.UrlType = array[index].ToString();
			}

			if (string.IsNullOrEmpty(this.UrlType))
				return null;

			return new Dictionary<string, string>
			{
				{ Constants.Headers.UrlType, this.UrlType}
			};
		}
	}
}
