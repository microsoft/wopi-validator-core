// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core;
using Microsoft.Office.WopiValidator.Core.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Office.WopiValidator.UnitTests.Validators
{
	[TestClass]
	public class ResponseContentValidatorUnitTests
	{
		[TestMethod]
		public void Validate_ResponseStreamAsExpected_ValidationSucceeds()
		{
			// Arrange
			const string fileId = "fileId";
			byte[] content = Encoding.UTF8.GetBytes("my content");
			MemoryStream responseStream = new MemoryStream(content);
			MemoryStream expectedStream = new MemoryStream(content);
			ResponseContentValidator validator = new ResponseContentValidator(fileId);
			ResourceManagerMock resourceManager = new ResourceManagerMock { { fileId, expectedStream } };
			ResponseDataMock responseData = new ResponseDataMock { ResponseStream = responseStream };

			// Act
			ValidationResult validationResult = validator.Validate(responseData, resourceManager, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Validate_ConflictingArguments_ThrowsException()
		{
			// Arrange
			const string fileId = "fileId";
			const string content = "my content";
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
			MemoryStream expectedStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

			// Act - should throw an ArgumentException
			ResponseContentValidator validator = new ResponseContentValidator(fileId, content);
		}

		[TestMethod]
		public void Validate_BodyAsExpected_ValidationSucceeds()
		{
			// Arrange
			const string fileId = "fileId";
			const string content = "my content";
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
			MemoryStream expectedStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
			ResponseContentValidator validator = new ResponseContentValidator(null, content);
			ResourceManagerMock resourceManager = new ResourceManagerMock { { fileId, expectedStream } };
			ResponseDataMock responseData = new ResponseDataMock { ResponseStream = responseStream };

			// Act
			ValidationResult validationResult = validator.Validate(responseData, resourceManager, null);

			// Assert
			Assert.IsFalse(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ResponseStreamNotAsExpected_ValidationFails()
		{
			// Arrange
			const string fileId = "fileId";
			MemoryStream responseStream = new MemoryStream();
			MemoryStream expectedStream = new MemoryStream(Encoding.UTF8.GetBytes("my content"));
			ResponseContentValidator validator = new ResponseContentValidator(fileId);
			ResourceManagerMock resourceManager = new ResourceManagerMock { { fileId, expectedStream } };
			ResponseDataMock responseData = new ResponseDataMock { ResponseStream = responseStream };

			// Act
			ValidationResult validationResult = validator.Validate(responseData, resourceManager, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_BodyNotAsExpected_ValidationFails()
		{
			// Arrange
			const string fileId = "fileId";
			const string content = "my content";
			MemoryStream responseStream = new MemoryStream();
			MemoryStream expectedStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
			ResponseContentValidator validator = new ResponseContentValidator(null, content);
			ResourceManagerMock resourceManager = new ResourceManagerMock { { fileId, expectedStream } };
			ResponseDataMock responseData = new ResponseDataMock { ResponseStream = responseStream };

			// Act
			ValidationResult validationResult = validator.Validate(responseData, resourceManager, null);

			// Assert
			Assert.IsTrue(validationResult.HasFailures);
		}

		[TestMethod]
		public void Validate_ResponseStreamNotAsExpected_OneErrorReturned()
		{
			// Arrange
			const string fileId = "fileId";
			MemoryStream responseStream = new MemoryStream();
			MemoryStream expectedStream = new MemoryStream(Encoding.UTF8.GetBytes("my content"));
			ResponseContentValidator validator = new ResponseContentValidator(fileId);
			ResourceManagerMock resourceManager = new ResourceManagerMock { { fileId, expectedStream } };
			ResponseDataMock responseData = new ResponseDataMock { ResponseStream = responseStream };

			// Act
			ValidationResult validationResult = validator.Validate(responseData, resourceManager, null);

			// Assert
			Assert.IsNotNull(validationResult.Errors);
			Assert.AreEqual(1, validationResult.Errors.Count());
		}

		[TestMethod]
		public void Validate_BodyNotAsExpected_OneErrorReturned()
		{
			// Arrange
			const string fileId = "fileId";
			const string content = "my content";
			MemoryStream responseStream = new MemoryStream();
			MemoryStream expectedStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
			ResponseContentValidator validator = new ResponseContentValidator(null, content);
			ResourceManagerMock resourceManager = new ResourceManagerMock { { fileId, expectedStream } };
			ResponseDataMock responseData = new ResponseDataMock { ResponseStream = responseStream };

			// Act
			ValidationResult validationResult = validator.Validate(responseData, resourceManager, null);

			// Assert
			Assert.IsNotNull(validationResult.Errors);
			Assert.AreEqual(1, validationResult.Errors.Count());
		}
	}
}

