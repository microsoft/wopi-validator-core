// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class PutRelativeFileWopiRequest : WopiRequest
	{
		public PutRelativeFileWopiRequest(WopiRequestParam param, string guid) : base(param)
		{
			this.ResourceId = param.ResourceId;
			if (string.IsNullOrEmpty(param.RequestedName))
				throw new ArgumentException("Requested name for a PutRelativeFile Wopi request cannot be null or empty");
			this.RequestedName = string.Format("{0}-{1}", param.RequestedName, guid);
			this.RequestType = param.PutRelativeFileMode;
			this.OverwriteRelative = param.OverwriteRelative;
		}

		public PutRelativeFileMode RequestType { get; private set; }
		public bool? OverwriteRelative { get; private set; }
		public string RequestedName { get; private set; }
		public string LockString { get; private set; }
		public string ResourceId { get; private set; }
		public override string Name { get { return Constants.Requests.PutRelativeFile; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.PutRelative; } }
		protected override bool HasRequestContent { get { return true; } }

		protected override MemoryStream GetRequestContent(IResourceManager resourceManager)
		{
			return resourceManager.GetContentStream(ResourceId);
		}

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add(Constants.Headers.Size, GetRequestContent(resourceManager).Length.ToString());
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
