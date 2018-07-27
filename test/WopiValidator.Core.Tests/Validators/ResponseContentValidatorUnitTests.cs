// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Office.WopiValidator.Core;
using Microsoft.Office.WopiValidator.Core.Validators;

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
		public void Validate_ResponseStreamNotAsExpected_ValidationFails()
		{
			// Arrange
			const string fileId = "fileId";
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes("my content"));
			MemoryStream expectedStream = new MemoryStream();
			ResponseContentValidator validator = new ResponseContentValidator(fileId);
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
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes("my content"));
			MemoryStream expectedStream = new MemoryStream();
			ResponseContentValidator validator = new ResponseContentValidator(fileId);
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

