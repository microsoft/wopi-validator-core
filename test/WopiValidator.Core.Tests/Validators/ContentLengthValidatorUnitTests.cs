// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Office.WopiValidator.Core;
using Microsoft.Office.WopiValidator.Core.Validators;

namespace Microsoft.Office.WopiValidator.UnitTests.Validators
{
	[TestClass]
	public class ContentLengthValidatorUnitTests
	{
		[TestMethod]
		public void Validate_NonZeroContentAndEqualContentLength_Succeeds()
		{
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes("my content"));
			IResponseData response = new ResponseDataMock
			{
				ResponseStream = responseStream,
				Headers = new CaseInsensitiveDictionary(1) { { "Content-Length", responseStream.Length.ToString() } }
			};
			ValidationResult result = new ContentLengthValidator().Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_ZeroContentAndZeroContentLength_Succeeds()
		{
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes(""));
			IResponseData response = new ResponseDataMock
			{
				ResponseStream = responseStream,
				Headers = new CaseInsensitiveDictionary(1) { { "Content-Length", "0" } }
			};
			ValidationResult result = new ContentLengthValidator().Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_ZeroContentAndNoContentLengthHeader_Succeeds()
		{
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes(""));
			IResponseData response = new ResponseDataMock
			{
				ResponseStream = responseStream,
				Headers = new CaseInsensitiveDictionary(0)
			};
			ValidationResult result = new ContentLengthValidator().Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}

		[TestMethod]
		public void Validate_ZeroContentAndNonZeroContentLength_Fails()
		{
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes(""));
			IResponseData response = new ResponseDataMock
			{
				ResponseStream = responseStream,
				Headers = new CaseInsensitiveDictionary(1) { { "Content-Length", "1" } }
			};
			ValidationResult result = new ContentLengthValidator().Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_NonZeroContentAndUnequalContentLength_Fails()
		{
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes("my content"));
			IResponseData response = new ResponseDataMock
			{
				ResponseStream = responseStream,
				Headers = new CaseInsensitiveDictionary(1) { { "Content-Length", "1" } }
			};
			ValidationResult result = new ContentLengthValidator().Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		//[Ignore("This test fails by current design; the ContentLengthValidator always returns true if the Content-Length header is not provided.")]
		public void Validate_NonZerContentAndNoContentLengthHeader_Fails()
		{
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes("my content"));
			IResponseData response = new ResponseDataMock
			{
				ResponseStream = responseStream,
				Headers = new CaseInsensitiveDictionary(0)
			};
			ValidationResult result = new ContentLengthValidator().Validate(response, null, null);
			Assert.IsTrue(result.HasFailures);
		}

		[TestMethod]
		public void Validate_CaseInsensitiveHeaderNameMatching_Succeeds()
		{
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes("my content"));
			IResponseData response = new ResponseDataMock
			{
				ResponseStream = responseStream,
				Headers = new CaseInsensitiveDictionary(1) { { "content-length", responseStream.Length.ToString() } }
			};
			ValidationResult result = new ContentLengthValidator().Validate(response, null, null);
			Assert.IsFalse(result.HasFailures);
		}
	}
}
