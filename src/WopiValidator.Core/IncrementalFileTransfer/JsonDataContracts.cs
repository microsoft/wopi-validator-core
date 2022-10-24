using System;
using System.Runtime.Serialization;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	[DataContract]
	internal class GetChunkedFileRequestMessage
	{
		/// <summary>
		/// An array of content property names to be read from the host
		/// </summary>
		[DataMember]
		public string[] ContentPropertiesToReturn { get; set; }

		/// <summary>
		/// List of filters one per stream to control what blobs are returned by the host
		/// </summary>
		[DataMember]
		public ContentFilter[] ContentFilters { get; set; }

		public GetChunkedFileRequestMessage()
		{
			ContentPropertiesToReturn = new string[0];
			ContentFilters = new ContentFilter[0];
		}

		public GetChunkedFileRequestMessage(
			string[] ContentPropertiesToReturn,
			ContentFilter[] ContentFilters)
		{
			this.ContentPropertiesToReturn = ContentPropertiesToReturn;
			this.ContentFilters = ContentFilters;
		}

		public override string ToString()
		{
			return string.Format(
				"ContentPropertiesToReturn: {0} , ContentFilters: {1}",
				string.Join(",", ContentPropertiesToReturn),
				String.Join<ContentFilter>(",", ContentFilters));
		}
	}

	[DataContract]
	internal class ContentFilter
	{
		/// <summary>
		/// The chunking scheme used by client to shred the stream 
		/// </summary>
		[DataMember]
		public string ChunkingScheme { get; set; }

		/// <summary>
		/// Identifier of the stream for which this filter applies 
		/// Only Streams listed would be returned by the host 
		/// </summary>
		[DataMember]
		public string StreamId { get; set; }

		/// <summary>
		/// string specifying what all chunks to return 
		/// </summary>
		[DataMember]
		public string ChunksToReturn { get; set; }

		/// <summary>
		/// List of chunk ids (Spooky hash) which the client already has and doesn't want the host to send back
		/// This filter is used on top of ChunksToReturn filter. e.g. even if the ChunksToReturn says all, AlreadyKnownChunks would not be returned
		/// </summary>
		[DataMember]
		public string[] AlreadyKnownChunks { get; set; }

		public ContentFilter()
		{
			ChunkingScheme = string.Empty;
			StreamId = string.Empty;
			ChunksToReturn = string.Empty;
			AlreadyKnownChunks = new string[0];
		}

		public ContentFilter(
			string ChunkingScheme,
			string StreamId,
			string ChunksToReturn,
			string[] AlreadyKnownChunks)
		{
			this.ChunkingScheme = ChunkingScheme;
			this.StreamId = StreamId;
			this.ChunksToReturn = ChunksToReturn;
			this.AlreadyKnownChunks = AlreadyKnownChunks;
		}

		public override string ToString()
		{
			return string.Format(
				"ChunkingScheme: {0}, StreamId: {1}, ChunksToReturn: {2}, AlreadyKnownChunks: {3}",
				ChunkingScheme,
				StreamId,
				ChunksToReturn,
				string.Join(",", AlreadyKnownChunks));
		}
	}

	[DataContract]
	internal class GetChunkedFileResponseMessage
	{
		[DataMember]
		public ContentProperty[] ContentProperties { get; set; }

		/// <summary>
		/// List of stream signatures, one per stream
		/// </summary>
		[DataMember]
		public StreamSignature[] Signatures { get; set; }

		public GetChunkedFileResponseMessage()
		{
			ContentProperties = new ContentProperty[0];
			Signatures = new StreamSignature[0];
		}

		public override string ToString()
		{
			return string.Format(
				"ContentProperties: {0} , Signatures: {1}",
				String.Join<ContentProperty>(",", ContentProperties),
				String.Join<StreamSignature>(",", Signatures));
		}
	}

	[DataContract]
	internal class PutChunkedFileRequestMessage
	{
		[DataMember]
		public ContentProperty[] ContentProperties { get; set; }

		/// <summary>
		/// List of stream signatures, one per stream
		/// </summary>
		[DataMember]
		public StreamSignature[] Signatures { get; set; }

		/// <summary>
		/// Token for the upload session whose content should be used to commit this request
		/// </summary>
		[DataMember]
		public string UploadSessionTokenToCommit { get; set; }

		public PutChunkedFileRequestMessage()
		{
			ContentProperties = new ContentProperty[0];
			Signatures = new StreamSignature[0];
			UploadSessionTokenToCommit = string.Empty;
		}

		public PutChunkedFileRequestMessage(ContentProperty[] ContentProperties, StreamSignature[] Signatures, string UploadSessionTokenToCommit)
		{
			this.ContentProperties = ContentProperties;
			this.Signatures = Signatures;
			this.UploadSessionTokenToCommit = UploadSessionTokenToCommit;
		}

		public override string ToString()
		{
			return string.Format(
				"ContentProperties: {0} , Signatures: {1} , UploadSessionTokenToCommit: {2}",
				String.Join<ContentProperty>(",", ContentProperties),
				String.Join<StreamSignature>(",", Signatures),
				UploadSessionTokenToCommit);
		}
	}

	[DataContract]
	internal class ContentProperty
	{
		[DataMember] public string Name { get; set; }

		[DataMember] public string Value { get; set; }

		// Indicates whether the content property should (or should not) be retained after document changes
		[DataMember] public string Retention { get; set; }

		public ContentProperty()
		{
			Name = String.Empty;
			Value = String.Empty;
			Retention = ContentPropertyRetention.DeleteOnContentChange.ToString();
		}

		public ContentProperty(string Name, string Value, string Retention)
		{
			this.Name = Name;
			this.Value = Value;
			this.Retention = Retention;
		}

		public override string ToString()
		{
			return string.Format(
				"Name: {0}, Value: {1}, Retention: {2}",
				Name,
				Value,
				Retention);
		}
	}

	[DataContract]
	internal class StreamSignature
	{
		/// <summary>
		/// The chunking scheme used to shred the stream into chunks
		/// </summary>
		[DataMember]
		public string ChunkingScheme { get; set; }

		/// <summary>
		/// Identifier of the stream
		/// </summary>
		[DataMember]
		public string StreamId { get; set; }

		/// <summary>
		/// Ordered list of chunk signatures for a given stream
		/// </summary>
		[DataMember]
		public ChunkSignature[] ChunkSignatures { get; set; }

		public StreamSignature()
		{
			ChunkingScheme = string.Empty;
			StreamId = string.Empty;
			ChunkSignatures = new ChunkSignature[0];
		}

		public StreamSignature(string ChunkingScheme, string StreamId, ChunkSignature[] ChunkSignatures)
		{
			this.ChunkingScheme = ChunkingScheme;
			this.StreamId = StreamId;
			this.ChunkSignatures = ChunkSignatures;
		}

		public override string ToString()
		{
			return string.Format(
				"ChunkingScheme: {0}, StreamId: {1}, ChunkSignatures: {2}",
				ChunkingScheme,
				StreamId,
				String.Join<ChunkSignature>(",", ChunkSignatures));
		}
	}

	[DataContract]
	internal class ChunkSignature
	{
		[DataMember] public string ChunkId { get; set; }

		[DataMember] public ulong Length { get; set; }

		public ChunkSignature()
		{
			ChunkId = String.Empty;
			Length = 0;
		}

		public ChunkSignature(string ChunkId, ulong Length)
		{
			this.ChunkId = ChunkId;
			this.Length = Length;
		}

		public override string ToString()
		{
			return string.Format(
				"ChunkId: {0}, Length: {1}",
				ChunkId,
				Length);
		}
	}


	[DataContract]
	internal class WopiCoauthLockMetadata
	{
		/// <summary>
		/// A string set by client when requesting the lock
		/// </summary>
		[DataMember]
		public string CoauthLockMetadata { get; set; }

		public WopiCoauthLockMetadata()
		{
			CoauthLockMetadata = string.Empty;
		}

		public override string ToString()
		{
			return string.Format("CoauthLockMetadata: {0}", CoauthLockMetadata);
		}
	}
}
