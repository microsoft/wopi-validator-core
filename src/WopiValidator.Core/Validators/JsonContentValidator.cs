// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Office.WopiValidator.Core.Validators
{
	/// <summary>
	/// Validates that response content is a JSON encoded string that contains provided set of properties with values matching expecting ones.
	/// </summary>
	internal class JsonContentValidator : IValidator
	{
		private readonly IJsonPropertyValidator[] _propertyValidators;
		private readonly bool _shouldExist;

		public JsonContentValidator(IJsonPropertyValidator propertyValidator = null)
		{
			_propertyValidators = propertyValidator == null ? new IJsonPropertyValidator[0] : new[] { propertyValidator };
			_shouldExist = true;
		}

		public JsonContentValidator(IEnumerable<IJsonPropertyValidator> propertyValidators, bool shouldExist)
		{
			_propertyValidators = (propertyValidators ?? Enumerable.Empty<IJsonPropertyValidator>()).ToArray();
			_shouldExist = shouldExist;
		}

		public string Name
		{
			get { return "JsonContentValidator"; }
		}

		public ValidationResult Validate(IResponseData data, IResourceManager resourceManager, Dictionary<string, string> savedState)
		{
			string responseContentString = data.GetResponseContentAsString();
			if (!data.IsTextResponse || String.IsNullOrEmpty(responseContentString))
			{
				if (_shouldExist)
				{
					return new ValidationResult("Response body should exist, but couldn't read resource content.");
				}
				else
				{
					return new ValidationResult();
				}
			}

			if (!_shouldExist)
			{
				return new ValidationResult("Response body shouldn't exist.");
			}

			return ValidateJsonContent(responseContentString, savedState);
		}

		private ValidationResult ValidateJsonContent(string jsonString, Dictionary<string, string> savedState)
		{
			try
			{
				JObject jObject = jsonString.ParseJObject();

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
			catch (JsonReaderException ex)
			{
				return new ValidationResult($"{Name}: {ex.GetType().Name} thrown while parsing JSON. Are you sure the response is JSON?");
			}
			catch (JsonException ex)
			{
				return new ValidationResult($"{Name}: {ex.GetType().Name} thrown while parsing JSON content: '{ex.Message}'");
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
			private readonly bool _mustIncludeAccessToken = false;

			public JsonAbsoluteUrlPropertyValidator(string key, bool isRequired, bool mustIncludeAccessToken, string expectedStateKey)
				: base(key, isRequired)
			{
				ExpectedStateKey = expectedStateKey;
				_mustIncludeAccessToken = mustIncludeAccessToken;
			}

			public override bool Validate(JToken actualValue, Dictionary<string, string> savedState, out string errorMessage)
			{
				errorMessage = null;

				if (IsActualValueNullOrEmpty(actualValue))
				{
					if (IsRequired)
					{
						errorMessage = "Value is required but not provided.";
						return false;
					}

					return true;
				}
				else
				{
					string value = actualValue.Value<string>();

					Uri uri;
					if (Uri.TryCreate(value, UriKind.Absolute, out uri))
					{
						if (_mustIncludeAccessToken && IncludesAccessToken(value))
						{
							errorMessage = $"URL '{value}' does not include the 'access_token' query parameter";
							return false;
						}

						return true;
					}
					else
					{
						errorMessage = string.Format("Cannot parse {0} as absolute URL", value);
						return false;
					}
				}
			}

			/// <summary>
			/// Returns true if the URI includes an access_token query string parameter; false otherwise.
			/// </summary>
			private bool IncludesAccessToken(string url)
			{
				return UrlHelper.GetQueryParameterValue(url, "access_token") == null;
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

			public T DefaultExpectedValue { get; protected set; }

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
			private readonly string _endsWithValue;
			private readonly bool _ignoreCase;
			private readonly StringComparison _comparisonType;

			public JsonStringPropertyValidator(string key, bool isRequired, string expectedValue, bool hasExpectedValue, string endsWithValue, string expectedStateKey, bool ignoreCase = false)
				: base(key, isRequired, expectedValue, hasExpectedValue, expectedStateKey)
			{
				_endsWithValue = endsWithValue;
				_ignoreCase = ignoreCase;
				_comparisonType = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
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

				if (!formattedActualValue.EndsWith(_endsWithValue, _comparisonType))
				{
					errorMessage = string.Format("Expected to end with: '{0}', Actual: '{1}'", _endsWithValue, formattedActualValue);
					return false;
				}

				return true;
			}

			protected override bool Compare(JToken actualValue, string expectedValue, out string errorMessage)
			{
				if (!_ignoreCase)
				{
					return base.Compare(actualValue, expectedValue, out errorMessage);
				}

				string formattedActualValue = FormatValue(actualValue.Value<string>());
				bool isValid = formattedActualValue.Equals(expectedValue, _comparisonType);
				errorMessage = string.Format(CultureInfo.CurrentCulture, "Expected: '{0} (case-insensitive)', Actual: '{1}'", expectedValue, formattedActualValue);
				return isValid;
			}
		}

		public class JsonFileNamePropertyValidator : JsonStringPropertyValidator
		{
			public JsonFileNamePropertyValidator(string key, string expectedValue, string guid, bool ignoreCase = false)
				: base(key, true, expectedValue, true, null, null, ignoreCase)
				{
					DefaultExpectedValue = string.Format("{0}-{1}{2}",
						System.IO.Path.GetFileNameWithoutExtension(DefaultExpectedValue),
						guid,
						System.IO.Path.GetExtension(DefaultExpectedValue));
				}
		}

		public class JsonStringRegexPropertyValidator : JsonPropertyEqualityValidator<string>
		{
			private readonly Regex _regex;
			private readonly bool _shouldMatch;

			public JsonStringRegexPropertyValidator(string key, bool isRequired, string expectedValue, bool hasExpectedValue, string expectedStateKey, bool shouldMatch, bool ignoreCase = false)
				: base(key, isRequired, expectedValue, hasExpectedValue, expectedStateKey)
			{
				RegexOptions options = RegexOptions.Compiled;

				if (ignoreCase)
				{
					options = options | RegexOptions.IgnoreCase;
				}
				_regex = new Regex(expectedValue, options | RegexOptions.Compiled);
				_shouldMatch = shouldMatch;
			}

			public override string FormatValue(string value)
			{
				return value;
			}

			public override bool Validate(JToken actualValue, Dictionary<string, string> savedState, out string errorMessage)
			{
				errorMessage = "";

				if (actualValue == null && !IsRequired)
					return true;

				errorMessage = "";
				string typedActualValue = actualValue.Value<string>();

				if (string.IsNullOrEmpty(typedActualValue))
				{
					errorMessage = $"Value null doesn't match the expected regular expression '{_regex}'";
					return false;
				}

				string formattedActualValue = FormatValue(typedActualValue);

				bool isMatch = _regex.IsMatch(typedActualValue);

				if (_shouldMatch)
				{
					if (isMatch)
					{
						return true;
					}
					errorMessage = string.Format("Value '{0}' doesn't match the expected regular expression '{1}'", formattedActualValue, _regex);
					return false;
				}
				else // _isMatchShouldBe == false
				{
					if (!isMatch)
					{
						return true;
					}

					errorMessage = string.Format("Value '{0}' matched the regular expression, but should not '{1}'", formattedActualValue, _regex);
					return false;
				}
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

		public class JsonResponseBodyPropertyValidator : JsonPropertyEqualityValidator<string>
		{
			public JsonResponseBodyPropertyValidator(string key, bool isRequired, string expectedValue, bool hasExpectedValue, string expectedStateKey)
				: base(key, isRequired, expectedValue, hasExpectedValue, expectedStateKey)
			{
			}

			public override bool Validate(JToken actualValue, Dictionary<string, string> savedState, out string errorMessage)
			{
				errorMessage = "";

				if (actualValue == null)
				{
					if (IsRequired)
					{
						errorMessage = "Value is required but not provided.";
						return false;
					}
					return true;
				}

				JToken expectedValue = JToken.Parse(this.DefaultExpectedValue);
				HashSet<string> propertyValuesToOmit = new HashSet<string>();

				JsonOmitProcessor(expectedValue, propertyValuesToOmit, false);
				JsonOmitProcessor(actualValue, propertyValuesToOmit, true);

				JToken sortedExpectedValue = JsonSort(expectedValue);
				JToken sortedActualValue = JsonSort(actualValue);

				if (JToken.DeepEquals(sortedExpectedValue, sortedActualValue))
				{
					return true;
				}

				errorMessage = string.Format("\nExpected: '{0}', \nActual: '{1}'", sortedExpectedValue.ToString(), sortedActualValue.ToString());
				return false;
			}

			public override string FormatValue(string value)
			{
				return value;
			}

			private void JsonOmitProcessor(JToken node, HashSet<string> propertyValuesToOmit, bool shouldReplace)
			{
				if (node.Type == JTokenType.Object)
				{
					foreach (JProperty property in node.Children<JProperty>())
					{
						if (shouldReplace && propertyValuesToOmit.Contains(property.Name))
						{
							property.Value = "*";
						}
						else if (property.Value.ToString().Equals("*"))
						{
							propertyValuesToOmit.Add(property.Name);
						}
						JsonOmitProcessor(property.Value, propertyValuesToOmit, shouldReplace);
					}
				}
				else if (node.Type == JTokenType.Array)
				{
					foreach (JToken child in node.Children())
					{
						foreach (JProperty property in child.Children<JProperty>())
						{
							if (shouldReplace && propertyValuesToOmit.Contains(property.Name))
							{
								property.Value = "*";
							}
							else if (property.Value.ToString().Equals("*"))
							{
								propertyValuesToOmit.Add(property.Name);
							}
							JsonOmitProcessor(property.Value, propertyValuesToOmit, shouldReplace);
						}
					}
				}
			}

			private JToken JsonSort(JToken node)
			{
				if (!node.HasValues)
				{
					return node;
				}

				return new JArray(node.OrderBy(obj => (string)obj["CoauthLockId"]));
			}
		}
		public class ArrayLengthPropertyValidator : JsonPropertyEqualityValidator<int>
		{
			public ArrayLengthPropertyValidator(string key, bool isRequired, int expectedValue, bool hasExpectedValue, string expectedStateKey)
				: base(key, isRequired, expectedValue, hasExpectedValue, expectedStateKey)
			{
			}

			public override bool Validate(JToken actualValue, Dictionary<string, string> savedState, out string errorMessage)
			{
				errorMessage = "";

				if (actualValue == null)
				{
					if (IsRequired)
					{
						errorMessage = "Value is required but not provided.";
						return false;
					}
					return true;
				}

				if (actualValue.Type != JTokenType.Array)
				{
					if (IsRequired)
					{
						errorMessage = string.Format("Value is of '{0}' type and is not an array type", actualValue.Type);
						return false;
					}
					return true;
				}

				JArray responseObject = (JArray)actualValue;
				int responseLength = responseObject.Count;

				if (this.DefaultExpectedValue == responseLength)
				{
					return true;
				}

				errorMessage = string.Format("Expected array to be of length '{0}', Actual: '{1}'", this.DefaultExpectedValue, responseLength);
				return false;
			}

			public override string FormatValue(int value)
			{
				return value.ToString(CultureInfo.InvariantCulture);
			}
		}
	}
}
