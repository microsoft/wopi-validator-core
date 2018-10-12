// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Office.WopiValidator.Core.Mutators
{
	/// <summary>
	/// Mutator used to mutate proof key related headers sent along with WOPI requests.
	///
	/// If MutateCurrent is true, X-WOPI-Proof is set to a known-invalid base-64 encoded string.
	/// If MutateOld is true, X-WOPI-ProofOld is set to a known-invalid base-64 encoded string.
	/// If KeyRelation is Synced, MutateCurrent and MutateOld are respected.
	/// If KeyRelation is non-Synced, MutateCurrent and MutateOld even if specified are overridden.
	/// If KeyRelation is Ahead, it is for the scenario where the WOPI client (e.g. Office Online) has
	///    published new public keys but hosts have not synced.
	/// If KeyRelation is Behind, it is for the scenario where the WOPI client (e.g. Office Online) has
	///    published new public keys and hosts have synced, but the datacenter machine making the WOPI
	///    request still has the old public keys.
	/// </summary>
	public class ProofKeyMutator : IMutator
	{
		public readonly bool MutateCurrent;
		public readonly bool MutateOld;
		public readonly long? WopiTimestamp;
		public readonly string KeyRelation;

		public static readonly string InvalidBase64String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("INVALID"));

		public ProofKeyMutator(
			bool mutateCurrent,
			bool mutateOld,
			string timestamp,
			string keyRelation)
		{
			this.MutateCurrent = mutateCurrent;
			this.MutateOld = mutateOld;
			this.WopiTimestamp = DateTime.TryParse(
					timestamp,
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out DateTime wopiTimestamp)
				? wopiTimestamp.Ticks : (long?)null;
			this.KeyRelation = keyRelation ?? KeyRelationType.Synced;
		}

		public string Name
		{
			get { return "ProofKeyMutator"; }
		}


		public Dictionary<string, string> Mutate(
			Dictionary<string, string> originalProofKeyHeaders,
			Func<long, Dictionary<string, string>> proofKeyGeneration)
		{
			Dictionary<string, string> proofKeyHeaders;

			if (WopiTimestamp != null && proofKeyGeneration != null)
			{
				proofKeyHeaders = proofKeyGeneration(WopiTimestamp.Value);
			}
			else
			{
				proofKeyHeaders = new Dictionary<string, string>(3);
				proofKeyHeaders[Constants.Headers.WopiTimestamp] = originalProofKeyHeaders[Constants.Headers.WopiTimestamp];
			}
			
			switch (KeyRelation)
			{
				case KeyRelationType.Synced:
					proofKeyHeaders[Constants.Headers.ProofKey] = MutateCurrent
						? InvalidBase64String : originalProofKeyHeaders[Constants.Headers.ProofKey];
					proofKeyHeaders[Constants.Headers.ProofKeyOld] = MutateOld
						? InvalidBase64String : originalProofKeyHeaders[Constants.Headers.ProofKeyOld];
					break;
				case KeyRelationType.Behind:
					proofKeyHeaders[Constants.Headers.ProofKey] = originalProofKeyHeaders[Constants.Headers.ProofKeyOld];
					proofKeyHeaders[Constants.Headers.ProofKeyOld] = InvalidBase64String;
					break;
				case KeyRelationType.Ahead:
					proofKeyHeaders[Constants.Headers.ProofKeyOld] = originalProofKeyHeaders[Constants.Headers.ProofKey];
					proofKeyHeaders[Constants.Headers.ProofKey] = InvalidBase64String;
					break;
			}

			return proofKeyHeaders;
		}

		public class KeyRelationType
		{
			public const string Synced = "Synced";
			public const string Ahead = "Ahead";
			public const string Behind = "Behind";
		}
	}
}
