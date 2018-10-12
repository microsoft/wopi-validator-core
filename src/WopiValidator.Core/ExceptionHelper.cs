using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core
{
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
			return CustomResponseData(ex.Message, ex.ToString());
		}

		public static IResponseData WrapExceptionInResponseData(WebException ex)
		{
			var message = $"{ex.Status}: {ex.Message}";
			return CustomResponseData(message, ex.ToString());
		}

	}
}
