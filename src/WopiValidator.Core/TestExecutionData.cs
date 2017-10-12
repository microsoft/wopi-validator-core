// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core
{
	public enum TestCategory
	{
		All = 0,
		WopiCore = 1,
		OfficeOnline = 2,
		OfficeNativeClient = 3
	}

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
