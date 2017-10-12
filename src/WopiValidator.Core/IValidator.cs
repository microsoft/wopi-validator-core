// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Office.WopiValidator.Core
{
	public interface IValidator
	{
		string Name { get; }

		ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState);
	}
}
