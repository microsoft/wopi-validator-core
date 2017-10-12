// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core.Logging
{
	interface ILogger
	{
		void Log(string message);

		void Log(string pattern, params object[] args);
	}
}
