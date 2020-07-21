// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;
using System;
using System.Diagnostics;

namespace Microsoft.Office.WopiValidator
{
	internal enum ExitCode
	{
		Success = 0,
		Failure = 1,
	}

	internal class Program
	{
		private static int Main(string[] args)
		{
			// Wrapping all logic in a top-level Exception handler to ensure that exceptions are
			// logged to the console and don't cause Windows Error Reporting to kick in.
			ExitCode exitCode = ExitCode.Success;
			try
			{
				exitCode = Parser.Default.ParseArguments<RunOptions, ListOptions, DiscoveryOptions>(args)
				.MapResult(
					(RunOptions options) => RunOptions.RunCommand(options),
					(ListOptions options) => ListOptions.ListCommand(options),
					(DiscoveryOptions options) => DiscoveryOptions.DiscoveryCommand(options),
					parseErrors => ExitCode.Failure);
			}
			catch (Exception ex)
			{
				Helpers.WriteToConsole(ex.ToString(), ConsoleColor.Red);
				exitCode = ExitCode.Failure;
			}

			if (Debugger.IsAttached)
			{
				Helpers.WriteToConsole("Press any key to exit", ConsoleColor.White);
				Console.ReadLine();
			}
			return (int)exitCode;
		}
	}
}
