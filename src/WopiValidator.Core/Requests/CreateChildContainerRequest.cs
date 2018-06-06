// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using System.Collections.Generic;
using System.Web;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class CreateChildContainerRequest : WopiRequest
	{
		public CreateChildContainerRequest(WopiRequestParam param) : base(param)
		{
			this.FolderName = param.FolderName;
			this.RequestType = param.PutRelativeFileMode;
		}

		public string FolderName { get; private set; }
		public PutRelativeFileMode RequestType { get; private set; }

		public override string Name { get { return Constants.Requests.CreateChildContainer; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.CreateChildContainer; } }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			switch (RequestType)
			{
				case PutRelativeFileMode.Suggested:
					headers.Add(Constants.Headers.SuggestedTarget, UrlHelper.GetUTF7EncodedUnescapedDataString(FolderName));
					break;
				case PutRelativeFileMode.ExactName:
					headers.Add(Constants.Headers.RelativeTarget, UrlHelper.GetUTF7EncodedUnescapedDataString(FolderName));
					break;
				case PutRelativeFileMode.Conflicting:
					headers.Add(Constants.Headers.SuggestedTarget, UrlHelper.GetUTF7EncodedUnescapedDataString(FolderName));
					headers.Add(Constants.Headers.RelativeTarget, UrlHelper.GetUTF7EncodedUnescapedDataString(FolderName));
					break;
				default:
					throw new ArgumentOutOfRangeException("RequestType", string.Format("Unknown PutRelativeFileMode specified: {0}", RequestType));
			}
			return headers;
		}
	}
}
