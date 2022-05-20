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
	class PutChunkedFileRequest : WopiRequest
	{
		public PutChunkedFileRequest(WopiRequestParam param) : base(param)
		{
			this.SequenceNumber = param.SequenceNumber;
			this.SequenceNumberStateKey = param.SequenceNumberStateKey;
			this.Lock = param.Lock;
			this.CoauthLockId = param.CoauthLockId;
			this.Editors = param.Editors;
			this.ContentProperties = param.ContentProperties;
			this.ContentStreams = param.ContentStreams;
			this.UploadSessionTokenToCommit = param.UploadSessionTokenToCommit;
		}

		public override string Name { get { return Constants.Requests.PutChunkedFile; } }
		public string SequenceNumber { get; private set; }
		public string SequenceNumberStateKey { get; private set; }
		public string Lock { get; private set; }
		public string CoauthLockId { get; private set; }
		public string Editors { get; private set; }
		public IEnumerable<XMLContentProperty> ContentProperties { get; private set; }
		public IEnumerable<XMLContentStream> ContentStreams { get; private set; }
		public string UploadSessionTokenToCommit { get; private set; }

		protected override string RequestMethod { get { return Constants.RequestMethods.Post; } }

		// WopiOverrideValue will go into "X-WOPI-Override" header
		protected override string WopiOverrideValue { get { return Constants.Overrides.PutChunkedFile; } }
		protected override string PathOverride { get { return "/contents"; } }
		public override bool IsTextResponseExpected { get { return false; } }
		protected override bool HasRequestContent { get { return true; } }
		protected override IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			ParseHeaderParameters(savedState, out int sequenceNumber);

			return new Dictionary<string, string>()
			{
				{Constants.Headers.SequenceNumber, sequenceNumber.ToString()},
				{Constants.Headers.Lock, String.IsNullOrWhiteSpace(this.Lock) ? String.Empty : this.Lock},
				{Constants.Headers.CoauthLockId, String.IsNullOrWhiteSpace(this.CoauthLockId) ? String.Empty : this.CoauthLockId},
				{Constants.Headers.Editors, String.IsNullOrWhiteSpace(this.Editors) ? String.Empty : this.Editors}
			};
		}

		protected override MemoryStream GetRequestContent(IResourceManager resourceManager)
		{
			ValidateBodyParameters();

			PutChunkedFileRequestMessage putChunkedFileRequestMessage = BuildPutChunkedFileRequestMessageAndDeltaBlobs(resourceManager, out Dictionary<string, IBlob> deltaBlobs);

			FrameProtocolBuilder builder = BuildMessageAndChunkFrames(putChunkedFileRequestMessage, deltaBlobs);
			// This will add EndFrame and create stream
			MemoryStream outputMemoryStream = (MemoryStream)builder.CreateStream();

			return outputMemoryStream;
		}

		private ContentProperty[] BuildContentProperties()
		{
			var contentProperties = new List<ContentProperty>();

			if (this.ContentProperties == null || this.ContentProperties.Count() == 0)
			{
				return contentProperties.ToArray();
			}

			foreach (var contentProperty in this.ContentProperties)
			{
				contentProperties.Add(new ContentProperty(
					Retention:contentProperty.Retention.ToString(),
					Name:contentProperty.Name,
					Value:contentProperty.Value));
			}

			return contentProperties.ToArray();
		}

		private void BuildChunkSignaturesAndDeltaBlobDictionary(
			string newContent,
			string lastKnownHostContent,
			string newContentResourceId,
			string lastKnownHostContentResourceId,
			IResourceManager resourceManager,
			ChunkingScheme chunkingScheme,
			Dictionary<string, IBlob> deltaBlobs,
			out ChunkSignature[] chunkSignatures)
		{
			string[] newContentBlobIds = new string[ 0 ];
			IReadOnlyDictionary<string, IBlob> newContentBlobs = new Dictionary<string, IBlob>();
			IReadOnlyDictionary<string, IBlob> lastKnownHostContentBlobs = new Dictionary<string, IBlob>();

			if (chunkingScheme == ChunkingScheme.FullFile)
			{
				Stream newContentStream = new MemoryStream(Encoding.UTF8.GetBytes(newContent));
				Stream lastKnownHostContentStream = new MemoryStream(Encoding.UTF8.GetBytes(lastKnownHostContent));

				IChunkProcessor chunkProcessor = ChunkProcessorFactory.Instance.CreateInstance(chunkingScheme);

				chunkProcessor.StreamToBlobs(
					newContentStream,
					new BlobAllocator(),
					out newContentBlobIds,
					out newContentBlobs
				);
				chunkProcessor.StreamToBlobs(
					lastKnownHostContentStream,
					new BlobAllocator(),
					out _,
					out lastKnownHostContentBlobs
				);
			}
			else if (chunkingScheme == ChunkingScheme.Zip)
			{
				IChunkProcessor chunkProcessor = ChunkProcessorFactory.Instance.CreateInstance(chunkingScheme);

				chunkProcessor.StreamToBlobs(
					resourceManager,
					newContentResourceId,
					out newContentBlobIds,
					out newContentBlobs
				);
				chunkProcessor.StreamToBlobs(
					resourceManager,
					lastKnownHostContentResourceId,
					out _,
					out lastKnownHostContentBlobs
				);
			}

			

			ChunkSignature[] chunkSignaturesToReturn = new ChunkSignature[newContentBlobIds.Length];
			for(int i = 0; i < newContentBlobIds.Length; i++)
			{
				string newContentBlobId = newContentBlobIds[i];
				IBlob newContentBlob = newContentBlobs[newContentBlobId];
				chunkSignaturesToReturn[i] = new ChunkSignature(ChunkId: newContentBlobId, Length: newContentBlob.Length);
				if (!lastKnownHostContentBlobs.ContainsKey(newContentBlobId) && !deltaBlobs.ContainsKey(newContentBlobId))
				{
					deltaBlobs.Add(newContentBlobId, newContentBlob);
				}
			}

			chunkSignatures = chunkSignaturesToReturn;
		}

		private StreamSignature[] BuildSignatures(IResourceManager resourceManager, out Dictionary<string, IBlob> deltaBlobs)
		{
			var streamSignatures = new List<StreamSignature>();
			Dictionary<string, IBlob> deltaBlobsToReturn = new Dictionary<string, IBlob>();

			foreach(var contentStream in this.ContentStreams)
			{
				string newContent = contentStream.NewContent;
				string lastKnownHostContent = contentStream.LastKnownHostContent;
				string newContentResourceId = contentStream.NewContentResourceId;
				string lastKnownHostContentResourceId = contentStream.LastKnownHostContentResourceId;

				ChunkingScheme chunkingScheme = contentStream.ChunkingScheme;

				BuildChunkSignaturesAndDeltaBlobDictionary(
					newContent,
					lastKnownHostContent,
					newContentResourceId,
					lastKnownHostContentResourceId,
					resourceManager,
					chunkingScheme,
					deltaBlobsToReturn,
					out ChunkSignature[] chunkSignatures);

				streamSignatures.Add(new StreamSignature(
					StreamId: contentStream.StreamId,
					ChunkingScheme: contentStream.ChunkingScheme.ToString(),
					ChunkSignatures: chunkSignatures));
			}

			deltaBlobs = deltaBlobsToReturn;
			return streamSignatures.ToArray();
		}

		private PutChunkedFileRequestMessage BuildPutChunkedFileRequestMessageAndDeltaBlobs(IResourceManager resourceManager, out Dictionary<string, IBlob> deltaBlobs)
		{
			return new PutChunkedFileRequestMessage(
				ContentProperties: BuildContentProperties(),
				Signatures: BuildSignatures(resourceManager, out deltaBlobs),
				UploadSessionTokenToCommit: this.UploadSessionTokenToCommit);
		}

		private FrameProtocolBuilder BuildMessageAndChunkFrames(
			PutChunkedFileRequestMessage putChunkedFileRequestMessage,
			Dictionary<string, IBlob> deltaBlobs)
		{
			FrameProtocolBuilder builder = new FrameProtocolBuilder();
			builder.AddFrame(putChunkedFileRequestMessage);

			// Concat multi-stream blobs together into a frame list
			foreach (IBlob blob in deltaBlobs.Values)
			{
				builder.AddFrame(blob);
			}

			return builder;
		}

		private void ParseHeaderParameters(Dictionary<string, string> savedState, out int outSequenceNumber)
		{
			if (!String.IsNullOrWhiteSpace(this.SequenceNumberStateKey) &&
				savedState.TryGetValue(this.SequenceNumberStateKey, out string val) &&
				int.TryParse(val, out int parsedVal))
			{
				outSequenceNumber = parsedVal;
				return;
			}

			if (int.TryParse(this.SequenceNumber, out int parsedSequenceNumber))
			{
				outSequenceNumber = parsedSequenceNumber;
				return;
			}

			throw new ArgumentException(String.Format(
				CultureInfo.CurrentCulture,
				"PutChunkedFileRequest.GetCustomHeaders '{0}' parameter encountered problem while parsing",
				nameof(this.SequenceNumber)));
		}

		private void ValidateBodyParameters()
		{
			if (this.ContentStreams == null || !this.ContentStreams.Any())
			{
				throw new ArgumentException(String.Format(
					CultureInfo.CurrentCulture,
					"PutChunkedFileRequest.GetRequestContent '{0}' is null or empty",
					nameof(this.ContentStreams)));
			}
		}
	}
}
