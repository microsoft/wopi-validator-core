// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core
{
	public interface IStateEntry
	{
		string Name { get; }

		string Source { get; }

		string GetValue(IResponseData data);
	}
}
