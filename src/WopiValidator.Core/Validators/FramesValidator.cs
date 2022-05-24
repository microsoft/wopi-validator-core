// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.Office.WopiValidator.Core.ResourceManagement;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	class FramesValidator : IValidator
	{
		public FramesValidator(
			string messageJsonPayloadSchema,
			int? expectedHostBlobsCount,
			IEnumerable<ContentStreamValidator> contentStreamValidators,
			IEnumerable<ContentPropertyValidator> contentPropertyValidators)
		{
			this.MessageJsonPayloadSchema = messageJsonPayloadSchema;
			this.ExpectedHostBlobsCount = expectedHostBlobsCount;
			this.ContentStreamValidators = contentStreamValidators;
			this.ContentPropertyValidators = contentPropertyValidators;

			this.HostBlobIdsPerStreamId = new Dictionary<string, List<string>>();
			this.HostBlobs = new Dictionary<string, IBlob>();
			this.ChunkingSchemePerStreamId = new Dictionary<string, string>();
			this.ContentProperties = new Dictionary<string, ContentProperty>();

			if (!JsonSchemas.Schemas.TryGetValue(this.MessageJsonPayloadSchema, out _schema))
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Schema with ID '{0}' not found.", this.MessageJsonPayloadSchema));
			}
		}

		public string MessageJsonPayloadSchema { get; private set; }
		public IEnumerable<ContentStreamValidator> ContentStreamValidators { get; set; }
		public IEnumerable<ContentPropertyValidator> ContentPropertyValidators { get; set; }
		public int? ExpectedHostBlobsCount { get; private set; }
		private readonly JsonSchema4 _schema;
		private Dictionary<string, List<string>> HostBlobIdsPerStreamId { get; set; }
		private Dictionary<string, IBlob> HostBlobs { get; set; }
		private Dictionary<string, string> ChunkingSchemePerStreamId { get; set; }
		private Dictionary<string, ContentProperty> ContentProperties { get; set; }
		public string Name { get { return "FramesValidator"; } }

		public ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState)
		{
			// Read and parse download response to enrich the chunkId to blob mapping, and stream id to chunk ids mapping of the file from download
			switch (this.MessageJsonPayloadSchema)
			{
				case Constants.JsonSchema.GetChunkedFileResponseSchema:
					// Validate GetChunkedFileResponseMessage, enrich HostBlobIdsPerStreamId and HostBlobs
					ValidationResult result = ValidateGetChunkedFileResponse(data);
					if (result.HasFailures)
						return result;
					break;
				default:
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unknown Schema: '{0}'", this.MessageJsonPayloadSchema));
			}

			// For each stream id, compose the new file content based on delta from download, and validate against expected value
			// In the future, we should think of better design to compose the new file content based on existing file content + delta from download
			foreach (ContentStreamValidator contentStreamValidator in this.ContentStreamValidators)
			{
				ValidationResult contentStreamValidation = contentStreamValidator.Validate(
					HostBlobIdsPerStreamId,
					HostBlobs,
					ChunkingSchemePerStreamId,
					resourceManager);

				if (contentStreamValidation.HasFailures)
					return contentStreamValidation;
			}

			// Iterate to validate content properties
			foreach (ContentPropertyValidator contentPropertyValidator in this.ContentPropertyValidators)
			{
				ValidationResult contentPropertyValidation = contentPropertyValidator.Validate(this.ContentProperties);

				if (contentPropertyValidation.HasFailures)
					return contentPropertyValidation;
			}

			return new ValidationResult();
		}

		private ValidationResult ValidateGetChunkedFileResponse(IResponseData data)
		{
			data.ResponseStream.Seek(0, SeekOrigin.Begin);

			if (data.ResponseStream == null || data.ResponseStream.Length == 0)
				return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "ResponseData.ResponseStream is null or empty"));

			List<Frame> outputFrames = FrameProtocolParser.ParseStream(data.ResponseStream);
			data.ResponseStream.Seek(0, SeekOrigin.Begin);

			if (outputFrames == null || outputFrames.Count < 1)
				return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "Parsed outputFrameList is null or empty"));

			if (outputFrames[0].Type != FrameType.MessageJSON)
				return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "The first frame is not MessageJson type"));

			for (int i = 0; i<outputFrames.Count ; i++)
			{
				Frame outputFrame = outputFrames[i];
				ValidationResult frameValidation = ValidateFrame(outputFrame);

				if (frameValidation.HasFailures)
					return frameValidation;

				switch (outputFrame.Type)
				{
					case FrameType.MessageJSON:
						// Enrich HostBlobIdsPerStreamId with chunk ids of the file from download, keyed by stream id
						PopulateMessageFrame(outputFrame);
						break;
					case FrameType.Chunk:
						// Enrich HostBlobs with delta blobs of the file from download
						PopulateHostFileChunkIdToBlobMapping(outputFrame);
						break;
					default:
						break;
				}
			}

			if (this.ExpectedHostBlobsCount != null && this.ExpectedHostBlobsCount.Value != HostBlobs.Count)
				return new ValidationResult($"{nameof(ExpectedHostBlobsCount)}: '{ExpectedHostBlobsCount.Value}' not equal to {nameof(HostBlobs)}.Count: '{HostBlobs.Count}'");

			return new ValidationResult();
		}

		private void PopulateMessageFrame(Frame messageJsonFrame)
		{
			byte[] payload = messageJsonFrame.Payload;
			GetChunkedFileResponseMessage getChunkedFileResponseMessage = JsonMessageSerializer.Instance.DeSerialize<GetChunkedFileResponseMessage>(new MemoryStream(payload));
			StreamSignature[] Signatures = getChunkedFileResponseMessage.Signatures;
			foreach (var Signature in Signatures)
			{
				string streamId = Signature.StreamId;
				ChunkSignature[] chunkSignatures = Signature.ChunkSignatures;
				this.ChunkingSchemePerStreamId[ streamId ] = Signature.ChunkingScheme;

				if (this.HostBlobIdsPerStreamId.ContainsKey(streamId))
					continue;

				this.HostBlobIdsPerStreamId[streamId] = new List<string>();

				foreach (var chunkSignature in chunkSignatures)
				{
					string chunkId = chunkSignature.ChunkId;
					this.HostBlobIdsPerStreamId[streamId].Add(chunkId);
				}
			}

			// Populate content properties
			if (getChunkedFileResponseMessage.ContentProperties != null)
			{
				foreach (var contentProperty in getChunkedFileResponseMessage.ContentProperties)
				{
					this.ContentProperties[contentProperty.Name] = contentProperty;
				}
			}
		}

		private void PopulateHostFileChunkIdToBlobMapping(Frame chunkFrame)
		{
			byte[] payload = chunkFrame.Payload;
			byte[] extendedHeader = chunkFrame.ExtendedHeader;

			string framePayload = Encoding.UTF8.GetString(payload);
			string frameChunkId = Convert.ToBase64String(extendedHeader);

			if (this.HostBlobs.ContainsKey(frameChunkId))
				return;

			this.HostBlobs[frameChunkId] = new MemoryBlob(payload);
		}

		private ValidationResult ValidateFrame(Frame frame)
		{
			FrameType type = frame.Type;
			byte[] extendedHeader = frame.ExtendedHeader;
			byte[] payload = frame.Payload;

			switch (type)
			{
				case FrameType.MessageJSON:
					if (extendedHeader != null && extendedHeader.Length != Constants.FrameHeaderConstants.MessageJSON.ExtendedHeaderSizeInBytes)
						return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "MessageJson frame extended header is not {0}", Constants.FrameHeaderConstants.MessageJSON.ExtendedHeaderSizeInBytes));

					if (payload == null || payload.Length == 0)
						return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "MessageJson payload is empty"));

					string messageJsonPayload = Encoding.UTF8.GetString(payload);
					return ValidateJsonContent(messageJsonPayload);
				case FrameType.Chunk:
					if (extendedHeader == null || extendedHeader.Length != Constants.FrameHeaderConstants.Chunk.ExtendedHeaderSizeInBytes)
						return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "Chunk frame extended header length is not {0}", Constants.FrameHeaderConstants.Chunk.ExtendedHeaderSizeInBytes));

					string frameChunkIdFromPayload = new MemoryBlob(payload).BlobId;
					string frameChunkIdFromExtendedHeader = Convert.ToBase64String(extendedHeader);

					if (!string.Equals(frameChunkIdFromExtendedHeader, frameChunkIdFromPayload))
					{
						return new ValidationResult(String.Format(
							CultureInfo.CurrentCulture,
							"ChunkId calculated from extendedHeader:'{0}' is different from chunkId calculated from payload:{1}",
							frameChunkIdFromExtendedHeader,
							frameChunkIdFromPayload));
					}

					return new ValidationResult();
				case FrameType.ChunkRange:
					//TODO: Add support for other frame types
					return new ValidationResult();
				case FrameType.EndFrame:
					return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "Parsed frame list shouldn't include EndFrame"));
				default:
					return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "Unknown frame type: '{0}'", type.ToString()));
			}
		}

		private ValidationResult ValidateJsonContent(string jsonContent)
		{
			var errors = _schema.Validate(jsonContent);
			if (errors.Count == 0)
			{
				return new ValidationResult();
			}

			return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "MessageJson payload Json validation failed: {0}", jsonContent));
		}

		public class ContentStreamValidator
		{
			public ContentStreamValidator(
				string streamId,
				string expectedChunkingScheme,
				string expectedContent,
				string expectedContentResourceId,
				string alreadyExistingContent,
				string alreadyExistingContentResourceId)
			{
				ValidateParameters(
					streamId,
					expectedChunkingScheme,
					expectedContent,
					expectedContentResourceId,
					alreadyExistingContent,
					alreadyExistingContentResourceId);

				this.StreamId = streamId;
				this.ExpectedChunkingScheme = expectedChunkingScheme;
				this.ExpectedContent = expectedContent;
				this.ExpectedContentResourceId = expectedContentResourceId;
				this.AlreadyExistingContent = alreadyExistingContent;
				this.AlreadyExistingContentResourceId = alreadyExistingContentResourceId;
			}

			public string StreamId { get; private set; }
			public string ExpectedChunkingScheme { get; private set; }
			public string AlreadyExistingContent { get; private set; }
			public string AlreadyExistingContentResourceId { get; private set; }
			public string ExpectedContent { get; private set; }
			public string ExpectedContentResourceId { get; private set; }

			private void ValidateParameters(
				string streamId,
				string expectedChunkingScheme,
				string expectedContent,
				string expectedContentResourceId,
				string alreadyExistingContent,
				string alreadyExistingContentResourceId)
			{
				if (string.IsNullOrWhiteSpace(streamId))
					throw new ArgumentNullException($"{nameof(ContentStreamValidator)} cannot have empty streamId.");

				if (!Enum.TryParse(expectedChunkingScheme, true, out ChunkingScheme parsedExpectedChunkingScheme))
					throw new ArgumentException($"{nameof(expectedChunkingScheme)}' parameter can not be parsed to enum.");

				if (expectedChunkingScheme.Equals(ChunkingScheme.FullFile.ToString()))
				{
					if (expectedContent == null && alreadyExistingContent == null)
						return;

					if (expectedContent == null)
						throw new ArgumentNullException($"{nameof(expectedContent)} cannot be null.");

					if (alreadyExistingContent == null)
						throw new ArgumentNullException($"{nameof(alreadyExistingContent)} cannot be null.");

					if (expectedContentResourceId != null)
						throw new ArgumentNullException($"{nameof(expectedContentResourceId)} shouldn't be populated.");

					if (alreadyExistingContentResourceId != null)
						throw new ArgumentNullException($"{nameof(alreadyExistingContentResourceId)} shouldn't be populated.");
				}
				else if (expectedChunkingScheme.Equals(ChunkingScheme.Zip.ToString()))
				{
					if (expectedContentResourceId == null && alreadyExistingContentResourceId == null)
						return;

					if (expectedContentResourceId == null)
						throw new ArgumentNullException($"{nameof(expectedContentResourceId)} cannot be null.");

					if (alreadyExistingContentResourceId == null)
						throw new ArgumentNullException($"{nameof(alreadyExistingContentResourceId)} cannot be null.");

					if (expectedContent != null)
						throw new ArgumentNullException($"{nameof(expectedContent)} shouldn't be populated.");

					if (alreadyExistingContent != null)
						throw new ArgumentNullException($"{nameof(alreadyExistingContent)} shouldn't be populated.");
				}
			}

			public ValidationResult Validate(
				Dictionary<string, List<string>> hostBlobIdsPerStreamId,
				Dictionary<string, IBlob> hostBlobs,
				Dictionary<string, string> chunkingSchemePerStreamId,
				IResourceManager resourceManager)
			{
				if (!chunkingSchemePerStreamId[ this.StreamId ].Equals(this.ExpectedChunkingScheme))
					return new ValidationResult($"For StreamId '{this.StreamId}', ResponseChunkingScheme '{chunkingSchemePerStreamId[ this.StreamId ]}' not equal to ExpectedChunkingScheme: {this.ExpectedChunkingScheme}");

				if (!hostBlobIdsPerStreamId.ContainsKey(this.StreamId))
					return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "streamId:{0} does not exist in hostBlobIdsPerStreamId", this.StreamId));

				// Get the chunk ids of the file from download
				string[] hostBlobIds = hostBlobIdsPerStreamId[this.StreamId].ToArray();
				IReadOnlyDictionary<string, IBlob> existingBlobs = new Dictionary<string, IBlob>();

				if (chunkingSchemePerStreamId[ this.StreamId ].Equals(ChunkingScheme.Zip.ToString()))
				{
					// Zip Chunking
					if (AlreadyExistingContentResourceId == null && ExpectedContentResourceId == null)
						return new ValidationResult();

					ChunkProcessorFactory.Instance.CreateInstance(ChunkingScheme.Zip)
						.StreamToBlobs(resourceManager, AlreadyExistingContentResourceId, out string[] _, out existingBlobs);

					// Combine blobs into a complete stream
					IReadOnlyDictionary<string, IBlob> hostRevisionBlobs = CreateHostRevisionBlobs(hostBlobIds, hostBlobs, existingBlobs);
					byte[] reconstructedBytes = StreamBuilder.BlobsToBytes(hostBlobIds, hostRevisionBlobs);

					// Zip Chunking validation
					using (Stream reconstructedStream = new MemoryStream(reconstructedBytes))
					using (Stream expectedStream = resourceManager.GetZipChunkingResourceStream(ExpectedContentResourceId))
					{
						if (!StreamUtil.StreamEquals(expectedStream, reconstructedStream))
							return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "Reconstructed stream and expected stream are not equal."));
					}
				}
				else
				{
					// FullFile Chunking
					if (AlreadyExistingContent == null && ExpectedContent == null)
						return new ValidationResult();

					byte[] AlreadyExistingContentBytes = Encoding.UTF8.GetBytes(AlreadyExistingContent);

					ChunkProcessorFactory.Instance.CreateInstance(ChunkingScheme.FullFile)
						.StreamToBlobs(new MemoryStream(AlreadyExistingContentBytes), new BlobAllocator(), out string[] _, out existingBlobs);

					// Combine blobs into a complete stream
					IReadOnlyDictionary<string, IBlob> hostRevisionBlobs = CreateHostRevisionBlobs(hostBlobIds, hostBlobs, existingBlobs);
					byte[] reconstructedBytes = StreamBuilder.BlobsToBytes(hostBlobIds, hostRevisionBlobs);

					// Full File Chunking validation
					string reconstructedString = Encoding.UTF8.GetString(reconstructedBytes);

					if (!string.Equals(ExpectedContent, reconstructedString))
						return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "Reconstructed payload '{0}' and expected value '{1}' are not equal", reconstructedString, this.ExpectedContent));
				}
				
				return new ValidationResult();
			}

			private IReadOnlyDictionary<string, IBlob> CreateHostRevisionBlobs(
				string[] hostBlobIds,
				IReadOnlyDictionary<string, IBlob> hostBlobs,
				IReadOnlyDictionary<string, IBlob> existingBlobs)
			{
				Dictionary<string, IBlob> hostRevisionBlobs = new Dictionary<string, IBlob>();
				foreach (string hostBlobId in hostBlobIds)
				{
					IBlob blob;
					if (!hostBlobs.TryGetValue(hostBlobId, out blob) && !existingBlobs.TryGetValue(hostBlobId, out blob))
					{
						throw new ArgumentException($"Missing blob: {hostBlobId} in blob dictionary.");
					}

					if (!hostRevisionBlobs.ContainsKey(hostBlobId))
						hostRevisionBlobs.Add(hostBlobId, blob);
				}

				return hostRevisionBlobs;
			}
		}

		public class ContentPropertyValidator
		{
			public string Name { get; private set; }
			public bool ShouldBeReturned { get; private set; }
			public string ExpectedValue { get; private set; }
			public string ExpectedRetention { get; private set; }

			public ContentPropertyValidator(string name, string expectedValue, string expectedRetention, bool shouldBeReturned)
			{
				ValidateParameters(name);

				Name = name;
				ShouldBeReturned = shouldBeReturned;
				ExpectedValue = expectedValue;
				ExpectedRetention = expectedRetention;
			}

			private void ValidateParameters(string name)
			{
				if (string.IsNullOrWhiteSpace(name))
					throw new ArgumentNullException($"{nameof(ContentPropertyValidator)} cannot have empty name.");
			}

			public ValidationResult Validate(IReadOnlyDictionary<string, ContentProperty> contentProperties)
			{
				if (!ShouldBeReturned)
				{
					if (contentProperties.ContainsKey(Name))
					{
						return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "ContentProperty:{0} should not be returned", Name));
					}

					return new ValidationResult();
				}

				if (!contentProperties.TryGetValue(Name, out ContentProperty contentPropertyRead))
				{
					return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "ContentProperty:{0} does not exist", Name));
				}

				if (contentPropertyRead.Value != ExpectedValue || contentPropertyRead.Retention != ExpectedRetention)
				{
					return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "ContentProperty mismatch. Expected Value: {0}, Actual Value: {1}, Expected Retention: {2}, Actual Retention: {3}", ExpectedValue, contentPropertyRead.Value, ExpectedRetention, contentPropertyRead.Retention));
				}

				return new ValidationResult();
			}
		}
	}
}
