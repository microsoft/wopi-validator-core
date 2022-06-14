// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class CreateChildFileRequest : WopiRequest
	{
		public CreateChildFileRequest(WopiRequestParam param, string guid) : base(param)
		{
			this.RequestedName = string.Format("{0}-{1}{2}",
				System.IO.Path.GetFileNameWithoutExtension(param.RequestedName),
				guid,
				System.IO.Path.GetExtension(param.RequestedName));
			this.RequestType = param.PutRelativeFileMode;
			this.OverwriteRelative = param.OverwriteRelative;
		}

		public PutRelativeFileMode RequestType { get; private set; }
		public bool? OverwriteRelative { get; private set; }
		public string RequestedName { get; private set; }
		public override string Name { get { return Constants.Requests.CreateChildFile; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.CreateChildFile; } }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			switch (RequestType)
			{
				case PutRelativeFileMode.Suggested:
					headers.Add(Constants.Headers.SuggestedTarget, UrlHelper.GetUTF7EncodedUnescapedDataString(RequestedName));
					break;
				case PutRelativeFileMode.ExactName:
					headers.Add(Constants.Headers.RelativeTarget, UrlHelper.GetUTF7EncodedUnescapedDataString(RequestedName));
					if (OverwriteRelative.HasValue)
						headers.Add(Constants.Headers.OverwriteRelative, OverwriteRelative.Value.ToString());
					break;
				case PutRelativeFileMode.Conflicting:
					headers.Add(Constants.Headers.SuggestedTarget, UrlHelper.GetUTF7EncodedUnescapedDataString(RequestedName));
					headers.Add(Constants.Headers.RelativeTarget, UrlHelper.GetUTF7EncodedUnescapedDataString(RequestedName));
					break;
				default:
					throw new ArgumentOutOfRangeException("RequestType", string.Format("Unknown PutRelativeFileMode specified: {0}", RequestType));
			}
			return headers;
		}
	}
}
