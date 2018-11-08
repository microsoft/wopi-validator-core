// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core
{
	/// <summary>
	/// This class contains functions to help handle exceptions while executing tests, so that the exception
	/// data can be displayed in the validator UI (or command line). The response code is set to
	/// HttpStatusCode.Unused (306) in this case. The exception message is put in the X-WOPI-ValidatorError
	/// header, and the response stream contains the stack trace (Exception.ToString).
	///
	/// This works in conjunction with the ExceptionValidator. That validator sees the 306 response and the
	/// X-WOPI-ValidatorError header and fails the test, passing the exception details in the test results.
	///
	/// Note: This is a somewhat hacky solution (passing the exception details through a 'fake' ResponseData
	/// that is then picked up by a Validator later), but this seems to be an unusual scenario overall and
	/// this approach can be cleaned up and improved over time if needed.
	/// </summary>
	class ExceptionHelper
	{
		private static IResponseData CustomResponseData(string exceptionMessage, string exceptionDetails)
		{
			var body = Encoding.UTF8.GetBytes(exceptionDetails);
			var headers = new Dictionary<string, string> { { Constants.Headers.ValidatorError, exceptionMessage } };
			return new ResponseData(new MemoryStream(body), (int)HttpStatusCode.Unused, headers, true, TimeSpan.Zero /* elapsed time */);
		}

		/// <summary>
		/// This method extracts details from an Exception and wraps them in an IResponseData.
		/// </summary>
		///
		/// <returns>An IResponseData object with the exception details included. The exception message
		/// will be in Headers["X-WOPI-ValidatorError"], and more detailed information including the
		/// stack trace will be in ResponseStream.</returns>
		public static IResponseData WrapExceptionInResponseData(Exception ex)
		{
			return CustomResponseData($"({ex.GetType().Name}) {ex.Message}", ex.ToString());
		}

		public static IResponseData WrapExceptionInResponseData(WebException ex)
		{
			var message = $"({ex.GetType().Name}){ex.Status}: {ex.Message}";
			return CustomResponseData(message, ex.ToString());
		}

	}
}
