// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using NJsonSchema;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Office.WopiValidator.Core.ResourceManagement
{
	internal class JsonSchemas
	{
		private const string SchemasPath = "Microsoft.Office.WopiValidator.Core.JsonSchemas.";
		private const string SchemaSuffix = ".json";

		public static IDictionary<string, JsonSchema4> Schemas { get; }

		static JsonSchemas()
		{
			Schemas = LoadAllSchemas();
		}

		private static IDictionary<string, JsonSchema4> LoadAllSchemas()
		{
			var schemaIds = Assembly.GetExecutingAssembly().GetManifestResourceNames()
				.Where(name => name.StartsWith(SchemasPath))
				.Select(name => ParseSchemaIdFromEmbeddedResourceName(name));
			return schemaIds.ToDictionary(x => x, x => LoadJsonSchema(x));
		}

		private static string ParseSchemaIdFromEmbeddedResourceName(string resourceName)
		{
			if (!resourceName.StartsWith(SchemasPath))
			{
				return null;
			}

			int startIndex = SchemasPath.Length;
			int length = resourceName.Length - SchemasPath.Length - SchemaSuffix.Length;
			return resourceName.Substring(startIndex, length);
		}

		private static JsonSchema4 LoadJsonSchema(string schemaId)
		{
			string json = ReadFileFromAssembly(schemaId);
			return JsonSchema4.FromJson(json);
		}

		private static string ReadFileFromAssembly(string schemaId)
		{
			string json = null;
			string resourcePath = SchemasPath + schemaId + SchemaSuffix;

			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
			if (stream != null)
			{
				using (StreamReader streamReader = new StreamReader(stream))
				{
					json = streamReader.ReadToEnd();
				}
			}
			return json;
		}
	}
}
