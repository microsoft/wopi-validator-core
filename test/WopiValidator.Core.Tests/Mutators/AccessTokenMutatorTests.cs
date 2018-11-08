// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.Mutators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Office.WopiValidator.UnitTests.Mutators
{
	[TestClass]
	public class AccessTokenMutatorTests
	{
		[TestMethod]
		public void Mutate_MutationNotSpecifiedIsGuid_Succeeds()
		{
			// Arrange
			AccessTokenMutator mutator = new AccessTokenMutator(null);

			// Act
			string mutated = mutator.Mutate(null);

			// Assert
			Assert.IsNotNull(mutated);
			Guid guid;
			Assert.IsTrue(Guid.TryParse(mutated, out guid));
		}

		[TestMethod]
		public void Mutate_MutationSpecified_Succeeds()
		{
			// Arrange
			const string expectedMutation = "MutationValue";
			AccessTokenMutator mutator = new AccessTokenMutator(expectedMutation);

			// Act
			string mutated = mutator.Mutate(null);

			// Assert
			Assert.AreEqual(expectedMutation, mutated);
		}
	}
}
