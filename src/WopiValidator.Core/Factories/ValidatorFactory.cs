// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.Validators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	static class ValidatorFactory
	{
		public static IEnumerable<IValidator> GetValidators(XElement definition, string fileNameGuid)
		{
			var validators = new List<IValidator> { new ExceptionValidator() };
			validators.AddRange(definition.Elements().Select(x => GetValidator(x, fileNameGuid)));
			return validators;
		}

		/// <summary>
		/// Parses single Validator information and instantiates proper IValidator instance based on element's name.
		/// </summary>
		private static IValidator GetValidator(XElement definition, string fileNameGuid)
		{
			string elementName = definition.Name.LocalName;

			switch (elementName)
			{
				case Constants.Validators.And:
					return GetAndValidator(definition);
				case Constants.Validators.LockMismatch:
					return GetLockMismatchValidator(definition);
				case Constants.Validators.ResponseCode:
					return GetResponseCodeValidator(definition);
				case Constants.Validators.ResponseHeader:
					return GetResponseHeaderValidator(definition);
				case Constants.Validators.JsonResponseContent:
					return GetJsonResponseContentValidator(definition, fileNameGuid);
				case Constants.Validators.JsonSchema:
					return GetJsonSchemaValidator(definition);
				case Constants.Validators.Or:
					return GetOrValidator(definition);
				case Constants.Validators.ResponseContent:
					return GetResponseContentValidator(definition);
				case Constants.Validators.FramesValidator:
					return GetFramesValidator(definition);
				default:
					throw new ArgumentException(string.Format("Unknown validator: '{0}'", elementName));
			}
		}

		/// <summary>
		/// Parses And validator information.
		/// </summary>
		private static IValidator GetAndValidator(XElement definition)
		{
			IEnumerable<IValidator> validators = definition.Elements().Select(x => GetValidator(x, null));
			return new AndValidator(validators.ToArray());
		}

		/// <summary>
		/// Parses ResponseContent validator information.
		/// </summary>
		private static IValidator GetResponseContentValidator(XElement definition)
		{
			string resourceId = (string)definition.Attribute("ExpectedResourceId");
			string expectedBodyContent = (string)definition.Attribute("ExpectedBodyContent");
			return new ResponseContentValidator(resourceId, expectedBodyContent);
		}

		/// <summary>
		/// Parses ResponseContent validator information.
		/// This is used for incremental file transfer scenario, where responseContent is frame list wrapped up in stream.
		/// FramesValidator checks the frame list schema and pre-process responseContent,
		/// then it passes the processed content to FramePayloadValidator to check payload content for a specific streamId
		/// </summary>
		private static IValidator GetFramesValidator(XElement definition)
		{
			IEnumerable<FramesValidator.ContentStreamValidator> contentStreamValidators = definition.Elements()
				.Where((childDefinition) => string.Equals(childDefinition.Name.LocalName, Constants.Validators.ContentStreamValidator, StringComparison.OrdinalIgnoreCase))
				.Select((childDefinition) => new FramesValidator.ContentStreamValidator(
					(string)childDefinition.Attribute("StreamId"),
					(string)childDefinition.Attribute("ExpectedChunkingScheme"),
					(string)childDefinition.Attribute("ExpectedContent"),
					(string)childDefinition.Attribute("ExpectedContentResourceId"),
					(string)childDefinition.Attribute("AlreadyExistingContent"),
					(string)childDefinition.Attribute("AlreadyExistingContentResourceId")
				));

			IEnumerable<FramesValidator.ContentPropertyValidator> contentPropertyValidators = definition.Elements()
				.Where((childDefinition) => string.Equals(childDefinition.Name.LocalName, Constants.Validators.ContentPropertyValidator, StringComparison.OrdinalIgnoreCase))
				.Select((childDefinition) => new FramesValidator.ContentPropertyValidator(
					(string)childDefinition.Attribute("Name"),
					(string)childDefinition.Attribute("ExpectedValue") ?? string.Empty,
					(string)childDefinition.Attribute("ExpectedRetention") ?? string.Empty,
					(bool?)childDefinition.Attribute("ShouldBeReturned") ?? true
				));

			return new FramesValidator(
				(string)definition.Attribute("MessageJsonPayloadSchema"),
				(int?)definition.Attribute("ExpectedHostBlobsCount"),
				contentStreamValidators,
				contentPropertyValidators);
		}

		/// <summary>
		/// Parses ResponseHeader validator information.
		/// </summary>
		private static IValidator GetResponseHeaderValidator(XElement definition)
		{
			string header = (string)definition.Attribute("Header");
			string comparator = (string)definition.Attribute("Comparator");
			string expectedStateKey = (string)definition.Attribute("ExpectedStateKey");
			string expectedValue = (string)definition.Attribute("ExpectedValue");
			bool isRequired = ((bool?)definition.Attribute("IsRequired")) ?? true;
			bool shouldMatch = ((bool?)definition.Attribute("ShouldMatch")) ?? true;

			return new ResponseHeaderValidator(header, expectedValue, expectedStateKey, comparator, isRequired, shouldMatch);
		}

		/// <summary>
		/// Parses ResponseCode validator information.
		/// </summary>
		private static IValidator GetResponseCodeValidator(XElement definition)
		{
			int expectedValue = (int)definition.Attribute("ExpectedCode");
			return new ResponseCodeValidator(expectedValue);
		}

		/// <summary>
		/// Parses Or validator information.
		/// </summary>
		private static IValidator GetOrValidator(XElement definition)
		{
			IEnumerable<IValidator> validators = definition.Elements().Select(x => GetValidator(x, null));
			return new OrValidator(validators.ToArray());
		}

		/// <summary>
		/// Parses LockMismatch validator information.
		/// </summary>
		private static IValidator GetLockMismatchValidator(XElement definition)
		{
			string expectedLockString = (string)definition.Attribute("ExpectedLock");
			return new LockMismatchValidator(expectedLockString);
		}

		/// <summary>
		/// Parses JsonResponseContent validator information.
		/// </summary>
		private static IValidator GetJsonResponseContentValidator(XElement definition, string fileNameGuid)
		{
			bool shouldExist = ((bool?)definition.Attribute("ShouldExist")) ?? true;
			IEnumerable<JsonContentValidator.IJsonPropertyValidator> propertyValidators = definition.Elements().Select(x => GetJsonPropertyValidator(x, fileNameGuid));
			return new JsonContentValidator(propertyValidators, shouldExist);
		}

		/// <summary>
		/// Parses JsonSchema validator information.
		/// </summary>
		private static IValidator GetJsonSchemaValidator(XElement definition)
		{
			string schemaId = (string)definition.Attribute("Schema");
			return new JsonSchemaValidator(schemaId);
		}

		/// <summary>
		/// Parses Json property information for JsonResponseContent validator information.
		/// </summary>
		private static JsonContentValidator.IJsonPropertyValidator GetJsonPropertyValidator(XElement definition, string fileNameGuid)
		{
			string elementName = definition.Name.LocalName;
			string key = (string)definition.Attribute("Name");
			string expectedValue = (string)definition.Attribute("ExpectedValue");
			bool hasExpectedValue = expectedValue != null;
			bool isRequired = ((bool?)definition.Attribute("IsRequired")) ?? false;
			string endsWithValue = (string)definition.Attribute("EndsWith");
			string expectedStateKey = (string)definition.Attribute("ExpectedStateKey");
			string containsValue = (string)definition.Attribute("ContainsValue");
			bool shouldMatch = ((bool?)definition.Attribute("ShouldMatch")) ?? true;
			bool hasContainsValue = containsValue != null;
			bool mustIncludeAccessToken = ((bool?)definition.Attribute("MustIncludeAccessToken")) ?? false;
			bool ignoreCase = ((bool?)definition.Attribute("IgnoreCase")) ?? false;

			switch (elementName)
			{
				case Constants.Validators.Properties.BooleanProperty:
					return new JsonContentValidator.JsonBooleanPropertyValidator(key,
						isRequired,
						hasExpectedValue ? (bool)definition.Attribute("ExpectedValue") : false,
						hasExpectedValue,
						expectedStateKey);

				case Constants.Validators.Properties.IntegerProperty:
					return new JsonContentValidator.JsonIntegerPropertyValidator(key,
						isRequired,
						hasExpectedValue ? (int)definition.Attribute("ExpectedValue") : 0,
						hasExpectedValue,
						expectedStateKey);

				case Constants.Validators.Properties.LongProperty:
					return new JsonContentValidator.JsonLongPropertyValidator(key,
						isRequired,
						hasExpectedValue ? (long)definition.Attribute("ExpectedValue") : 0,
						hasExpectedValue,
						expectedStateKey);

				case Constants.Validators.Properties.FileNameProperty:
					return new JsonContentValidator.JsonFileNamePropertyValidator(key,
						expectedValue,
						fileNameGuid,
						ignoreCase);

				case Constants.Validators.Properties.StringProperty:
					return new JsonContentValidator.JsonStringPropertyValidator(key,
						isRequired,
						expectedValue,
						hasExpectedValue,
						endsWithValue,
						expectedStateKey,
						ignoreCase);

				case Constants.Validators.Properties.StringRegexProperty:
					return new JsonContentValidator.JsonStringRegexPropertyValidator(key,
						isRequired,
						expectedValue,
						hasExpectedValue,
						expectedStateKey,
						shouldMatch,
						ignoreCase);

				case Constants.Validators.Properties.AbsoluteUrlProperty:
					return new JsonContentValidator.JsonAbsoluteUrlPropertyValidator(key,
						isRequired,
						mustIncludeAccessToken,
						expectedStateKey);

				case Constants.Validators.Properties.ArrayProperty:
					return new JsonContentValidator.JsonArrayPropertyValidator(key,
						isRequired,
						containsValue,
						hasContainsValue,
						expectedStateKey);

				case Constants.Validators.Properties.ResponseBodyProperty:
					return new JsonContentValidator.JsonResponseBodyPropertyValidator(key,
						isRequired,
						expectedValue,
						hasExpectedValue,
						expectedStateKey);

				case Constants.Validators.Properties.ArrayLengthProperty:
					return new JsonContentValidator.ArrayLengthPropertyValidator(key,
						isRequired,
						hasExpectedValue ? (int)definition.Attribute("ExpectedValue") : 0,
						hasExpectedValue,
						expectedStateKey);

				default:
					throw new ArgumentException(string.Format("Unknown property type: '{0}'", elementName));
			}
		}
	}
}
