// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
		public string RestrictedLink { get; set; }
	}

	public enum PutRelativeFileMode
	{
		Unknown = 0,
		Conflicting,
		Suggested,
		ExactName,
	}
}
