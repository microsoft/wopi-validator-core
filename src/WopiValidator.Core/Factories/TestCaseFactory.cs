// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	public class TestCaseFactory : ITestCaseFactory
	{
		public IEnumerable<ITestCase> GetTestCases(XElement definitions)
		{
			return definitions.Elements("TestCase").Select(x => GetTestCase(x));
		}

		public void GetTestCases(
			XElement definition,
			Dictionary<string, ITestCase> prereqCasesDictionary,
			out IEnumerable<ITestCase> prereqTests,
			out IEnumerable<ITestCase> groupTests)
		{
			XElement prereqsElement = definition.Element("PrereqTests") ?? new XElement("PrereqTests");
			prereqTests = GetPrereqTests(prereqsElement, prereqCasesDictionary);

			XElement testCasesElement = definition.Element("TestCases") ?? new XElement("TestCases");
			groupTests = GetTestCases(testCasesElement);
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
		/// User RequestFactory.GetRequests to parse requests defined in that Test Case.
		/// </summary>
		private static ITestCase GetTestCase(XElement definition)
		{
			string category = (string)definition.Attribute("Category");
			string name = (string)definition.Attribute("Name");
			string resourceId = (string)definition.Attribute("Document");
			string description = (string)definition.Element("Description");
			string uiScreenshot = (string)definition.Attribute("UiScreenshot");
			string documentationLink = (string)definition.Attribute("DocumentationLink");
			string failMessage = (string)definition.Attribute("FailMessage");

			bool uploadDocumentOnSetup = (bool?)definition.Attribute("UploadDocumentOnSetup") ?? true;
			bool deleteDocumentOnTeardown = (bool?)definition.Attribute("DeleteDocumentOnTeardown") ?? true;

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
				CondenseMultiLineString(description),
				uploadDocumentOnSetup,
				deleteDocumentOnTeardown,
				category);

			testCase.UiScreenShot = uiScreenshot;
			testCase.DocumentationLink = documentationLink;
			testCase.FailMessage = failMessage;

			return testCase;
		}

		/// <summary>
		/// Condenses a multi-line string into a more compact form.
		///
		/// A single new-line will be converted to a space in the output.
		/// 2+ consecutive new-lines will be condensed to a single new-line in the output.
		/// Leading and trailing whitespace will be removed from each line.
		/// </summary>
		internal static string CondenseMultiLineString(string input)
		{
			const string doubleNewLinePlaceholder = "|-DOUBLE-|";

			// Trim leading/trailing whitespace from each line
			input = input.Trim();
			input = Regex.Replace(input, @"^[ \t]+|[ \t]+$", "", RegexOptions.Multiline);

			// Condense two or more new-lines to a placeholder string
			input = Regex.Replace(input, @"(\r\n){2,}|\n{2,}", doubleNewLinePlaceholder);

			// Convert remaining single new-lines to a space
			input = Regex.Replace(input, @"(\r\n)+|\n+", " ");

			// Replace the double new-line placeholder with a new-line
			input = input.Replace(doubleNewLinePlaceholder, Environment.NewLine);

			return input;
		}
	}
}
