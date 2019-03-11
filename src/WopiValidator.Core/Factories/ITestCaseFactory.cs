// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	public interface ITestCaseFactory
	{
		/// <summary>
		/// Parse XML run configuration to get list of Test Cases
		/// </summary>
		/// <param name="definitions"><![CDATA[<TestCases>]]> element from run configuration XML file.</param>
		/// <returns>Collection of Test Cases.</returns>
		IEnumerable<ITestCase> GetTestCases(
			XElement definitions);

		/// <summary>
		/// Parse XML run configuration testgroup element to get a list of TestCases.
		/// </summary>
		/// <param name="definition"><![CDATA[<TestGroup>]]> element from run configuration XML file.</param>
		/// <param name="prereqCasesDictionary">Dictionary of name to testcase already parsed from <![CDATA[<PrereqCases>]]> element from run configuration file.</param>
		/// <param name="prereqTests">PrereqCases applicable to testcases in this test group.</param>
		/// <param name="groupTests">TestCases in this test group.</param>
		void GetTestCases(XElement definition,
			Dictionary<string, ITestCase> prereqCasesDictionary,
			out IEnumerable<ITestCase> prereqTests,
			out IEnumerable<ITestCase> groupTests);
	}
}
