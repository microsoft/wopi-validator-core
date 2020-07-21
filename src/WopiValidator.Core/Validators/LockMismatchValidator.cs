// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Validates that response is equivalent to LockMismatch with expected actual lock string provided.
	/// </summary>
	class LockMismatchValidator : AndValidator
	{
		public LockMismatchValidator(string expectedLock):
			base(GetValidators(expectedLock))
		{
		}

		private static IValidator[] GetValidators(string expectedLock)
		{
			return new IValidator[] {
				new ResponseCodeValidator(409),
				new ResponseHeaderValidator(Constants.Headers.Lock, expectedLock, null, isRequired: !string.IsNullOrEmpty(expectedLock))
			};
		}

		public override string Name
		{
			get { return "LockMismatchValidator"; }
		}
	}
}
