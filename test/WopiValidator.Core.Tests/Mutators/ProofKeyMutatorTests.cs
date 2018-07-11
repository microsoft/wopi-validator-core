// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Office.WopiValidator.Core.Mutators;

namespace Microsoft.Office.WopiValidator.UnitTests.Mutators
{
	using System.Collections.Generic;
	using Core;

	[TestClass]
	public class ProofKeyMutatorTests
	{
		private const string CurrentProofKey = "currentProofKey";
		private const string OldProofKey = "oldProofKey";
		private const string MutatedWopiTimestampString = "2015-08-17T23:00:00";

		private static readonly string WopiTimestamp =
			DateTime.Parse("2015-08-17T21:00:00").Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture);
		private static readonly string MutatedWopiTimestamp =
			DateTime.Parse("2015-08-17T23:00:00").Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture);

		[TestMethod]
		public void Mutate_NullAttributes_NoMutation()
		{
			// Arrange
			const bool mutateCurrent = false;
			const bool mutateOld = false;
			const string timestamp = null;
			const string keyRelation = null; /* default synced */
			Dictionary<string, string> originalHeaders = CreateDefaultProofKeyHeaders();
			ProofKeyMutator mutator = new ProofKeyMutator(mutateCurrent, mutateOld, timestamp, keyRelation);

			// Act
			Dictionary<string, string> mutatedHeaders = mutator.Mutate(originalHeaders);

			// Assert
			Assert.AreEqual(CurrentProofKey, mutatedHeaders[Constants.Headers.ProofKey]);
			Assert.AreEqual(OldProofKey, mutatedHeaders[Constants.Headers.ProofKeyOld]);
			Assert.AreEqual(WopiTimestamp, mutatedHeaders[Constants.Headers.WopiTimestamp]);
		}

		[TestMethod]
		public void Mutate_SpecifiedAttributes_ExpectedMutation()
		{
			// Arrange
			const bool mutateCurrent = true;
			const bool mutateOld = true;
			const string keyRelation = null; 

			Dictionary<string, string> originalHeaders = CreateDefaultProofKeyHeaders();
			ProofKeyMutator mutator = new ProofKeyMutator(mutateCurrent, mutateOld, MutatedWopiTimestampString, keyRelation);

			// Act
			Dictionary<string, string> mutatedHeaders = mutator.Mutate(originalHeaders);

			// Assert
			Assert.AreEqual(ProofKeyMutator.InvalidBase64String, mutatedHeaders[Constants.Headers.ProofKey]);
			Assert.AreEqual(ProofKeyMutator.InvalidBase64String, mutatedHeaders[Constants.Headers.ProofKeyOld]);
			Assert.AreEqual(MutatedWopiTimestamp, mutatedHeaders[Constants.Headers.WopiTimestamp]);
		}

		[TestMethod]
		public void Mutate_KeyRelationAhead_ExpectedMutation()
		{ // Arrange
			const bool mutateCurrent = false;
			const bool mutateOld = false;
			const string mutatedWopiTimestamp = null;

			Dictionary<string, string> originalHeaders = CreateDefaultProofKeyHeaders();
			ProofKeyMutator mutator = new ProofKeyMutator(
				mutateCurrent,
				mutateOld,
				mutatedWopiTimestamp,
				ProofKeyMutator.KeyRelationType.Ahead);

			// Act
			Dictionary<string, string> mutatedHeaders = mutator.Mutate(originalHeaders);

			// Assert
			Assert.AreEqual(ProofKeyMutator.InvalidBase64String, mutatedHeaders[Constants.Headers.ProofKey]);
			Assert.AreEqual(CurrentProofKey, mutatedHeaders[Constants.Headers.ProofKeyOld]);
			Assert.AreEqual(WopiTimestamp, mutatedHeaders[Constants.Headers.WopiTimestamp]);
		}

		[TestMethod]
		public void Mutate_KeyRelationBehind_ExpectedMutation()
		{
			// Arrange
			const bool mutateCurrent = false;
			const bool mutateOld = false;

			Dictionary<string, string> originalHeaders = CreateDefaultProofKeyHeaders();
			ProofKeyMutator mutator = new ProofKeyMutator(
				mutateCurrent,
				mutateOld,
				MutatedWopiTimestampString,
				ProofKeyMutator.KeyRelationType.Behind);

			// Act
			Dictionary<string, string> mutatedHeaders = mutator.Mutate(originalHeaders);

			// Assert
			Assert.AreEqual(OldProofKey, mutatedHeaders[Constants.Headers.ProofKey]);
			Assert.AreEqual(ProofKeyMutator.InvalidBase64String, mutatedHeaders[Constants.Headers.ProofKeyOld]);
			Assert.AreEqual(MutatedWopiTimestamp, mutatedHeaders[Constants.Headers.WopiTimestamp]);
		}

		[TestMethod]
		public void Mutate_KeyRelationAheadOverridesSpecifiedMutation_ExpectedMutationRetainsTimestampOverride()
		{
			// Arrange
			const bool mutateCurrent = true;
			const bool mutateOld = true;

			Dictionary<string, string> originalHeaders = CreateDefaultProofKeyHeaders();
			ProofKeyMutator mutator = new ProofKeyMutator(
				mutateCurrent,
				mutateOld,
				MutatedWopiTimestampString,
				ProofKeyMutator.KeyRelationType.Ahead);

			// Act
			Dictionary<string, string> mutatedHeaders = mutator.Mutate(originalHeaders);

			// Assert
			Assert.AreEqual(ProofKeyMutator.InvalidBase64String, mutatedHeaders[Constants.Headers.ProofKey]);
			Assert.AreEqual(CurrentProofKey, mutatedHeaders[Constants.Headers.ProofKeyOld]);
			Assert.AreEqual(MutatedWopiTimestamp, mutatedHeaders[Constants.Headers.WopiTimestamp]);
		}

		[TestMethod]
		public void Mutate_KeyRelationBehindOverridesSpecifiedMutation_ExpectedMutationRetainsTimestampOverride()
		{
			// Arrange
			const bool mutateCurrent = true;
			const bool mutateOld = true;

			Dictionary<string, string> originalHeaders = CreateDefaultProofKeyHeaders();
			ProofKeyMutator mutator = new ProofKeyMutator(
				mutateCurrent,
				mutateOld,
				MutatedWopiTimestampString,
				ProofKeyMutator.KeyRelationType.Behind);

			// Act
			Dictionary<string, string> mutatedHeaders = mutator.Mutate(originalHeaders);

			// Assert
			Assert.AreEqual(OldProofKey, mutatedHeaders[Constants.Headers.ProofKey]);
			Assert.AreEqual(ProofKeyMutator.InvalidBase64String, mutatedHeaders[Constants.Headers.ProofKeyOld]);
			Assert.AreEqual(MutatedWopiTimestamp, mutatedHeaders[Constants.Headers.WopiTimestamp]);
		}

		private static Dictionary<string, string> CreateDefaultProofKeyHeaders()
		{
			return new Dictionary<string, string>(3)
			{
				{ Constants.Headers.ProofKey, CurrentProofKey },
				{ Constants.Headers.ProofKeyOld, OldProofKey },
				{ Constants.Headers.WopiTimestamp, WopiTimestamp }
			};
		}
	}
}
