// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Office.WopiValidator.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Office.WopiValidator
{
	/// <summary>
	/// Options shared by all commands
	/// </summary>
	internal abstract class OptionsBase : IFilterOptions
	{
		[Option('c', "config", Required = false, Default = "TestCases.xml", HelpText = "Path to XML file with test definitions")]
		public string RunConfigurationFilePath { get; set; }

		[Option('g', "testgroup", Required = false, HelpText = "Run only the tests in the specified group (cannot be used with testname)")]
		public string TestGroup { get; set; }

		[Option('n', "testname", Required = false, HelpText = "Run only the test specified (cannot be used with testgroup)")]
		public string TestName { get; set; }

		[Option('e', "testcategory", Required = false, Default = Core.TestCategory.All, HelpText = "Run only the tests in the specified category")]
		public TestCategory TestCategory { get; set; }

		TestCategory? IFilterOptions.TestCategory
		{
			get { return TestCategory; }
			set
			{
				if (!value.HasValue)
				{
					TestCategory = TestCategory.All;
				}
				TestCategory = value.Value;
			}
		}
	}
}
