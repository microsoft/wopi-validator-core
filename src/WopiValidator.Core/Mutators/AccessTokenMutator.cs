// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Office.WopiValidator.Core.Mutators
{
	/// <summary>
	/// Mutator used to mutate the access token of a WOPI request.
	/// </summary>
	public class AccessTokenMutator : IMutator
	{
		public AccessTokenMutator(string mutation)
		{
			this.Mutation = mutation;
		}

		public string Mutation { get; private set; }

		public string Name
		{
			get { return "AccessTokenMutator"; }
		}

		public string Mutate(string original)
		{
			return Mutation ?? Guid.NewGuid().ToString();
		}
	}
}
