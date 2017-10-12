// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Validates that response code matches provided expected value.
	/// </summary>
	class ResponseCodeValidator : IValidator
	{
		private readonly int _expectedResponseCode;

		public ResponseCodeValidator(int expectedResponseCode)
		{
			_expectedResponseCode = expectedResponseCode;
		}

		public int ExpectedResponseCode
		{
			get { return _expectedResponseCode; }
		}

		public string Name { get { return "ResponseCodeValidator"; } }

		public ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState)
		{
			if (ExpectedResponseCode == data.StatusCode)
				return new ValidationResult();
			return
				new ValidationResult(string.Format("Incorrect StatusCode. Expected: {0}, Actual: {1}",
					_expectedResponseCode, data.StatusCode));
		}
	}
}
