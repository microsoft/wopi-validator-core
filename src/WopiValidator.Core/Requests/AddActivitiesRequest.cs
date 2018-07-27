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
			if (String.IsNullOrWhiteSpace(param.RequestBodyJson))
				throw new ArgumentOutOfRangeException("RequestBodyJson", "No RequestBodyJson specified for AddActivities operation");

			this.ActivitiesJson = param.RequestBodyJson;
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
