// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Office.WopiValidator.Core.Factories;
using Microsoft.Office.WopiValidator.Core.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core
{
	public static class ConfigParser
	{
		public static IEnumerable<TestExecutionData> ParseExecutionData(
			string filePath,
			string testGroupName,
			TestCategory targetTestCategory,
			ILoggerFactory loggerFactory)
		{
			return ParseExecutionData(filePath, new ResourceManagerFactory(loggerFactory), new TestCaseFactory(), testGroupName, targetTestCategory, loggerFactory);
		}

		public static IEnumerable<TestExecutionData> ParseExecutionData(
			string filePath,
			TestCategory targetTestCategory,
			ILoggerFactory loggerFactory)
		{
			return ParseExecutionData(filePath, new ResourceManagerFactory(loggerFactory), new TestCaseFactory(), string.Empty, targetTestCategory, loggerFactory);
		}

		/// <summary>
		/// Parses run configuration XML file to get ExecutionData.
		/// </summary>
		public static IEnumerable<TestExecutionData> ParseExecutionData(
			string filePath,
			IResourceManagerFactory resourceManagerFactory,
			ITestCaseFactory testCaseFactory,
			string testGroupName,
			TestCategory targetTestCategory,
			ILoggerFactory loggerFactory)
		{
			ILogger logger = loggerFactory.CreateLogger("configParser");
			logger.Log($"Loading tests from '{filePath}'.");
			XDocument xDoc = XDocument.Load(filePath);

			XElement resourcesElement = xDoc.Root.Element("Resources");
			IResourceManager resourceManager = resourceManagerFactory.GetResourceManager(resourcesElement);

			XElement prereqCasesElement = xDoc.Root.Element("PrereqCases") ?? new XElement("PrereqCases");
			IEnumerable<ITestCase> prereqCases = testCaseFactory.GetTestCases(prereqCasesElement, targetTestCategory);
			Dictionary<string, ITestCase> prereqCasesDictionary = prereqCases.ToDictionary(e => e.Name);

			return xDoc.Root.Elements("TestGroup")
				.SelectMany(x => GetTestExecutionDataForGroup(x, prereqCasesDictionary, testCaseFactory, resourceManager, targetTestCategory, loggerFactory));
		}

		private static IEnumerable<TestExecutionData> GetTestExecutionDataForGroup(
			XElement definition,
			Dictionary<string, ITestCase> prereqCasesDictionary,
			ITestCaseFactory testCaseFactory,
			IResourceManager resourceManager,
			TestCategory targetTestCategory,
			ILoggerFactory loggerFactory)
		{
			IEnumerable<ITestCase> prereqs;
			IEnumerable<ITestCase> groupTestCases;
			testCaseFactory.GetTestCases(definition, prereqCasesDictionary, out prereqs, out groupTestCases, targetTestCategory);

			List<ITestCase> prereqList = prereqs.ToList();

			return groupTestCases.Select(testcase =>
				new TestExecutionData(testcase, prereqList, resourceManager, (string)definition.Attribute("Name")));
		}
	}
}
