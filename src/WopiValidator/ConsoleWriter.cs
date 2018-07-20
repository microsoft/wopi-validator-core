// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Office.WopiValidator
{
	public static class ConsoleWriter
	{
		public static void Write(string message, ConsoleColor color = ConsoleColor.Gray, int indentLevel = 0, bool addNewLine = true)
		{
			ConsoleColor currentColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			string indent = new string(' ', indentLevel * 2);
			if (addNewLine)
			{
				Console.WriteLine(indent + message);
			}
			else
			{
				Console.Write(indent + message);
			}
			Console.ForegroundColor = currentColor;
		}
	}
}
