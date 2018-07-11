// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Office.WopiValidator.Core;
using Microsoft.Office.WopiValidator.Core.Validators;

namespace Microsoft.Office.WopiValidator.UnitTests.Validators
{
	[TestClass]
	public class ResponseCodeValidatorUnitTests
	{
		[TestMethod]
		public void Validate_ResponseCodeAsExpected_ValidationSucceeds()
		{
			// Arrange
			const int expectedResponseCode = 200;
			const int actualResponseCode = 200;
			ResponseCodeValidator validator = new ResponseCodeValidator(expectedResponseCode);
			ResponseDataMock responseData = new ResponseDataMock { StatusCode = actualResponseCode };

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ResponseCodeDifferentThanExpected_ValidationFails()
		{
			// Arrange
			const int expectedResponseCode = 200;
			const int actualResponseCode = 300;
			ResponseCodeValidator validator = new ResponseCodeValidator(expectedResponseCode);
			ResponseDataMock responseData = new ResponseDataMock { StatusCode = actualResponseCode };

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ResponseCodeDifferentThanExpected_OneErrorReturned()
		{
			// Arrange
			const int expectedResponseCode = 200;
			const int actualResponseCode = 300;
			ResponseCodeValidator validator = new ResponseCodeValidator(expectedResponseCode);
			ResponseDataMock responseData = new ResponseDataMock { StatusCode = actualResponseCode };

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsNotNull(validationResult.Errors);
			Assert.AreEqual(1, validationResult.Errors.Count());
		}

		[TestMethod]
		public void Validate_ResponseCodeDifferentThanExpected_ErrorMessageContainsActualResponseCode()
		{
			// Arrange
			const int expectedResponseCode = 200;
			const int actualResponseCode = 300;
			ResponseCodeValidator validator = new ResponseCodeValidator(expectedResponseCode);
			ResponseDataMock responseData = new ResponseDataMock { StatusCode = actualResponseCode };

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Contains(actualResponseCode.ToString(CultureInfo.InvariantCulture)));
		}

		[TestMethod]
		public void Validate_ResponseCodeDifferentThanExpected_ErrorMessageContainsExpectedResponseCode()
		{
			// Arrange
			const int expectedResponseCode = 200;
			const int actualResponseCode = 300;
			ResponseCodeValidator validator = new ResponseCodeValidator(expectedResponseCode);
			ResponseDataMock responseData = new ResponseDataMock { StatusCode = actualResponseCode };

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			string error = validationResult.Errors.First();
			Assert.IsTrue(error.Contains(expectedResponseCode.ToString(CultureInfo.InvariantCulture)));
		}
	}
}
