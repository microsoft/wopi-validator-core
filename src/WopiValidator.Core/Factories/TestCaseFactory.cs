// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	public class TestCaseFactory : ITestCaseFactory
	{
		public IEnumerable<ITestCase> GetTestCases(XElement definitions, TestCategory targetTestCategory)
		{
			return definitions.Elements("TestCase").Where(x => DoesTestCategoryMatchTargetTestCategory(x, targetTestCategory)).Select(x => GetTestCase(x));
		}

		public void GetTestCases(
			XElement definition,
			Dictionary<string, ITestCase> prereqCasesDictionary,
			out IEnumerable<ITestCase> prereqTests,
			out IEnumerable<ITestCase> groupTests,
			TestCategory targetTestCategory)
		{
			XElement prereqsElement = definition.Element("PrereqTests") ?? new XElement("PrereqTests");
			prereqTests = GetPrereqTests(prereqsElement, prereqCasesDictionary);

			XElement testCasesElement = definition.Element("TestCases") ?? new XElement("TestCases");
			groupTests = GetTestCases(testCasesElement, targetTestCategory);
		}

		private static IEnumerable<ITestCase> GetPrereqTests(XElement definition, Dictionary<string, ITestCase> prereqsDictionary)
		{
			IEnumerable<string> prereqTestNames = definition.Elements("PrereqTest").Select(x => x.Value);
			foreach (string testName in prereqTestNames)
			{
				ITestCase testCase;
				if (prereqsDictionary.TryGetValue(testName, out testCase))
					yield return testCase;
				else
					throw new ArgumentException("Could not find a prereq test case named {0}", testName);
			}
		}

		/// <summary>
		/// Parses single TestCase.
		///
		/// Use RequestFactory.GetRequests to parse requests defined in that Test Case.
		/// </summary>
		private static ITestCase GetTestCase(XElement definition)
		{
			string category = (string)definition.Attribute("Category");
			string name = (string)definition.Attribute("Name");
			string resourceId = (string) definition.Attribute("Document");
			string description = (string) definition.Element("Description");
			string uiScreenshot = (string) definition.Attribute("UiScreenshot");
			string documentationLink = (string) definition.Attribute("DocumentationLink");
			string failMessage = (string) definition.Attribute("FailMessage");

			bool uploadDocumentOnSetup = (bool?) definition.Attribute("UploadDocumentOnSetup") ?? true;
			bool deleteDocumentOnTeardown = (bool?) definition.Attribute("DeleteDocumentOnTeardown") ?? true;

			XElement requestsDefinition = definition.Element("Requests");
			IEnumerable<IRequest> requests = RequestFactory.GetRequests(requestsDefinition);

			IEnumerable<IRequest> cleanupRequests = null;
			XElement cleanupRequestsDefinition = definition.Element("CleanupRequests");
			if (cleanupRequestsDefinition != null)
				cleanupRequests = RequestFactory.GetRequests(cleanupRequestsDefinition);

			ITestCase testCase = new TestCase(resourceId,
				requests,
				cleanupRequests,
				name,
				description,
				uploadDocumentOnSetup,
				deleteDocumentOnTeardown,
				category);

			testCase.UiScreenShot = uiScreenshot;
			testCase.DocumentationLink = documentationLink;
			testCase.FailMessage = failMessage;

			return testCase;
		}

		///<summary>
		/// This function helps ensure that,
		/// We are getting all the TestCases if the targetTestCategory is set to "All"
		/// We are getting all the TestCases with "WopiCore" as their "Category", regardless of the targetTestCategory.
		/// The rest of the test cases are picked up if their "Category" matches the targetTestCategory.
		///</summary>
		private static bool DoesTestCategoryMatchTargetTestCategory(XElement definition, TestCategory targetTestCategory)
		{
			string category = (string)definition.Attribute("Category");
			string name = (string)definition.Attribute("Name");

			if (string.IsNullOrEmpty(category))
			{
				throw new Exception(string.Format(CultureInfo.InvariantCulture, "The category of {0} TestCase is empty", name));
			}

			TestCategory testCaseCategory;
			if (!Enum.TryParse(category, true /* ignoreCase */, out testCaseCategory))
			{
				throw new Exception(string.Format(CultureInfo.InvariantCulture, "The category of {0} TestCase is invalid", name));
			}

			return targetTestCategory == TestCategory.All || testCaseCategory == TestCategory.WopiCore || targetTestCategory == testCaseCategory;
		}
	}
}
