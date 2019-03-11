// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Office.WopiValidator.Core
{
	/// <summary>
	/// Represents a single test case.
	/// </summary>
	internal class TestCase : ITestCase
	{
		public TestCase(
			string resourceId,
			IEnumerable<IRequest> requests,
			IEnumerable<IRequest> cleanupRequests,
			string name,
			string description,
			bool uploadDocumentOnSetup,
			bool deleteDocumentOnTearDown,
			string category)
		{
			DeleteDocumentOnTearDown = deleteDocumentOnTearDown;
			UploadDocumentOnSetup = uploadDocumentOnSetup;

			if (requests == null)
				throw new ArgumentNullException("requests");
			Requests = requests.ToArray();
			if (!Requests.Any())
				throw new ArgumentException("TestCase has to have at least one request.", "requests");

			if (cleanupRequests == null)
				cleanupRequests = Enumerable.Empty<IRequest>();
			CleanupRequests = cleanupRequests.ToArray();

			if (string.IsNullOrEmpty(resourceId))
				throw new ArgumentException("ResourceId cannot be empty.", "resourceId");
			ResourceId = resourceId;

			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("Name cannot be empty.", "name");
			Name = name;

			Description = description;
			UiScreenShot = String.Empty;
			DocumentationLink = String.Empty;
			FailMessage = String.Empty;
			Category = category;
		}

		public IEnumerable<IRequest> Requests { get; private set; }
		public IEnumerable<IRequest> CleanupRequests { get; private set; }
		public string Name { get; private set; }
		public string Description { get; private set; }
		public string ResourceId { get; private set; }
		public string UiScreenShot { get; set; }
		public string DocumentationLink { get; set; }
		public string FailMessage { get; set; }
		public bool UploadDocumentOnSetup { get; private set; }
		public bool DeleteDocumentOnTearDown { get; private set; }
		public string Category { get; private set; }
		public TestCategory TestCategory
		{
			get
			{
				if (!Enum.TryParse(Category, true /* ignoreCase */, out TestCategory testCategory))
				{
					throw new Exception($"Invalid TestCategory: {Category}");
				}
				return testCategory;
			}
		}
	}
}
