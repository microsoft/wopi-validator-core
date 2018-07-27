// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Office.WopiValidator.Core;
using Microsoft.Office.WopiValidator.Core.Validators;

namespace Microsoft.Office.WopiValidator.UnitTests.Validators
{
	[TestClass]
	public class JsonContentValidatorUnitTests
	{
		[TestMethod]
		public void Validate_ResponseStreamIsNull_ValidationFails()
		{
			// Arrange
			JsonContentValidator validator = new JsonContentValidator();
			ResponseDataMock responseData = new ResponseDataMock
			{
				ResponseStream = null
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ResponseStreamIsNotValidJson_ValidationFails()
		{
			// Arrange
			const string content = "Some non-JSON content";
			JsonContentValidator validator = new JsonContentValidator();
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithCorrectExpectedValueAndNoExpectedState_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: '2' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 2, true, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithNoExpectedValueAndNoExpectedState_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: '2' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 0, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithIncorrectExpectedValueAndNoExpectedState_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: '2' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 3, true, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithCorrectExpectedStateValue_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: '2' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "2" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 1, true, "StateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithCorrectExpectedValueAndIncorrectExpectedStateKey_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: '2' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "2" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 2, true, "IncorrectStateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithIncorrectExpectedValueAndExpectedStateKey_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: '2' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "2" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 1, true, "IncorrectStateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithIncorrectExpectedValueAndExpectedStateValue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: '2' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "3" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 1, true, "StateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithNonIntegerActualValue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: 'value' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 3, true, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithCorrectExpectedValueAndNonIntegerExpectedStateValue_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: '2' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "non integer" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 2, true, "StateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithNoExpectedValueAndNonIntegerExpectedStateValue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: '2' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "non integer" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 0, false, "StateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithIsRequiredSetToTrue_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: '2' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 0, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithNoActualValueAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: '' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 0, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithActualValueAsEmptyArrayAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: [] }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 0, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithActualValueAsNullAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: null }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 0, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingIntegerPropertyWithEmptyJsonAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonIntegerPropertyValidator("propertyName", true, 0, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithCorrectExpectedValue_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: 'true' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, true, true, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithNoExpectedValueAndNoExpectedStateKey_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: 'true' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, false, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithNonBooleanActualValue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: 'value' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, true, true, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithIncorrectExpectedValue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: 'false' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, true, true, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithCorrectExpectedValueAndIncorrectExpectedStateKey_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: 'false' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "false" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, false, true, "IncorrectStateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithIncorrectExpectedValueAndExpectedStateKey_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: 'false' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "false" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, true, true, "IncorrectStateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithIncorrectExpectedValueAndExpectedStateValue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: 'false' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "true" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, true, true, "StateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithCorrectExpectedValueAndNonBooleanExpectedStateValue_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: 'false' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "non boolean" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, false, true, "StateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithNoExpectedValueAndNonBooleanExpectedStateValue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: '2' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "non boolean" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, false, false, "StateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithIsRequiredSetToTrue_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: 'true' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, false, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithNoActualValueAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: '' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, false, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithActualValueAsEmptyArrayAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: [] }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, false, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithActualValueAsNullAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: null }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, false, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingBooleanPropertyWithEmptyJsonAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonBooleanPropertyValidator("propertyName", true, false, false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyWithCorrectExpectedValue_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: 'value' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "value", true, null, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyWithIncorrectExpectedValue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: 'value' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "differentValue", true, null, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyEndsWith_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: 'propertyValueSuffix' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "propertyValueSuffix", true, "Suffix", null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyEndsWith_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: 'propertyValueSuffix' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "propertyValueSuffix", true, "Value", null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyWithNoExpectedValueAndNoExpectedStateKey_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: 'value' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "", false, null, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyWithCorrectExpectedValueAndIncorrectExpectedStateKey_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: 'value' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "value" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "value", true, null, "IncorrectStateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyWithIncorrectExpectedValueAndExpectedStateKey_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: 'value' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "value" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "incorrect value", true, null, "IncorrectStateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyWithIncorrectExpectedValueAndExpectedStateValue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: 'value' }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "incorrect value" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "incorrect value", true, null, "StateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyWithIsRequiredSetToTrue_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: 'value' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "value", false, null, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyWithNoActualValueAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: '' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "value", false, null, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyWithActualValueAsEmptyArrayAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: [] }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "value", false, null, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyWithActualValueAsNullAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: null }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "value", false, null, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringPropertyWithEmptyJsonAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "value", false, null, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingAbsoluteUrlPropertyWithCorrectValue_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: 'https://bing.com' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonAbsoluteUrlPropertyValidator("propertyName", false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingAbsoluteUrlPropertyWithIncorrectValue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: 'bing.com' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonAbsoluteUrlPropertyValidator("propertyName", false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_RequiredValidatingAbsoluteUrlPropertyWithNoValue_ValidationFails()
		{
			// Arrange
			const string content = "{ }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonAbsoluteUrlPropertyValidator("propertyName", true, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_NotRequiredValidatingAbsoluteUrlPropertyWithNoValue_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonAbsoluteUrlPropertyValidator("propertyName", false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyWithExpectedValuePresent_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: ['value', 'value2'] }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "value2", true, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyWithExpectedValueNotPresent_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: ['value', 'value2'] }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "differentValue", true, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyWithNoExpectedValueAndNoExpectedStateKey_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: ['value', 'value2'] }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "", false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyWithCorrectExpectedValueAndIncorrectExpectedStateKey_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: ['value', 'value2'] }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "value" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "value", true, "IncorrectStateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyWithExpectedValueNotPresentAndIncorrectExpectedStateKey_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: ['value', 'value2'] }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "value" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "incorrect value", true, "IncorrectStateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyWithExpectedValueNotPresentAndIncorrectExpectedStateValue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: ['value', 'value2'] }";
			Dictionary<string, string> savedState = new Dictionary<string, string>() { { "StateKey", "incorrect value" } };
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "incorrect value", true, "StateKey"));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, savedState);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyIgnoresCase_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: ['value', 'value2'] }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "Value", false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyWithIsRequiredSetToTrue_ValidationSucceeds()
		{
			// Arrange
			const string content = "{ propertyName: ['value', 'value2'] }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "value", false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyWithNoActualValueAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: '' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "value", false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyWithActualValueAsEmptyArrayAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: [] }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "value", false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyWithActualValueAsNullAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ propertyName: null }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "value", false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ValidatingStringArrayPropertyWithEmptyJsonAndIsRequiredSetToTrue_ValidationFails()
		{
			// Arrange
			const string content = "{ }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonArrayPropertyValidator("propertyName", true, "value", false, null));
			ResponseDataMock responseData = CreateDefaultResponseDataMock(content);

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_Non_Text_Content_Validation_Fails()
		{
			// Arrange
			const string content = "{ propertyName: 'value' }";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "value", true, null, null));
			ResponseDataMock responseData = new ResponseDataMock
			{
				ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(content)),
				ResponseContentText = content,
				IsTextResponse = false
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_Null_Content_String_Validation_Fails()
		{
			// Arrange
			const string content = "";
			JsonContentValidator validator = new JsonContentValidator(
				new JsonContentValidator.JsonStringPropertyValidator("propertyName", true, "value", true, null, null));
			ResponseDataMock responseData = new ResponseDataMock
			{
				ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(content)),
				ResponseContentText = null,
				IsTextResponse = true
			};

			// Act
			ValidationResult validationResult = validator.Validate(responseData, null, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		private static ResponseDataMock CreateDefaultResponseDataMock(string content)
		{
			return new ResponseDataMock
			{
				ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(content)),
				ResponseContentText = content,
				IsTextResponse = true,
			};
		}
	}
}
