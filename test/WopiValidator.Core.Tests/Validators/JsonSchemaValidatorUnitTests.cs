// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core;
using Microsoft.Office.WopiValidator.Core.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Microsoft.Office.WopiValidator.UnitTests.Validators
{
	[TestClass]
	public class JsonSchemaValidatorUnitTests
	{
		// Unix timestamp helper for .NET Framework 4.5 compatibility
		private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		
		private static long ToUnixTimeSeconds(DateTime dateTime)
		{
			return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
		}

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

		[TestMethod]
		public void Validate_AccessTokenExpiry_ValidTimestamp_Succeeds()
		{
			// Valid timestamp: one hour from now
			long validTimestamp = ToUnixTimeSeconds(DateTime.UtcNow.AddHours(1));

			string jsonInput = $"{{\"BaseFileName\": \"test.docx\"," +
			                   "\"OwnerId\": \"owner123\"," +
			                   "\"Size\": 1024," +
			                   "\"UserId\": \"user123\"," +
			                   "\"Version\": \"1.0\"," +
			                   $"\"AccessTokenExpiry\": {validTimestamp}}}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_AccessTokenExpiry_OldTimestamp_Fails()
		{
			// Invalid timestamp: 15 years ago
			long invalidTimestamp = ToUnixTimeSeconds(DateTime.UtcNow.AddYears(-15));

			string jsonInput = $"{{\"BaseFileName\": \"test.docx\"," +
			                   "\"OwnerId\": \"owner123\"," +
			                   "\"Size\": 1024," +
			                   "\"UserId\": \"user123\"," +
			                   "\"Version\": \"1.0\"," +
			                   $"\"AccessTokenExpiry\": {invalidTimestamp}}}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
			Assert.IsTrue(result.Errors.Any(e => e.Contains("AccessTokenExpiry")));
			Assert.IsTrue(result.Errors.Any(e => e.Contains("outside the valid range")));
		}

		[TestMethod]
		public void Validate_AccessTokenExpiry_FutureTimestamp_Fails()
		{
			// Invalid timestamp: 15 years in the future
			long invalidTimestamp = ToUnixTimeSeconds(DateTime.UtcNow.AddYears(15));

			string jsonInput = $"{{\"BaseFileName\": \"test.docx\"," +
			                   "\"OwnerId\": \"owner123\"," +
			                   "\"Size\": 1024," +
			                   "\"UserId\": \"user123\"," +
			                   "\"Version\": \"1.0\"," +
			                   $"\"AccessTokenExpiry\": {invalidTimestamp}}}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
			Assert.IsTrue(result.Errors.Any(e => e.Contains("AccessTokenExpiry")));
			Assert.IsTrue(result.Errors.Any(e => e.Contains("outside the valid range")));
		}

		[TestMethod]
		public void Validate_ServerTime_ValidTimestamp_Succeeds()
		{
			// Valid timestamp: current time
			long validTimestamp = ToUnixTimeSeconds(DateTime.UtcNow);

			string jsonInput = $"{{\"BaseFileName\": \"test.docx\"," +
			                   "\"OwnerId\": \"owner123\"," +
			                   "\"Size\": 1024," +
			                   "\"UserId\": \"user123\"," +
			                   "\"Version\": \"1.0\"," +
			                   $"\"ServerTime\": {validTimestamp}}}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_ServerTime_InvalidTimestamp_Fails()
		{
			// Invalid timestamp: 15 years ago
			long invalidTimestamp = ToUnixTimeSeconds(DateTime.UtcNow.AddYears(-15));

			string jsonInput = $"{{\"BaseFileName\": \"test.docx\"," +
			                   "\"OwnerId\": \"owner123\"," +
			                   "\"Size\": 1024," +
			                   "\"UserId\": \"user123\"," +
			                   "\"Version\": \"1.0\"," +
			                   $"\"ServerTime\": {invalidTimestamp}}}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
			Assert.IsTrue(result.Errors.Any(e => e.Contains("ServerTime")));
			Assert.IsTrue(result.Errors.Any(e => e.Contains("outside the valid range")));
		}

		[TestMethod]
		public void Validate_FileExtension_ValidFormat_Succeeds()
		{
			string jsonInput =
				"{\"BaseFileName\": \"test.docx\"," +
				"\"OwnerId\": \"owner123\"," +
				"\"Size\": 1024," +
				"\"UserId\": \"user123\"," +
				"\"Version\": \"1.0\"," +
				"\"FileExtension\": \".docx\"}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_FileExtension_InvalidFormat_Fails()
		{
			string jsonInput =
				"{\"BaseFileName\": \"test.docx\"," +
				"\"OwnerId\": \"owner123\"," +
				"\"Size\": 1024," +
				"\"UserId\": \"user123\"," +
				"\"Version\": \"1.0\"," +
				"\"FileExtension\": \"docx\"}"; // Missing dot

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_SHA256_ValidFormat_Succeeds()
		{
			string jsonInput =
				"{\"BaseFileName\": \"test.docx\"," +
				"\"OwnerId\": \"owner123\"," +
				"\"Size\": 1024," +
				"\"UserId\": \"user123\"," +
				"\"Version\": \"1.0\"," +
				"\"SHA256\": \"a1b2c3d4e5f6789012345678901234567890123456789012345678901234567890\"}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_SHA256_InvalidFormat_Fails()
		{
			string jsonInput =
				"{\"BaseFileName\": \"test.docx\"," +
				"\"OwnerId\": \"owner123\"," +
				"\"Size\": 1024," +
				"\"UserId\": \"user123\"," +
				"\"Version\": \"1.0\"," +
				"\"SHA256\": \"invalid-sha256\"}"; // Invalid format

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_EmptyRequiredFields_Fails()
		{
			string jsonInput =
				"{\"BaseFileName\": \"\"," + // Empty required field
				"\"OwnerId\": \"owner123\"," +
				"\"Size\": 1024," +
				"\"UserId\": \"user123\"," +
				"\"Version\": \"1.0\"}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_NegativeSize_Fails()
		{
			string jsonInput =
				"{\"BaseFileName\": \"test.docx\"," +
				"\"OwnerId\": \"owner123\"," +
				"\"Size\": -100," + // Negative size
				"\"UserId\": \"user123\"," +
				"\"Version\": \"1.0\"}";

			IResponseData response = new ResponseDataMock
			{
				IsTextResponse = true,
				ResponseContentText = jsonInput,
			};

			ValidationResult result = new JsonSchemaValidator("CheckFileInfoSchema").Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}
	}
}
