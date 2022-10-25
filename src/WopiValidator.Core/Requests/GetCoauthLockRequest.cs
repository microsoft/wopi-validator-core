// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.Office.WopiValidator.Core.ResourceManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Principal;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetCoauthLockRequest : WopiRequest
	{
		public GetCoauthLockRequest(WopiRequestParam param) : base(param)
		{
			CoauthLockType = param.CoauthLockType;
			CoauthLockMetadata = param.CoauthLockMetadata;
			CoauthLockId = param.CoauthLockId;
			CoauthLockExpirationTimeout = param.CoauthLockExpirationTimeout;
			CoauthLockMetadataAsBody = param.CoauthLockMetadataAsBody;
		}

		protected override bool HasRequestContent { get { return true; } }
		public uint? CoauthLockExpirationTimeout { get; private set; }
		public string CoauthLockMetadata { get; private set; }
		public CoauthLockMetadataEntity CoauthLockMetadataAsBody { get; private set; }
		public string CoauthLockId { get; private set; }
		public CoauthLockType? CoauthLockType { get; private set; }
		public override string Name { get { return Constants.Requests.GetCoauthLock; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Post; } }
		protected override string WopiOverrideValue { get { return Constants.Overrides.GetCoauthLock; } }

		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();

			if (CoauthLockType != null)
			{
				headers.Add(Constants.Headers.CoauthLockType, CoauthLockType.Value.ToString());
			}
			if (CoauthLockMetadataAsBody == null && CoauthLockMetadata != null)
			{
				headers.Add(Constants.Headers.CoauthLockMetadata, CoauthLockMetadata);
			}
			if (CoauthLockId != null)
			{
				headers.Add(Constants.Headers.CoauthLockId, CoauthLockId);
			}
			if (CoauthLockExpirationTimeout != null)
			{
				headers.Add(Constants.Headers.CoauthLockExpirationTimeout, CoauthLockExpirationTimeout.Value.ToString());
			}
			return headers;
		}

		protected override MemoryStream GetRequestContent(IResourceManager resourceManager)
		{
			if (CoauthLockMetadataAsBody != null)
			{
				WopiCoauthLockMetadata metadata = new WopiCoauthLockMetadata();
				metadata.CoauthLockMetadata = CoauthLockMetadataAsBody.CoauthLockMetadata;
				byte[] jsonAsBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata));
				MemoryStream stream = new MemoryStream(jsonAsBytes);
				stream.Position = 0;
				return stream;
			}

			return null;
		}
	}
}
