// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Validates that response contains given header and it has expected value.
	/// </summary>
	class ResponseHeaderValidator : IValidator
	{
		public readonly string Key;
		public readonly string Comparator;
		public readonly string DefaultExpectedValue;
		public readonly string ExpectedStateKey;
		public readonly bool IsRequired;
		public readonly bool ShouldMatch;

		public ResponseHeaderValidator(string key, string expectedValue, string expectedStateKey, string comparator = "=", bool isRequired = true, bool shouldMatch = true)
		{
			Key = key;
			Comparator = comparator;
			DefaultExpectedValue = expectedValue;
			ExpectedStateKey = expectedStateKey;
			IsRequired = isRequired;
			ShouldMatch = shouldMatch;

			if (String.IsNullOrWhiteSpace(comparator))
				Comparator = "=";
		}

		public string Name
		{
			get { return "ResponseHeaderValidator"; }
		}

		public ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState)
		{
			string headerValue;

			if (!data.Headers.TryGetValue(Key, out headerValue))
			{
				if (IsRequired)
				{
					return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "'{0}' header is not present on the response", Key));
				}
				else
				{
					return new ValidationResult();
				}
			}

			// If the "ExpectedValue" and "ExpectedStateKey" attributes are non-empty on a Validator, then ExpectedStateKey will take precedence.
			// But if the mentioned "ExpectedStateKey" is invalid or doesn't have a saved state value, then the logic below will default to the value set in
			// "ExpectedValue" attribute of the Validator.
			string expectedValue = savedState != null &&
				ExpectedStateKey != null &&
				savedState.ContainsKey(ExpectedStateKey) &&
				!string.IsNullOrEmpty(savedState[ExpectedStateKey]) ? savedState[ExpectedStateKey] : DefaultExpectedValue;

			switch (this.Comparator)
			{
				case "=":
					return CheckEqual(headerValue, expectedValue);
				case ">":
				case "<":
				case ">=":
				case "<=":
					// Only support integer comparison
					return CheckNumericComparison(headerValue, this.Comparator, expectedValue);
				default:
					return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "Comparator '{0}' is not valid", this.Comparator));
			}
		}

		private ValidationResult CheckNumericComparison(string headerValue, string comparator, string expectedValue)
		{
			if (!int.TryParse(headerValue, out int parsedHeaderValue))
			{
				return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "'{0}' header is present, but its value '{1}' can not be parsed to integer", this.Key, headerValue));
			}

			if (!int.TryParse(expectedValue, out int parsedExpectedValue))
			{
				return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "expected value '{0}' can not be parsed to integer", expectedValue));
			}

			string assertionError = String.Format(CultureInfo.CurrentCulture, "headerVal:{0} {1} comparedValue:{2} comparison failed", parsedHeaderValue, comparator, parsedExpectedValue);

			switch (comparator)
			{
				case "<":
					if (parsedHeaderValue < parsedExpectedValue)
						return new ValidationResult();
					break;
				case "<=":
					if (parsedHeaderValue <= parsedExpectedValue)
						return new ValidationResult();
					break;
				case ">":
					if (parsedHeaderValue > parsedExpectedValue)
						return new ValidationResult();
					break;
				case ">=":
					if (parsedHeaderValue >= parsedExpectedValue)
						return new ValidationResult();
					break;
				default:
					return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "comparator:{0} shouldn't be in CheckNumericComparison method", comparator));
			}

			return new ValidationResult(assertionError);
		}

		private ValidationResult CheckEqual(string headerValue, string expectedValue)
		{
			if (expectedValue == null)
			{
				return new ValidationResult();
			}

			if (!ShouldMatch & string.IsNullOrEmpty(headerValue) & expectedValue == string.Empty)
			{
				return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "'{0}' header value should be any non empty string.",
					Key));
			}

			bool headerValueMatchesExpectedValue = String.Equals(headerValue, expectedValue, StringComparison.OrdinalIgnoreCase);

			if (ShouldMatch & !headerValueMatchesExpectedValue)
			{
				return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "'{0}' header value is different than expected. Expected: '{1}', Actual: '{2}'",
					Key, expectedValue ?? "[null]", headerValue ?? "[null]"));
			}
			else if (!ShouldMatch & headerValueMatchesExpectedValue)
			{
				return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "'{0}' header value should not be : '{1}'",
					Key, expectedValue ?? "[null]"));
			}
			else
			{
				return new ValidationResult();
			}
		}
	}
}
