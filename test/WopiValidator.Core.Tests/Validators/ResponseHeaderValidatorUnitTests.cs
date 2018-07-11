// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Office.WopiValidator.Core;
using Microsoft.Office.WopiValidator.Core.Validators;

namespace Microsoft.Office.WopiValidator.UnitTests.Validators
{
	[TestClass]
	public class ResponseHeaderValidatorUnitTests
	{
		[TestMethod]
		public void Validate_HeaderValueSameAsExpectedValue_ShouldMatchIsTrue_ValidationSucceeds()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const string actualValue = expectedValue;
			const bool shouldMatch = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderValueSameAsExpectedValue_ShouldMatchIsFalse_ValidationFails()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const string actualValue = expectedValue;
			const bool shouldMatch = false;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderKeyDoesntExist_IsRequiredIsTrue_ValidationFails()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const bool shouldMatch = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, true, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary()
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderKeyDoesntExist_IsRequiredIsFalse_ValidationSucceeds()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const bool shouldMatch = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary()
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderValueDifferentThanExpectedValue_ShouldMatchIsTrue_ValidationFails()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const string actualValue = "differentValue";
			const bool shouldMatch = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderValueDifferentThanExpectedValue_ShouldMatchIsFalse_ValidationSucceeds()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const string actualValue = "differentValue";
			const bool shouldMatch = false;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderValueDifferentThanExpectedValue_ShouldMatchIsTrue_OneErrorReturned()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const string actualValue = "differentValue";
			const bool shouldMatch = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsNotNull(validationResult.Errors);
			Assert.AreEqual(1, validationResult.Errors.Count());
		}

