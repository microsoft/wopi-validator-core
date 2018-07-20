// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Microsoft.Office.WopiValidator.Core.Logging
{
	public static class ApplicationLogging
	{
		public static ILoggerFactory LoggerFactory { get; } = new LoggerFactory();
		public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();

		public static void Log(this ILogger logger, string message)
		{
			logger.Log(LogLevel.Debug, message);
		}
	}
}
