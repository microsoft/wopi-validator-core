// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.ResourceManagement;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	class JsonSchemaValidator : IValidator
	{
		private readonly JsonSchema4 _schema;

		public JsonSchemaValidator(string schemaId)
		{
			if (!JsonSchemas.Schemas.TryGetValue(schemaId, out _schema))
			{
				throw new ArgumentException($"Schema with ID '{schemaId}' not found.");
			}
		}

		public string Name
		{
			get { return "JsonSchemaValidator"; }
		}

		public ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState)
		{
			string responseContentString = data.GetResponseContentAsString();

			if (!data.IsTextResponse)
			{
				throw new NotSupportedException(String.Format(CultureInfo.CurrentCulture, "The JsonSchemaValidator can only be used on requests that have a JSON response."));
			}

			if (String.IsNullOrEmpty(responseContentString))
			{
				return new ValidationResult(String.Format(CultureInfo.CurrentCulture, "Couldn't read response content."));
			}

			return ValidateJsonContent(responseContentString);
		}

		private ValidationResult ValidateJsonContent(string jsonContent)
		{
			var errors = _schema.Validate(jsonContent);
			if (errors.Count == 0)
			{
				return new ValidationResult();
			}

			List<string> errorMessages = new List<string>();
			var grouped = errors.GroupBy(error => error.Kind);

			foreach (var group in grouped)
			{
				string errorMessageForEachKind = string.Empty;
				foreach (var element in group)
				{
					errorMessageForEachKind = errorMessageForEachKind + element.ToString();
				}
				errorMessages.Add($"{errorMessageForEachKind}");
			}

			return new ValidationResult(errorMessages.ToArray());
		}
	}
}
