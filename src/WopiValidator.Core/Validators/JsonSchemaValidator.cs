// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.ResourceManagement;
using NJsonSchema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using JsonValidation = NJsonSchema.Validation;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	class JsonSchemaValidator : IValidator
	{
		private readonly JsonSchema4 _schema;
		private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		
		// Timestamp properties that need Unix timestamp validation (seconds since epoch)
		private static readonly HashSet<string> UnixTimestampProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"AccessTokenExpiry",
			"ServerTime"
		};

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
			List<string> errorMessages = new List<string>();

			// Perform standard JSON schema validation
			if (errors.Count > 0)
			{
				var grouped = errors.GroupBy(error => error.Kind);

				foreach (var group in grouped)
				{
					var errorKind = ValidationErrorKindString(group.Key);
					var properties = group.Select(error => error.Property);
					string propertiesString = String.Join(", ", properties);
					errorMessages.Add($"{errorKind}: {propertiesString}");
				}
			}

			// Perform additional timestamp validation for Unix timestamp properties
			try
			{
				JObject jObject = JObject.Parse(jsonContent);
				var timestampErrors = ValidateTimestampProperties(jObject);
				errorMessages.AddRange(timestampErrors);
			}
			catch (Exception ex)
			{
				errorMessages.Add($"Error parsing JSON for timestamp validation: {ex.Message}");
			}

			if (errorMessages.Count == 0)
			{
				return new ValidationResult();
			}

			return new ValidationResult(errorMessages.ToArray());
		}

		private List<string> ValidateTimestampProperties(JObject jObject)
		{
			List<string> errors = new List<string>();
			DateTime currentTime = DateTime.UtcNow;
			DateTime minValidTime = currentTime.AddYears(-10);
			DateTime maxValidTime = currentTime.AddYears(10);

			foreach (var property in UnixTimestampProperties)
			{
				JToken token = jObject[property];
				if (token != null && token.Type == JTokenType.Integer)
				{
					try
					{
						long unixTimestamp = token.Value<long>();
						
						// Convert Unix timestamp (seconds since epoch) to DateTime
						DateTime dateTime;
						try
						{
							dateTime = UnixEpoch.AddSeconds(unixTimestamp);
						}
						catch (ArgumentOutOfRangeException)
						{
							errors.Add($"Property '{property}' has invalid Unix timestamp value '{unixTimestamp}' that cannot be converted to a valid DateTime.");
							continue;
						}

						// Check if timestamp is within reasonable bounds (current time ± 10 years)
						if (dateTime < minValidTime || dateTime > maxValidTime)
						{
							errors.Add($"Property '{property}' has timestamp value '{unixTimestamp}' ('{dateTime:yyyy-MM-dd HH:mm:ss} UTC') that is outside the valid range of {minValidTime:yyyy-MM-dd HH:mm:ss} UTC to {maxValidTime:yyyy-MM-dd HH:mm:ss} UTC. Timestamps should be within 10 years of current time.");
						}
					}
					catch (Exception ex)
					{
						errors.Add($"Property '{property}' timestamp validation failed: {ex.Message}");
					}
				}
				else if (token != null && token.Type == JTokenType.Float)
				{
					try
					{
						double unixTimestamp = token.Value<double>();
						
						// Convert Unix timestamp (seconds since epoch) to DateTime
						DateTime dateTime;
						try
						{
							dateTime = UnixEpoch.AddSeconds(unixTimestamp);
						}
						catch (ArgumentOutOfRangeException)
						{
							errors.Add($"Property '{property}' has invalid Unix timestamp value '{unixTimestamp}' that cannot be converted to a valid DateTime.");
							continue;
						}

						// Check if timestamp is within reasonable bounds (current time ± 10 years)
						if (dateTime < minValidTime || dateTime > maxValidTime)
						{
							errors.Add($"Property '{property}' has timestamp value '{unixTimestamp}' ('{dateTime:yyyy-MM-dd HH:mm:ss} UTC') that is outside the valid range of {minValidTime:yyyy-MM-dd HH:mm:ss} UTC to {maxValidTime:yyyy-MM-dd HH:mm:ss} UTC. Timestamps should be within 10 years of current time.");
						}
					}
					catch (Exception ex)
					{
						errors.Add($"Property '{property}' timestamp validation failed: {ex.Message}");
					}
				}
			}

			return errors;
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
