// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class AddActivitiesRequest : WopiRequest
	{
		public AddActivitiesRequest(WopiRequestParam param) : base(param)
		{
			if (String.IsNullOrWhiteSpace(param.RequestBody))
				throw new ArgumentOutOfRangeException("RequestBody", "No RequestBody specified for AddActivities operation");

			this.ActivitiesJson = param.RequestBody;
		}

		public string ActivitiesJson { get; private set; }
		public override string Name { get { return Constants.Requests.AddActivities; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.AddActivities; } }
		protected override bool HasRequestContent { get { return true; } }

		protected override MemoryStream GetRequestContent(IResourceManager resourceManager)
		{
			byte[] jsonAsBytes = Encoding.UTF8.GetBytes(this.ActivitiesJson);
			return new MemoryStream(jsonAsBytes);
		}
	}
}
