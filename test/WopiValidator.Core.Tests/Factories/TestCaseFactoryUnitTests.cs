// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Office.WopiValidator.UnitTests.Factories
{
	[TestClass]
	public class TestCaseFactoryUnitTests
	{
		[TestMethod]
		public void CondenseMultilineString_Singleline()
		{
			string inputValue = "   Line 1.\r\nLine 2.    ";
			string expectedValue = "Line 1. Line 2.";
			string actualValue = TestCaseFactory.CondenseMultiLineString(inputValue);

			Assert.AreEqual(expectedValue, actualValue);
		}

		[TestMethod]
		public void CondenseMultilineString_Doubleline()
		{
			string input = "   Line 1.\r\n\r\n     Line 2.   ";
			string expectedValue = "Line 1." + Environment.NewLine + "Line 2.";
			string actualValue = TestCaseFactory.CondenseMultiLineString(input);

			Assert.AreEqual(expectedValue, actualValue);
		}

		[TestMethod]
		public void CondenseMultilineString_Multiline()
		{
			string input = "Line 1.   \n    \n    \n     Line 2.    \n\n\nLine 3.        ";
			string expectedValue = "Line 1." + Environment.NewLine + "Line 2." + Environment.NewLine + "Line 3.";
			string actualValue = TestCaseFactory.CondenseMultiLineString(input);

			Assert.AreEqual(expectedValue, actualValue);
		}

		[TestMethod]
		public void CondenseMultilineString_Mixed()
		{
			string input = "Line 1.\n  \t   Line 2.   \n\nLine 3.   \n\n\n\n\n   \t  Line 4.";
			string expectedValue = "Line 1. Line 2." + Environment.NewLine + "Line 3." + Environment.NewLine + "Line 4.";
			string actualValue = TestCaseFactory.CondenseMultiLineString(input);

			Assert.AreEqual(expectedValue, actualValue);
		}
	}
}
