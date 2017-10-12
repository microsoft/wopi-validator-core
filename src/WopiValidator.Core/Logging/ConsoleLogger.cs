// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Office.WopiValidator.Core.Logging
{
	/// <summary>
	/// ILogger implementation that uses standard console output to print information to the user.
	/// </summary>
	class ConsoleLogger : ILogger
	{
		private static ILogger _instance;
		public static ILogger Instance { get { return _instance ?? (_instance = new ConsoleLogger()); } }

		public void Log(string message)
		{
			Console.WriteLine(message);
		}

		public void Log(string pattern, params object[] args)
		{
			Log(string.Format(pattern, args));
		}
	}
}
