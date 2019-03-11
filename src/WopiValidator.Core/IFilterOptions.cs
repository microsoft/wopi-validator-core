// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core
{
	public interface IFilterOptions
	{
		string TestName { get; set; }
		TestCategory? TestCategory { get; set; }
		string TestGroup { get; set; }
	}

	public static class IFilterOptionsExtensions
	{
		public static IEnumerable<TestExecutionData> ApplyFilters(this IEnumerable<TestExecutionData> testData, IFilterOptions options)
		{
			var toReturn = testData;

			// Filter by test name
			if (!string.IsNullOrEmpty(options.TestName))
			{
				toReturn = toReturn.Where(t => t.TestCase.Name == options.TestName);
				if (toReturn.Count() == 1)
				{
					return toReturn;
				}
			}

			if (options.TestCategory != null)
			{
				toReturn = toReturn.Where(t => t.TestCategoryMatches(options.TestCategory) == true);
			}

			if (!string.IsNullOrEmpty(options.TestGroup))
			{
				toReturn = toReturn.Where(t => t.TestGroupName.Equals(options.TestGroup, StringComparison.InvariantCultureIgnoreCase));
			}

			return toReturn;
		}

		public static IEnumerable<TestExecutionData> ApplyToData(this IFilterOptions filters, IEnumerable<TestExecutionData> testData)
		{
			return testData.ApplyFilters(filters);
		}
	}
}
