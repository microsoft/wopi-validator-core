// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Base class for validators that use other validators to perform validation
	/// (e.g. ResponeCode + ResponseHeader combination for LockMismatch).
	/// </summary>
	abstract class CombinationValidator : IValidator
	{
		private readonly IEnumerable<IValidator> _validators;

		protected CombinationValidator(params IValidator[] validators)
		{
			_validators = validators;
		}

		public abstract string Name { get; }

		public ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState)
		{
			ValidationResult[] results = _validators.Select(v => v.Validate(data, resourceManager, savedState)).ToArray();

			return CombineValidationResults(results);
		}

		protected abstract ValidationResult CombineValidationResults(ValidationResult[] results);
	}
}
