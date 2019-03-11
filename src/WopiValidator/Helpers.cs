// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Office.WopiValidator
{
	internal static class Helpers
	{
		internal static void WriteToConsole(string message, ConsoleColor color, int indentLevel = 0)
		{
			ConsoleColor currentColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			string indent = new string(' ', indentLevel * 2);
			Console.Write(indent + message);
			Console.ForegroundColor = currentColor;
		}

		internal static bool ContainsAny<T>(this HashSet<T> set, params T[] items)
		{
			return set.Intersect(items).Any();
		}

		internal static string StripNewLines(this string str)
		{
			StringBuilder sb = new StringBuilder(str);
			bool newLineAtStart = str.StartsWith(Environment.NewLine);
			bool newLineAtEnd = str.EndsWith(Environment.NewLine);
			sb.Replace(Environment.NewLine, " ");

			if (newLineAtStart)
			{
				sb.Insert(0, Environment.NewLine);
			}

			if (newLineAtEnd)
			{
				sb.Append(Environment.NewLine);
			}
			return sb.ToString();
		}

	}
}
