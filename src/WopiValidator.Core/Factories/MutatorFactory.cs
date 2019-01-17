// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.Mutators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	static class MutatorFactory
	{
		/// <summary>
		/// Parse mutator information for a request.
		/// </summary>
		public static IEnumerable<IMutator> GetMutators(XElement definition)
		{
			return definition.Elements().Select(GetMutator);
		}

		/// <summary>
		/// Parse a single mutator information and instantiate an IMutator instance.
		/// </summary>
		private static IMutator GetMutator(XElement definition)
		{
			string elementName = definition.Name.LocalName;

			switch (elementName)
			{
				case Constants.Mutators.AccessToken:
					return GetAccessTokenMutator(definition);
				case Constants.Mutators.ProofKey:
					return GetProofKeyMutator(definition);
				case Constants.Mutators.Id:
					return GetIdMutator(definition);
				default:
					throw new ArgumentException(string.Format("Unknown mutator: '{0}'", elementName));
			}
		}

		private static IMutator GetAccessTokenMutator(XElement definition)
		{
			string mutation = (string)definition.Attribute("Mutation");
			return new AccessTokenMutator(mutation);
		}

		private static IMutator GetIdMutator(XElement definition)
		{
			string mutation = (string)definition.Attribute("Mutation");
			return new IdMutator(mutation);
		}

		private static IMutator GetProofKeyMutator(XElement definition)
		{
			bool mutateCurrent = ((bool?)definition.Attribute("MutateCurrent")) ?? false;
			bool mutateOld = ((bool?)definition.Attribute("MutateOld")) ?? false;
			string timestamp = (string)definition.Attribute("Timestamp");
			string keyRelation = (string)definition.Attribute("KeyRelation");

			return new ProofKeyMutator(mutateCurrent, mutateOld, timestamp, keyRelation);
		}
	}
}
