// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	/// <summary>
	/// Struct to store parameters required to build wopi requests.
	/// </summary>
	public struct WopiRequestParam
	{
		public string FileExtensionFilterList { get; set; }
		public string FolderName { get; set; }
		public string LockString { get; set; }
		public bool? LockUserVisible { get; set; }
		public IEnumerable<IMutator> Mutators { get; set; }
		public string NewLockString { get; set; }
		public string OldLockString { get; set; }
		public string OverrideUrl { get; set; }
		public bool? OverwriteRelative { get; set; }
		public PutRelativeFileMode PutRelativeFileMode { get; set; }
		public string RequestBody { get; set; }
		public string RequestedName { get; set; }
		public string ResourceId { get; set; }
		public IEnumerable<IStateEntry> StateSavers { get; set; }
		public IEnumerable<IValidator> Validators { get; set; }
		public string WopiSrc { get; set; }
		public string UrlType { get; set; }
		public CoauthLockType? CoauthLockType { get; set; }
		public string CoauthLockMetadata { get; set; }
		public CoauthLockMetadataEntity CoauthLockMetadataAsBody { get; set; }
		public string Lock { get; set; }
		public string CoauthLockId { get; set; }
		public string Editors { get; set; }
		public uint? CoauthLockExpirationTimeout { get; set; }
		public string CoauthTableVersion { get; set; }
		public string CoauthTableVersionStateKey { get; set; }
		public string SequenceNumber { get; set; }
		public string SequenceNumberStateKey { get; set; }
		public IEnumerable<XMLContentPropertyToReturn> ContentPropertiesToReturn { get; set; }
		public IEnumerable<XMLContentFilter> ContentFilters { get; set; }
		public IEnumerable<XMLContentProperty> ContentProperties { get; set; }
		public IEnumerable<XMLContentStream> ContentStreams { get; set; }
		public string UploadSessionTokenToCommit { get; set; }
		public uint? DelayTimeInSeconds { get; set; }
	}

	public enum PutRelativeFileMode
	{
		Unknown = 0,
		Conflicting,
		Suggested,
		ExactName,
	}

	public enum CoauthLockType
	{
		Coauth,
		CoauthExclusive,
		None
	}

	public class CoauthLockMetadataEntity
	{
		public string CoauthLockMetadata { get; set; }
	}
}
 

