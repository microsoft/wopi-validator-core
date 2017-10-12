// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Validates that response content is a JSON encoded string that contains provided set of properties with values matching expecting ones.
	/// </summary>
	class JsonContentValidator : IValidator
	{
		private readonly IJsonPropertyValidator[] _propertyValidators;

		public JsonContentValidator(IJsonPropertyValidator propertyValidator = null)
		{
			_propertyValidators = propertyValidator == null ? new IJsonPropertyValidator[0] : new[] { propertyValidator };
		}

		public JsonContentValidator(IEnumerable<IJsonPropertyValidator> propertyValidators)
		{
			_propertyValidators = (propertyValidators ?? Enumerable.Empty<IJsonPropertyValidator>()).ToArray();
		}

		public string Name
		{
			get { return "JsonContentValidator"; }
		}

		public ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState)
		{
			string responseContentString = data.GetResponseContentAsString();
			if (!data.IsTextResponse || String.IsNullOrEmpty(responseContentString))
				return new ValidationResult("Couldn't read resource content.");

			return ValidateJsonContent(responseContentString, savedState);
		}

		private ValidationResult ValidateJsonContent(string jsonString, Dictionary<string, string> savedState)
		{
			try
			{
				JObject jObject = JObject.Parse(jsonString);

				List<string> errors = new List<string>();
				foreach (IJsonPropertyValidator propertyValidator in _propertyValidators)
				{
					JToken propertyValue = jObject.SelectToken(propertyValidator.Key);

					string errorMessage;
					bool result = propertyValidator.Validate(propertyValue, savedState, out errorMessage);

					if (!result)
						errors.Add(string.Format("Incorrect value for '{0}' property. {1}", propertyValidator.Key, errorMessage));
				}

				if (errors.Count == 0)
					return new ValidationResult();

				return new ValidationResult(errors.ToArray());
			}
			catch (JsonException ex)
			{
				return new ValidationResult(
					string.Format("Exception thrown when trying to validate content as JSON: '{0}', '{1}'", ex.Message, jsonString));
			}
		}

		public interface IJsonPropertyValidator
		{
			string Key { get; }
			bool Validate(JToken actualValue, Dictionary<string, string> savedState, out string errorMessage);
		}

		public abstract class JsonPropertyValidator : IJsonPropertyValidator
		{
			protected JsonPropertyValidator(string key, bool isRequired)
			{
				Key = key;
				IsRequired = isRequired;
			}

			protected bool IsActualValueNullOrEmpty(JToken actualValue)
			{
				return (actualValue == null) ||
						(actualValue.Type == JTokenType.Array && !actualValue.HasValues) ||
						(actualValue.Type == JTokenType.Object && !actualValue.HasValues) ||
						(actualValue.Type == JTokenType.String && string.IsNullOrEmpty(actualValue.Value<string>())) ||
						(actualValue.Type == JTokenType.Null);
			}

			public string Key { get; private set; }

			public bool IsRequired { get; private set; }

			public abstract bool Validate(JToken actualValue, Dictionary<string, string> savedState, out string errorMessage);
		}


		public class JsonAbsoluteUrlPropertyValidator : JsonPropertyValidator
		{
			public string ExpectedStateKey { get; private set; }

			public JsonAbsoluteUrlPropertyValidator(string key, bool isRequired, string expectedStateKey)
				: base(key, isRequired)
			{
				ExpectedStateKey = expectedStateKey;
			}

			public override bool Validate(JToken actualValue, Dictionary<string, string> savedState, out string errorMessage)
			{
				bool isValid = false;
				errorMessage = null;

				if (IsActualValueNullOrEmpty(actualValue))
				{
					if (IsRequired)
					{
						errorMessage = string.Format("Value is required but not provided.");
					}
					else
					{
						isValid = true;
					}
				}
				else
				{
					string value = actualValue.Value<string>();

					Uri uri;
					if (Uri.TryCreate(value, UriKind.Absolute, out uri))
					{
						isValid = true;
					}
					else
					{
						errorMessage = string.Format("Cannot parse {0} as absolute URL", value);
					}
				}
				return isValid;
			}
		}

		public abstract class JsonPropertyEqualityValidator<T> : JsonPropertyValidator
			where T : IEquatable<T>
		{
			protected JsonPropertyEqualityValidator(string key, bool isRequired, T expectedValue, bool hasExpectedValue, string expectedStateKey)
				: base(key, isRequired)
			{
				DefaultExpectedValue = expectedValue;
				HasExpectedValue = hasExpectedValue;
				ExpectedStateKey = expectedStateKey;
			}

			public override bool Validate(JToken actualValue, Dictionary<string, string> savedState, out string errorMessage)
			{
				if (IsActualValueNullOrEmpty(actualValue))
				{
					if (IsRequired)
					{
						errorMessage = string.Format(CultureInfo.CurrentCulture, "Required property missing");
						return false;
					}
					else
					{
						errorMessage = "";
						return true;
					}
				}

				// If the "ExpectedValue" and "ExpectedStateKey" attributes are non-empty on a Validator, then ExpectedStateKey will take precedence.
				// But if the mentioned "ExpectedStateKey" is invalid or doesn't have a saved state value, then the logic below will default to the value set in 
				// "ExpectedValue" attribute of the Validator.
				T expectedValue = DefaultExpectedValue;
				bool hasExpectedStateValue = false;
				if (savedState != null && ExpectedStateKey != null && savedState.ContainsKey(ExpectedStateKey) && !string.IsNullOrEmpty(savedState[ExpectedStateKey]))
				{
					try
					{
						expectedValue = (T)Convert.ChangeType(savedState[ExpectedStateKey], typeof(T));
						hasExpectedStateValue = true;
					}
					catch (FormatException)
					{
						if (!HasExpectedValue)
						{
							errorMessage = string.Format(CultureInfo.CurrentCulture, "ExpectedStateValue should be of type : {0}", typeof(T).FullName);
							return false;
						}
					}
				}

				if (!HasExpectedValue && !hasExpectedStateValue)
				{
					errorMessage = "";
					return true;
				}

				return Compare(actualValue, expectedValue, out errorMessage);
			}

			protected virtual bool Compare(JToken actualValue, T expectedValue, out string errorMessage)
			{
				string formattedActualValue;
				bool isValid = false;
				try
				{
					T typedActualValue = actualValue.Value<T>();
					formattedActualValue = FormatValue(typedActualValue);

					isValid = typedActualValue.Equals(expectedValue);
				}
				catch (FormatException)
				{
					formattedActualValue = actualValue.Value<string>();
					isValid = false;
				}

				errorMessage = string.Format(CultureInfo.CurrentCulture, "Expected: '{0}', Actual: '{1}'", FormattedExpectedValue, formattedActualValue);
				return isValid;
			}

			public T DefaultExpectedValue { get; private set; }

			public bool HasExpectedValue { get; private set; }

			public string ExpectedStateKey { get; private set; }

			public string FormattedExpectedValue { get { return FormatValue(DefaultExpectedValue); } }

			public abstract string FormatValue(T value);
		}

		public class JsonIntegerPropertyValidator : JsonPropertyEqualityValidator<int>
		{
			public JsonIntegerPropertyValidator(string key, bool isRequired, int expectedValue, bool hasExpectedValue, string expectedStateKey)
				: base(key, isRequired, expectedValue, hasExpectedValue, expectedStateKey)
			{
			}

			public override string FormatValue(int value)
			{
				return value.ToString(CultureInfo.InvariantCulture);
			}
		}

		public class JsonLongPropertyValidator : JsonPropertyEqualityValidator<long>
		{
			public JsonLongPropertyValidator(string key, bool isRequired, long expectedValue, bool hasExpectedValue, string expectedStateKey)
				: base(key, isRequired, expectedValue, hasExpectedValue, expectedStateKey)
			{
			}

			public override string FormatValue(long value)
			{
				return value.ToString(CultureInfo.InvariantCulture);
			}
		}

		public class JsonBooleanPropertyValidator : JsonPropertyEqualityValidator<bool>
		{
			public JsonBooleanPropertyValidator(string key, bool isRequired, bool expectedValue, bool hasExpectedValue, string expectedStateKey)
				: base(key, isRequired, expectedValue, hasExpectedValue, expectedStateKey)
			{
			}

			public override string FormatValue(bool value)
			{
				return value.ToString(CultureInfo.InvariantCulture);
			}
		}

		public class JsonStringPropertyValidator : JsonPropertyEqualityValidator<string>
		{
			private string _endsWithValue;

			public JsonStringPropertyValidator(string key, bool isRequired, string expectedValue, bool hasExpectedValue, string endsWithValue, string expectedStateKey)
				: base(key, isRequired, expectedValue, hasExpectedValue, expectedStateKey)
			{
				_endsWithValue = endsWithValue;
			}

			public override string FormatValue(string value)
			{
				return value;
			}

			public override bool Validate(JToken actualValue, Dictionary<string, string> savedState, out string errorMessage)
			{
				if (!base.Validate(actualValue, savedState, out errorMessage))
					return false;

				errorMessage = "";
				if (String.IsNullOrWhiteSpace(_endsWithValue))
					return true;

				string typedActualValue = actualValue.Value<string>();
				string formattedActualValue = FormatValue(typedActualValue);

				if (!formattedActualValue.EndsWith(_endsWithValue))
				{
					errorMessage = string.Format("Expected to end with: '{0}', Actual: '{1}'", _endsWithValue, formattedActualValue);
					return false;
				}

				return true;
			}
		}

		public class JsonArrayPropertyValidator : JsonPropertyEqualityValidator<string>
		{
			public JsonArrayPropertyValidator(string key, bool isRequired, string containsValue, bool hasContainsValue, string expectedStateKey)
				: base(key, isRequired, containsValue, hasContainsValue, expectedStateKey)
			{
			}

			protected override bool Compare(JToken actualArrayOfValues, string expectedValue, out string errorMessage)
			{
				string formattedActualValue;
				bool isValid = false;

				try
				{
					IList<string> typedActualValue = actualArrayOfValues.ToObject<List<string>>();
					formattedActualValue = typedActualValue.ToString();

					isValid = typedActualValue.Contains(expectedValue, StringComparer.OrdinalIgnoreCase);
				}
				catch (FormatException)
				{
					formattedActualValue = "";
					isValid = false;
				}

				errorMessage = string.Format(CultureInfo.CurrentCulture, "Expected: '{0}', Actual: '{1}'", FormattedExpectedValue, formattedActualValue);
				return isValid;
			}

			public override string FormatValue(string value)
			{
				return value;
			}
		}
	}
}
