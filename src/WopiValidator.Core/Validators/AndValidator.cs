// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Base class for validators that use other validators to perform validation
	/// (e.g. ResponeCode + ResponseHeader combination for LockMismatch) where
	/// Conjunction validator validates if all the combined validators also validate.
	/// </summary>
	class AndValidator : CombinationValidator
	{
		public AndValidator(IValidator[] validators) : base(validators) {}

		public override string Name
		{
			get
			{
				return "And";
			}
		}

		protected override ValidationResult CombineValidationResults(ValidationResult[] results)
		{
			string[] errors = results.SelectMany(r => r.Errors).ToArray();

			if (errors.Length == 0)
				return new ValidationResult();

			return new ValidationResult(errors);
		}
	}
}