		[TestMethod]
		public void Validate_HeaderValueDifferentThanExpectedValue_ShouldMatchIsTrue_ErrorMessageContainsHeaderKey()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const string actualValue = "differentValue";
			const bool shouldMatch = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Contains(headerKey));
		}

		[TestMethod]
		public void Validate_HeaderValueDifferentThanExpectedValue_ShouldMatchIsTrue_ErrorMessageContainsActualHeaderValues()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const string actualValue = "differentValue";
			const bool shouldMatch = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Contains(actualValue));
		}

		[TestMethod]
		public void Validate_HeaderValueDifferentThanExpectedValue_ShouldMatchIsTrue_ErrorMessageContainsExpectedHeaderValues()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const string actualValue = "differentValue";
			const bool shouldMatch = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Contains(expectedValue));
		}

		[TestMethod]
		public void Validate_HeaderValueSameAsExpectedValue_ShouldMatchIsFalse_OneErrorReturned()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const string actualValue = expectedValue;
			const bool shouldMatch = false;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsNotNull(validationResult.Errors);
			Assert.AreEqual(1, validationResult.Errors.Count());
		}

		[TestMethod]
		public void Validate_HeaderValueSameAsExpectedValue_ShouldMatchIsFalse_ErrorMessageContainsHeaderKey()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const string actualValue = expectedValue;
			const bool shouldMatch = false;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Contains(headerKey));
		}

		[TestMethod]
		public void Validate_HeaderValueSameAsExpectedValue_ShouldMatchIsFalse_ErrorMessageContainsExpectedHeaderValue()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "testvalue";
			const string actualValue = expectedValue;
			const bool shouldMatch = false;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Contains(expectedValue));
		}

		[TestMethod]
		public void Validate_HeaderValueCaseDifferent_Succeeds()
		{
			// Arrange
			const string headerKey = "key";
			const string expectedValue = "value";
			const string actualValue = "Value";
			const bool shouldMatch = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { {headerKey, actualValue} }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_DifferentCaseHeaderName_ValidationSucceeds()
		{
			// Arrange
			const string headerKey = "key";
			const string differentCaseHeaderKey = "Key";
			const string expectedValue = "value";
			const string actualValue = expectedValue;
			const bool shouldMatch = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, null, false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { differentCaseHeaderKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderAndExpectedValueAreNull_ShouldMatchAndIsRequiredAreTrue_ValidationSucceeds()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = null;
			const bool shouldMatch = true;
			const bool isRequired = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, null, null, isRequired, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderAndExpectedValueAreNull_ShouldMatchAndIsRequiredAreFalse_ValidationSucceeds()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = null;
			const bool shouldMatch = false;
			const bool isRequired = false;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, null, null, isRequired, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderValueIsNullAndExpectedValueIsNotNull_ShouldMatchAndIsRequiredAreTrue_ErrorMessageContainsHeaderKey()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = null;
			const bool shouldMatch = true;
			const bool isRequired = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, "value", null, isRequired, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Contains(headerKey));
		}

		[TestMethod]
		public void Validate_HeaderValueIsEmptyAndExpectedValueIsNull_ShouldMatchAndIsRequiredAreTrue_ValidationSucceeds()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = "";
			const bool shouldMatch = true;
			const bool isRequired = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, null, null, isRequired, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderValueIsEmptyAndExpectedValueIsNull_ShouldMatchAndIsRequiredAreFalse_ValidationSucceeds()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = "";
			const bool shouldMatch = false;
			const bool isRequired = false;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, null, null, isRequired, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderAndExpectedValueAreEmpty_ShouldMatchAndIsRequiredAreTrue_ValidationSucceeds()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = "";
			const bool shouldMatch = true;
			const bool isRequired = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, "", null, isRequired, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_HeaderValueIsNonEmptyAndExpectedValueIsEmpty_ShouldMatchAndIsRequiredAreTrue_ErrorMessageContainsHeaderKey()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = "value";
			const bool shouldMatch = true;
			const bool isRequired = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, "", null, isRequired, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Contains(headerKey));
		}

		[TestMethod]
		public void Validate_HeaderAndExpectedValueAreEmpty_ShouldMatchIsFalseAndIsRequiredIsTrue_ErrorMessageContainsHeaderKey()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = "";
			const bool shouldMatch = false;
			const bool isRequired = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, "", null, isRequired, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Equals("'key' header value should be any non empty string."));
		}

		[TestMethod]
		public void Validate_HeaderValueIsNullAndExpectedValueIsEmpty_ShouldMatchIsFalseAndIsRequiredIsTrue_ErrorMessageContainsHeaderKey()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = null;
			const bool shouldMatch = false;
			const bool isRequired = true;
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, "", null, isRequired, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Equals("'key' header value should be any non empty string."));
		}

		[TestMethod]
		public void Validate_HeaderValueDifferentThanExpectedStateValue_ShouldMatchIsTrue_ErrorMessageContainsExpectedStateValue()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = "value";
			const string expectedValue = actualValue;
			const bool shouldMatch = true;
			Dictionary<string, string> savedState = new Dictionary<string, string>() { {"StateKey", "incorrect value" } };
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, "StateKey", false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Contains(savedState["StateKey"]));
		}

		[TestMethod]
		public void Validate_HeaderValueDifferentThanExpectedValueAndIncorrectExpectedStateKey_ShouldMatchIsTrue_ErrorMessageContainsExpectedStateValue()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = "value";
			const string expectedValue = "incorrect expected value";
			const bool shouldMatch = true;
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "value" } };
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, expectedValue, "IncorrectStateKey", false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Contains(expectedValue));
		}

		[TestMethod]
		public void Validate_HeaderValueSameAsExpectedStateValue_ShouldMatchIsTrue_ValidationSucceeds()
		{
			// Arrange
			const string headerKey = "key";
			const string actualValue = "value";
			const bool shouldMatch = true;
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", actualValue } };
			ResponseHeaderValidator validator = new ResponseHeaderValidator(headerKey, null, "StateKey", false, shouldMatch);

			ResponseDataMock responseData = new ResponseDataMock
			{
				Headers = new CaseInsensitiveDictionary() { { headerKey, actualValue } }
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}
	}
}
