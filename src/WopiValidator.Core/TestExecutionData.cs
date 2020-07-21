// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Office.WopiValidator.Core
{
	public class TestExecutionData
	{
		internal TestExecutionData(ITestCase testCase, IEnumerable<ITestCase> prereqCases, IResourceManager resourceManager, string testGroupName)
		{
			TestCase = testCase;
			PrereqCases = prereqCases;
			ResourceManager = resourceManager;
			TestGroupName = testGroupName;
		}

		public ITestCase TestCase { get; set; }
		public IResourceManager ResourceManager { get; private set; }
		public IEnumerable<ITestCase> PrereqCases { get; set; }
		public string TestGroupName { get; set; }

		public static TestExecutionData GetDataForSpecificTest(
			IEnumerable<TestExecutionData> testData,
			string testName)
		{
			Dictionary<string, TestExecutionData> executionDataDictionary = testData.ToDictionary(x => x.TestCase.Name);

			TestExecutionData data;
			if (executionDataDictionary.TryGetValue(testName, out data))
				return data;
			else
				throw new ArgumentException($"Could not find a test case named {testName}");
		}
	}
}
