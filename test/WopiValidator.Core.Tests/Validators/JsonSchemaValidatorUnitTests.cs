// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core;
using Microsoft.Office.WopiValidator.Core.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Office.WopiValidator.UnitTests.Validators
{
	[TestClass]
	public class JsonSchemaValidatorUnitTests
	{
		[TestMethod]
		public void Validate_CheckFileInfoSchema_Succeeds()
		{
			string jsonInput =
				"{\"BaseFileName\": \"dummyFileName\"," +
				"\"OwnerId\": \"dummyOwnerId\"," +
				"\"Size\": 100," +
				"\"UserFriendlyName\": \"dummyUserFriendlyName\"," +
				"\"UserId\": \"dummyUserId\"," +
				"\"Version\": \"dummyVersion\"}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CheckFileInfoSchema_MissingRequiredFields_Fails()
		{
			string jsonInput =
				"{\"BaseFileName\": \"dummyFileName\"," +
				"\"OwnerId\": \"dummyOwnerId\"," +
				"\"Size\": 100," +
				"\"UserFriendlyName\": \"dummyUserFriendlyName\"," +
				"\"Version\": \"dummyVersion\"}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CheckFileInfoSchema_WrongRequiredFields_Fails()
		{
			string jsonInput =
				"{\"BaseFileNameppppppp\": \"dummyFileName\"," +
				"\"OwnerId\": \"dummyOwnerId\"," +
				"\"Size\": 100," +
				"\"UserFriendlyName\": \"dummyUserFriendlyName\"," +
				"\"UserId\": \"dummyUserId\"," +
				"\"Version\": \"dummyVersion\"}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CheckFileInfoSchema_UndefinedOptionalFields_Fails()
		{
			string jsonInput =
				"{\"BaseFileName\": \"dummyFileName\"," +
				"\"OwnerId\": \"dummyOwnerId\"," +
				"\"Size\": 100," +
				"\"UserFriendlyName\": \"dummyUserFriendlyName\"," +
				"\"UserId\": \"dummyUserId\"," +
				"\"Version\": \"dummyVersion\"," +
				"\"xxx\": \"xxx\"}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CheckFileInfoSchema_DefinedOptionalFields_Succeeds()
		{
			string jsonInput =
				"{\"BaseFileName\": \"dummyFileName\"," +
				"\"OwnerId\": \"dummyOwnerId\"," +
				"\"Size\": 100," +
				"\"UserFriendlyName\": \"dummyUserFriendlyName\"," +
				"\"UserId\": \"dummyUserId\"," +
				"\"Version\": \"dummyVersion\"," +
				"\"WebEditingDisabled\": false}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}
	}
}
