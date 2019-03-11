// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.Factories;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core
{
	public static class ConfigParser
	{
		public static IEnumerable<TestExecutionData> ParseExecutionData(string filePath, string applicationId = null, string usingRestrictedScenario = null)
		{
			return ParseExecutionData(filePath, new ResourceManagerFactory(), new TestCaseFactory(), applicationId, usingRestrictedScenario);
		}

		/// <summary>
		/// Parses run configuration XML file to get ExecutionData.
		/// </summary>
		public static IEnumerable<TestExecutionData> ParseExecutionData(
			string filePath,
			IResourceManagerFactory resourceManagerFactory,
			ITestCaseFactory testCaseFactory,
			string applicationId,
			string usingRestrictedScenario)
		{
			XDocument xDoc = XDocument.Load(filePath);

			XElement resourcesElement = xDoc.Root.Element("Resources");
			IResourceManager resourceManager = resourceManagerFactory.GetResourceManager(resourcesElement);

			XElement prereqCasesElement = xDoc.Root.Element("PrereqCases") ?? new XElement("PrereqCases");
			IEnumerable<ITestCase> prereqCases = testCaseFactory.GetTestCases(prereqCasesElement, applicationId, usingRestrictedScenario);
			Dictionary<string, ITestCase> prereqCasesDictionary = prereqCases.ToDictionary(e => e.Name);

			return xDoc.Root.Elements("TestGroup")
				.SelectMany(x => GetTestExecutionDataForGroup(x, prereqCasesDictionary, testCaseFactory, resourceManager, applicationId, usingRestrictedScenario));
		}

		private static IEnumerable<TestExecutionData> GetTestExecutionDataForGroup(
			XElement definition,
			Dictionary<string, ITestCase> prereqCasesDictionary,
			ITestCaseFactory testCaseFactory,
			IResourceManager resourceManager,
			string applicationId,
			string usingRestrictedScenario)
		{
			IEnumerable<ITestCase> prereqs;
			IEnumerable<ITestCase> groupTestCases;
			testCaseFactory.GetTestCases(definition, prereqCasesDictionary, out prereqs, out groupTestCases, applicationId, usingRestrictedScenario);

			List<ITestCase> prereqList = prereqs.ToList();

			return groupTestCases.Select(testcase =>
				new TestExecutionData(testcase, prereqList, resourceManager, (string) definition.Attribute("Name")));
		}
	}
}
