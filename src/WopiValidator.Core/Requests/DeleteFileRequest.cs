// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class DeleteFileRequest : WopiRequest
	{
		public DeleteFileRequest(WopiRequestParam param) : base(param)
		{
		}

		public override string Name { get { return Constants.Requests.DeleteFile; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.Delete; } }
	}
}
