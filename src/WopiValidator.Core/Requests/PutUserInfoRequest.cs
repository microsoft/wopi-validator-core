// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class PutUserInfoRequest : WopiRequest
	{
		public PutUserInfoRequest(WopiRequestParam param) : base(param)
		{
			if (String.IsNullOrWhiteSpace(param.RequestBody))
				throw new ArgumentOutOfRangeException("RequestBody", "No RequestBody specified for PutUserInfo operation");

			this.UserInfo = param.RequestBody;
		}

		public string UserInfo { get; private set; }
		public override string Name { get { return Constants.Requests.PutUserInfo; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.PutUserInfo; } }
		protected override bool HasRequestContent { get { return true; } }

		protected override MemoryStream GetRequestContent(IResourceManager resourceManager)
		{
			byte[] bodyAsBytes = Encoding.UTF8.GetBytes(this.UserInfo);
			return new MemoryStream(bodyAsBytes);
		}
	}
}
