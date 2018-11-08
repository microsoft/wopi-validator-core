// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core;
using Microsoft.Office.WopiValidator.Core.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Office.WopiValidator.UnitTests.Validators
{
	[TestClass]
	public class OrValidatorTests
	{
		[TestMethod]
		public void Validate_OneValidatorValidation_Succeeds()
		{
			//Arrange
			const int actualResponseCode = 200;
			int expectedResponseCode = 200;
			IValidator validator = new ResponseCodeValidator(expectedResponseCode);
			IValidator orValidator = new OrValidator(new IValidator[] { validator });
			ResponseDataMock responseData = new ResponseDataMock { StatusCode = actualResponseCode };

			//Test
			ValidationResult result = orValidator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_OneValidatorValidation_Fails()
		{
			//Arrange
			const int actualResponseCode = 200;
			int expectedResponseCode = 400;
			IValidator validator = new ResponseCodeValidator(expectedResponseCode);
			IValidator orValidator = new OrValidator(new IValidator[] { validator });
			ResponseDataMock responseData = new ResponseDataMock { StatusCode = actualResponseCode };

			//Test
			ValidationResult result = orValidator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_MultipleValidatorsFirstValid_Succeeds()
		{
			//Arrange
			const int actualResponseCode = 200;
			IValidator[] validators = new ResponseCodeValidator[] {
				new ResponseCodeValidator(200),
				new ResponseCodeValidator(400)
			};
			IValidator orValidator = new OrValidator(validators);
			ResponseDataMock responseData = new ResponseDataMock { StatusCode = actualResponseCode };

			//Test
			ValidationResult result = orValidator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_MultipleValidatorsSecondValid_Succeeds()
		{
			//Arrange
			const int actualResponseCode = 200;
			IValidator[] validators = new ResponseCodeValidator[] {
				new ResponseCodeValidator(400),
				new ResponseCodeValidator(200)
			};
			IValidator orValidator = new OrValidator(validators);
			ResponseDataMock responseData = new ResponseDataMock { StatusCode = actualResponseCode };

			//Test
			ValidationResult result = orValidator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_MultipleValidatorsAllInvalid_Fails()
		{
			//Arrange
			const int actualResponseCode = 200;
			IValidator[] validators = new ResponseCodeValidator[] {
				new ResponseCodeValidator(400),
				new ResponseCodeValidator(401)
			};
			IValidator orValidator = new OrValidator(validators);
			ResponseDataMock responseData = new ResponseDataMock { StatusCode = actualResponseCode };

			//Test
			ValidationResult result = orValidator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(result.HasFailures);
		}
	}
}
