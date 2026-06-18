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
		public void Validate_CsppCheckFileInfoSchema_Succeeds()
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

			ValidationResult result = new JsonSchemaValidator("CsppCheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CsppCheckFileInfoSchema_FailsIfIncludingCsppPlusProperties()
		{
			string jsonInput =
				"{\"BaseFileName\": \"dummyFileName\"," +
				"\"OwnerId\": \"dummyOwnerId\"," +
				"\"Size\": 100," +
				"\"UserFriendlyName\": \"dummyUserFriendlyName\"," +
				"\"UserId\": \"dummyUserId\"," +
				"\"Version\": \"dummyVersion\"," +
				"\"OfficeCollaborationServiceEndpointUrl\": \"http:\\/\\/localhost\\/ocs\\/join.ashx?app=wopitest\"}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CsppCheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CsppPlusCheckFileInfoSchema_Succeeds()
		{
			string jsonInput =
				"{\"BaseFileName\": \"dummyFileName\"," +
				"\"OwnerId\": \"dummyOwnerId\"," +
				"\"Size\": 100," +
				"\"UserFriendlyName\": \"dummyUserFriendlyName\"," +
				"\"UserId\": \"dummyUserId\"," +
				"\"Version\": \"dummyVersion\"," +
				"\"SupportsCoauth\": true," +
				"\"SequenceNumber\": 10," +
				"\"OfficeCollaborationServiceEndpointUrl\": \"http:\\/\\/localhost\\/ocs\\/join.ashx?app=wopitest\"," +
				"\"RealTimeChannelEndpointUrl\": \"http:\\/\\/localhost\\/rtc2\\/\"," +
				"\"AccessTokenExpiry\": 0," +
				"\"ServerTime\": 100," +
				"\"SharingStatus\": \"Shared\"," +
				"\"FileGeoLocationCode\": \"\",}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CsppPlusCheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CsppCheckFileInfoSchema_MissingRequiredFields_Fails()
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

			ValidationResult result = new JsonSchemaValidator("CsppCheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CsppPlusCheckFileInfoSchema_MissingRequiredFields_Fails()
		{
			string jsonInput =
				"{\"BaseFileName\": \"dummyFileName\"," +
				"\"OwnerId\": \"dummyOwnerId\"," +
				"\"Size\": 100," +
				"\"UserFriendlyName\": \"dummyUserFriendlyName\"," +
				"\"UserId\": \"dummyUserId\"," +
				"\"Version\": \"dummyVersion\"," +
				"\"SupportsCoauth\": true," +
				"\"SequenceNumber\": 10," +
				//"\"OfficeCollaborationServiceEndpointUrl\": \"http:\\/\\/localhost\\/ocs\\/join.ashx?app=wopitest\"," +
				"\"RealTimeChannelEndpointUrl\": \"http:\\/\\/localhost\\/rtc2\\/\"," +
				"\"AccessTokenExpiry\": 0," +
				"\"ServerTime\": 100," +
				"\"SharingStatus\": \"Shared\"," +
				"\"FileGeoLocationCode\": \"\",}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CsppPlusCheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CsppCheckFileInfoSchema_WrongRequiredFields_Fails()
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

			ValidationResult result = new JsonSchemaValidator("CsppCheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CsppPlusCheckFileInfoSchema_WrongRequiredFields_Fails()
		{
			string jsonInput =
				"{\"BaseFileName\": \"dummyFileName\"," +
				"\"OwnerId\": \"dummyOwnerId\"," +
				"\"Size\": 100," +
				"\"UserFriendlyName\": \"dummyUserFriendlyName\"," +
				"\"UserId\": \"dummyUserId\"," +
				"\"Version\": \"dummyVersion\"," +
				"\"SupportsCoauth\": true," +
				"\"SequenceNumber\": 10," +
				"\"OfficeCollaborationServiceEndpointUrlxxxxxxxxxxxxxxxxxxxxxxxx\": \"http:\\/\\/localhost\\/ocs\\/join.ashx?app=wopitest\"," +
				"\"RealTimeChannelEndpointUrl\": \"http:\\/\\/localhost\\/rtc2\\/\"," +
				"\"AccessTokenExpiry\": 0," +
				"\"ServerTime\": 100," +
				"\"SharingStatus\": \"Shared\"," +
				"\"FileGeoLocationCode\": \"\",}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CsppPlusCheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CsppCheckFileInfoSchema_UndefinedOptionalFields_Succeeds()
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

			ValidationResult result = new JsonSchemaValidator("CsppCheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CsppPlusCheckFileInfoSchema_UndefinedOptionalFields_Succeeds()
		{
			string jsonInput =
				"{\"BaseFileName\": \"dummyFileName\"," +
				"\"OwnerId\": \"dummyOwnerId\"," +
				"\"Size\": 100," +
				"\"UserFriendlyName\": \"dummyUserFriendlyName\"," +
				"\"UserId\": \"dummyUserId\"," +
				"\"Version\": \"dummyVersion\"," +
				"\"SupportsCoauth\": true," +
				"\"SequenceNumber\": 10," +
				"\"OfficeCollaborationServiceEndpointUrl\": \"http:\\/\\/localhost\\/ocs\\/join.ashx?app=wopitest\"," +
				"\"RealTimeChannelEndpointUrl\": \"http:\\/\\/localhost\\/rtc2\\/\"," +
				"\"AccessTokenExpiry\": 0," +
				"\"ServerTime\": 100," +
				"\"SharingStatus\": \"Shared\"," +
				"\"FileGeoLocationCode\": \"\"," +
				"\"xxx\":\"xxx\"}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CsppPlusCheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CsppCheckFileInfoSchema_DefinedOptionalFields_Succeeds()
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

			ValidationResult result = new JsonSchemaValidator("CsppCheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CsppPlusCheckFileInfoSchema_DefinedOptionalFields_Succeeds()
		{
			string jsonInput =
				"{\"BaseFileName\": \"dummyFileName\"," +
				"\"OwnerId\": \"dummyOwnerId\"," +
				"\"Size\": 100," +
				"\"UserFriendlyName\": \"dummyUserFriendlyName\"," +
				"\"UserId\": \"dummyUserId\"," +
				"\"Version\": \"dummyVersion\"," +
				"\"SupportsCoauth\": true," +
				"\"SequenceNumber\": 10," +
				"\"OfficeCollaborationServiceEndpointUrl\": \"http:\\/\\/localhost\\/ocs\\/join.ashx?app=wopitest\"," +
				"\"RealTimeChannelEndpointUrl\": \"http:\\/\\/localhost\\/rtc2\\/\"," +
				"\"AccessTokenExpiry\": 0," +
				"\"ServerTime\": 100," +
				"\"SharingStatus\": \"Shared\"," +
				"\"FileGeoLocationCode\": \"\"," +
				"\"AllowAdditionalMicrosoftServices\":true}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CsppPlusCheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}
	}
}
