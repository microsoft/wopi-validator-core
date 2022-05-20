// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetCoauthTableRequest : WopiRequest
	{
		public GetCoauthTableRequest(WopiRequestParam param) : base(param)
		{
			this.CoauthTableVersion = param.CoauthTableVersion;
			this.CoauthTableVersionStateKey = param.CoauthTableVersionStateKey;
		}

		public override string Name { get { return Constants.Requests.GetCoauthTable; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Post; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.GetCoauthTable; } }
		public string CoauthTableVersion { get; private set; }
		public string CoauthTableVersionStateKey { get; private set; }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			string coauthTableVersion = GetCoauthTableVersion(savedState);

			return new Dictionary<string, string>()
			{
				{Constants.Headers.CoauthTableVersion, String.IsNullOrWhiteSpace(coauthTableVersion) ? String.Empty : coauthTableVersion}
			};
		}

		private string GetCoauthTableVersion(Dictionary<string, string> savedState)
		{
			if (!String.IsNullOrWhiteSpace(this.CoauthTableVersionStateKey) &&
				!String.IsNullOrWhiteSpace(this.CoauthTableVersion))
			{
				throw new ArgumentException(String.Format(
					CultureInfo.CurrentCulture,
					"GetCoauthTableRequest attributes '{0}' and '{1}' cannot both exist, only one of them should be populated.",
					nameof(this.CoauthTableVersionStateKey),
					nameof(this.CoauthTableVersion)));
			}

			if (!String.IsNullOrWhiteSpace(this.CoauthTableVersionStateKey) &&
				savedState.TryGetValue(this.CoauthTableVersionStateKey, out string val))
			{
				return val;
			}

			if (!String.IsNullOrWhiteSpace(this.CoauthTableVersion))
			{
				return this.CoauthTableVersion;
			}

			return String.Empty;
		}
	}
}
