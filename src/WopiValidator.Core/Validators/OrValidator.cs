// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Base class for validators that use other validators to perform validation
	/// (e.g. ResponeCode + ResponseCode combination for MultipleValidResponseCode) where
	/// Disjunction validator validates if at least one of combined validators validate.
	/// </summary>
	class OrValidator : CombinationValidator
	{
		public OrValidator(IValidator[] validators) : base(validators) {}

		public override string Name
		{
			get
			{
				return "Or";
			}
		}

		protected override ValidationResult CombineValidationResults(ValidationResult[] results)
		{
			if (results.Any(r => !r.HasFailures))
				return new ValidationResult();

			string[] errors = results.SelectMany(r => r.Errors).ToArray();
			return new ValidationResult(errors);
		}
	}
}
