// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core
{
	public enum TestCategory
	{
		All = 0,
		WopiCore = 1,
		OfficeOnline = 2,
		OfficeNativeClient = 3
	}

	internal static class TestCategoryExtensions
	{
		/// <summary>
		/// This function applies the rules of category filtering. If this returns true, the test should be included based on the category.
		///
		/// The rules to apply are as follows:
		/// If the filterCategory is All or null, the test should be included.
		/// If the test's category is WopiCore, it should be included (all WopiCore tests should always be included).
		/// If the test's category matches the filterCategory, it should be included.
		/// </summary>
		internal static bool TestCategoryMatches(this TestExecutionData testData, TestCategory? filterCategory)
		{
			return TestCategoryMatches(testData.TestCase, filterCategory);
		}

		/// <summary>
		/// This function applies the rules of category filtering. If this returns true, the test should be included based on the category.
		///
		/// The rules to apply are as follows:
		/// If the filterCategory is All or null, the test should be included.
		/// If the test's category is WopiCore, it should be included (all WopiCore tests should always be included).
		/// If the test's category matches the filterCategory, it should be included.
		/// </summary>
		internal static bool TestCategoryMatches(this ITestCase testCase, TestCategory? category)
		{
			if (!category.HasValue ||
				category == TestCategory.All ||
				testCase.TestCategory == TestCategory.WopiCore)
			{
				return true;
			}

			return testCase.TestCategory == category;
		}
	}
}
