// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Office.WopiValidator.Core.Mutators
{
	/// <summary>
	/// Mutator used to mutate the id of a WOPI request.
	/// </summary>
	public class IdMutator : IMutator
	{
		public IdMutator(string mutation)
		{
			this.Mutation = mutation;
		}

		public string Mutation { get; private set; }

		public string Name
		{
			get { return "IdMutator"; }
		}

		public string Mutate(string original)
		{
			return Mutation ?? Guid.NewGuid().ToString();
		}
	}
}
