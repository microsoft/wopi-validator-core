// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Office.WopiValidator.Core
{
	public enum ResultStatus
	{
		Pass,
		Fail,
		Skipped
	}

	public sealed class ValidationResult
	{
		public ValidationResult()
			: this(Enumerable.Empty<string>())
		{
		}

		public ValidationResult(string error)
			: this(new[] { error })
		{
		}

		public ValidationResult(IEnumerable<string> errors)
		{
			Errors = errors.ToList();
		}

		public IEnumerable<string> Errors { get; private set; }

		public bool HasFailures
		{
			get { return Errors.Any(); }
		}
	}

	public sealed class TestCaseResult
	{
		public TestCaseResult(string name, IEnumerable<RequestInfo> requestDetails, ResultStatus status)
			: this(name, requestDetails, String.Empty, Enumerable.Empty<string>(), status)
		{
		}

		public TestCaseResult(string name, IEnumerable<RequestInfo> requestDetails, string message, IEnumerable<string> errors, ResultStatus status)
		{
			Name = name;
			RequestDetails = requestDetails;
			Message = message;
			Errors = errors;
			Status = status;
		}

		public ResultStatus Status { get; private set; }
		public string Message { get; private set; }
		public IEnumerable<string> Errors { get; private set; }
		public string Name { get; private set; }
		public IEnumerable<RequestInfo> RequestDetails { get; private set; }
	}
}
