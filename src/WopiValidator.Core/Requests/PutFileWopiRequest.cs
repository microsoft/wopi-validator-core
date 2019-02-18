// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class PutFileWopiRequest : WopiRequest
	{
		public PutFileWopiRequest(WopiRequestParam param) : base(param)
		{
			this.LockString = param.LockString;
			this.ResourceId = param.ResourceId;
		}

		public string LockString { get; private set; }
		public string ResourceId { get; private set; }
		public override string Name { get { return Constants.Requests.PutFile; } }
		protected override string PathOverride { get { return "/contents"; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.Put; } }
		protected override bool HasRequestContent { get { return true; } }
		protected override IEnumerable<KeyValuePair<string, string>> DefaultHeaders
		{
			get
			{
				if (string.IsNullOrEmpty(LockString))
					return Enumerable.Empty<KeyValuePair<string, string>>();

				return new Dictionary<string, string>
				{
					{Constants.Headers.Lock, LockString}
				};
			}
		}

		protected override MemoryStream GetRequestContent(IResourceManager resourceManager)
		{
			return resourceManager.GetContentStream(ResourceId);
		}
	}
}
