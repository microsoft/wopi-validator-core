// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	class GetChunkedFileRequest : WopiRequest
	{
		public GetChunkedFileRequest(WopiRequestParam param) : base(param)
		{
			this.WopiSrc = param.WopiSrc;
			this.ContentPropertiesToReturn = param.ContentPropertiesToReturn;
			this.ContentFilters = param.ContentFilters;
		}

		public override string Name { get { return Constants.Requests.GetChunkedFile; } }
		protected override string RequestMethod { get { return Constants.RequestMethods.Post; } }

		// WopiOverrideValue will go into "X-WOPI-Override" header
		protected override string WopiOverrideValue { get { return Constants.Overrides.GetChunkedFile; } }
		public string WopiSrc { get; private set; }
		public IEnumerable<XMLContentPropertyToReturn> ContentPropertiesToReturn { get; private set; }
		public IEnumerable<XMLContentFilter> ContentFilters { get; private set; }
		public override bool IsTextResponseExpected { get { return false; } }
		protected override bool HasRequestContent { get { return true; } }

		protected override MemoryStream GetRequestContent(IResourceManager resourceManager)
		{
			ValidateBodyParameters();

			GetChunkedFileRequestMessage GetChunkedFileRequestMessage = BuildGetChunkedFileRequest(resourceManager);

			FrameProtocolBuilder builder = BuildMessageJSONFrame(GetChunkedFileRequestMessage);
			// builder.CreateStream() will add EndFrame and create stream
			MemoryStream outputMemoryStream = (MemoryStream)builder.CreateStream();

			return outputMemoryStream;
		}

		private string[] BuildContentPropertiesToReturn()
		{
			var contentPropertiesToReturn = new List<string>();

			if (ContentPropertiesToReturn == null)
			{
				return contentPropertiesToReturn.ToArray();
			}

			foreach(var contentPropertyToReturn in this.ContentPropertiesToReturn)
			{
				contentPropertiesToReturn.Add(contentPropertyToReturn.Value);
			}

			return contentPropertiesToReturn.ToArray();
		}

		private ContentFilter[] BuildContentFilters(IResourceManager resourceManager)
		{
			var contentFilters = new List<ContentFilter>();

			foreach(var contentFilter in this.ContentFilters)
			{
				ChunkingScheme chunkingScheme = contentFilter.ChunkingScheme;
				IReadOnlyDictionary<string, IBlob> alreadyExistingContentBlobs = new Dictionary<string, IBlob>();

				if (chunkingScheme == ChunkingScheme.FullFile)
				{
					string alreadyExistingContent = contentFilter.AlreadyExistingContent;
					Stream alreadyExistingContentStream = new MemoryStream(Encoding.UTF8.GetBytes(alreadyExistingContent));
					IBlobAllocator blobAllocator = new BlobAllocator();

					ChunkProcessorFactory.Instance.CreateInstance(chunkingScheme).StreamToBlobs(
						alreadyExistingContentStream,
						blobAllocator,
						out string[] _,
						out alreadyExistingContentBlobs
					);
				}
				else if (chunkingScheme == ChunkingScheme.Zip)
				{
					string alreadyExistingContentResourceId = contentFilter.AlreadyExistingContentResourceId;
					ChunkProcessorFactory.Instance.CreateInstance(chunkingScheme).StreamToBlobs(
						resourceManager,
						alreadyExistingContentResourceId,
						out string[] _,
						out alreadyExistingContentBlobs
					);
				}


				contentFilters.Add(new ContentFilter(
						ChunkingScheme: contentFilter.ChunkingScheme.ToString(),
						StreamId: contentFilter.StreamId,
						ChunksToReturn: contentFilter.ChunksToReturn.ToString(),
						AlreadyKnownChunks: alreadyExistingContentBlobs.Keys.ToArray()
				));
			}

			return contentFilters.ToArray();
		}

		private GetChunkedFileRequestMessage BuildGetChunkedFileRequest(IResourceManager resourceManager)
		{
			return new GetChunkedFileRequestMessage(
				BuildContentPropertiesToReturn(),
				BuildContentFilters(resourceManager)
			);
		}

		private FrameProtocolBuilder BuildMessageJSONFrame(GetChunkedFileRequestMessage getChunkedFileRequestMessage)
		{
			FrameProtocolBuilder builder = new FrameProtocolBuilder();
			builder.AddFrame(getChunkedFileRequestMessage);

			return builder;
		}

		private void ValidateBodyParameters()
		{
			if (this.ContentFilters == null || !this.ContentFilters.Any())
			{
				throw new ArgumentException(String.Format(
					CultureInfo.CurrentCulture,
					"GetChunkedFileRequest.GetRequestContent '{0}' parameter can't be null or empty",
					nameof(this.ContentFilters)));
			}
		}
	}
}
