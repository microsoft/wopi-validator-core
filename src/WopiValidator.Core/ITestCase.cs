// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core
{
	public interface ITestCase
	{
		string Name { get; }

		string Description { get; }

		IEnumerable<IRequest> Requests { get; }

		IEnumerable<IRequest> CleanupRequests { get; }

		string ResourceId { get; }

		string UiScreenShot { get; set; }

		string DocumentationLink { get; set; }

		string FailMessage { get; set; }

		bool UploadDocumentOnSetup { get; }

		bool DeleteDocumentOnTearDown { get; }

		string Category { get; }

		TestCategory TestCategory { get; }
	}
}
