// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using JsonValidation = NJsonSchema.Validation;
using Microsoft.Office.WopiValidator.Core.ResourceManagement;
using System.Text.RegularExpressions;

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
			if(!data.IsTextResponse)
			{
				throw new NotSupportedException("The JsonSchemaValidator can only be used on requests that have a JSON response.");
			}

			if (String.IsNullOrEmpty(responseContentString))
			{
				return new ValidationResult("Couldn't read response content.");
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
				
			foreach (var theGroup in grouped)
			{
				var errorKind = ValidationErrorKindString(theGroup.Key);
				var properties = theGroup.Select(error => error.Property);
				string propertiesString = String.Join(", ", properties);
				errorMessages.Add($"{errorKind}: {propertiesString}");
			}

			return new ValidationResult(errorMessages.ToArray());
		}

		private string ValidationErrorKindString(JsonValidation.ValidationErrorKind kind)
		{
			switch (kind)
			{
				case JsonValidation.ValidationErrorKind.NoAdditionalPropertiesAllowed:
					return "Unknown Properties";

				default:
					return SpaceIntercappedString(kind.ToString());
			}
		}

		private string SpaceIntercappedString(string s)
		{
			return Regex.Replace(s, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
		}
	}
}
