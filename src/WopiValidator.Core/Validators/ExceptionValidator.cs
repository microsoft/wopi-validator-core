using System.Collections.Generic;
using System.Net;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Validates that no unhandled WebExceptions were thrown while handling a response.
	/// If an unhandled exception is thrown, it is caught and the exception details are
	/// inserted into the response headers.
	///
	/// This validator is included on every request; it does not need to be explicitly added
	/// to the test case definition XML.
	/// </summary>
	class ExceptionValidator : IValidator
	{
		public string Name { get { return "ExceptionValidator"; } }

		public ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState)
		{
			if (data.StatusCode == (int)HttpStatusCode.Unused &&
				data.Headers.ContainsKey(Constants.Headers.ValidatorError))
			{
				return new ValidationResult(data.Headers[Constants.Headers.ValidatorError]);
			}

			return new ValidationResult();
		}
	}
}
