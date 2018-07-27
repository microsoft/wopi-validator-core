// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

namespace Microsoft.Office.WopiValidator.Core
{
	public static class UrlHelper
	{
		/// <summary>
		/// Reads the value of a querystring parameter from a url
		/// </summary>
		/// <param name="baseQuery">the url to operate on</param>
		/// <param name="parameterName">name of the parameter to get</param>
		public static string GetQueryParameterValue(string baseQuery, string parameterName)
		{
			if (baseQuery == null)
				return null;

			if (String.IsNullOrWhiteSpace(parameterName))
				return baseQuery;

			string paramWithQ = "?" + parameterName + "=";
			string paramWithAmp = "&" + parameterName + "=";
			int ichParam = -1;

			// If the parameter is already there, grab its index so we know where to replace.
			if (-1 == (ichParam = baseQuery.IndexOf(paramWithQ, StringComparison.OrdinalIgnoreCase)) &&
				-1 == (ichParam = baseQuery.IndexOf(paramWithAmp, StringComparison.OrdinalIgnoreCase)))
			{
				// the parameter is not on the url
				return null;
			}

			int ichParamValue = ichParam + paramWithQ.Length;
			string value;

			// Look to see if there's another parameter after the current one
			int ichNextParam = baseQuery.IndexOfAny(new char[] { '&', '#' }, ichParamValue);
			if (ichNextParam != -1)
			{
				// There is something after it, so just grab the characters up til the start of the next thing
				value = baseQuery.Substring(ichParamValue, ichNextParam - ichParamValue);
			}
			else
			{
				// There's nothing after this query param, so return the entire rest of the string
				value = baseQuery.Substring(ichParamValue);
			}

			return HttpUtility.UrlDecode(value);
		}

		/// <summary>
		/// This method is used to properly append a query parameter to an existing url query
		/// </summary>
		/// <param name="baseQuery">the query to operate</param>
		/// <param name="parameterName">name of the parameter to add or replace</param>
		/// <param name="parameterValue">value of the parameter to be appended or replaced into the query with proper encoding</param>
		public static string AppendOrReplaceQueryParameter(string baseQuery, string parameterName, string parameterValue)
		{
			if (baseQuery == null)
				return "";

			if (String.IsNullOrWhiteSpace(parameterName))
				return baseQuery;

			string encodedParamName = HttpUtility.UrlEncode(parameterName);
			string encodedParamValue = HttpUtility.UrlEncode(parameterValue) ?? "";

			StringBuilder output = new StringBuilder(baseQuery.Length + encodedParamName.Length + encodedParamValue.Length + 2);

			string paramWithQ = "?" + encodedParamName + "=";
			string paramWithAmp = "&" + encodedParamName + "=";
			int ichParam = -1;

			// If the parameter is already there, grab its index so we know where to replace.
			if (-1 != (ichParam = baseQuery.IndexOf(paramWithQ, StringComparison.OrdinalIgnoreCase)) ||
				-1 != (ichParam = baseQuery.IndexOf(paramWithAmp, StringComparison.OrdinalIgnoreCase)))
			{
				// Copy everything up to the beginning of the parameter
				// and the ? or the &
				output.Append(baseQuery, 0, ichParam + 1);

				// Append the encoded version of the user specified "parameterName"
				output.Append(encodedParamName);

				// Append the "="
				output.Append('=');

				// Append the new value
				output.Append(encodedParamValue);

				// Find the beginning of the first thing after the original parameter value and append everything after it
				int ichNextParam = baseQuery.IndexOfAny(new char[] { '&', '#' }, ichParam + paramWithQ.Length);
				if (ichNextParam != -1)
					output.Append(baseQuery, ichNextParam, baseQuery.Length - ichNextParam);
			}
			else
			{
				// The parameter isn't already there so we'll append it to the url

				// Build the value we'll append to the querystring, handling ? or &
				char outputParamPrefix = (baseQuery.IndexOf('?') > -1) ? '&' : '?';

				// If the URL doesn't have a # then all we have to do is append to the starting url
				int ichHash = baseQuery.IndexOf('#');
				if (ichHash == -1)
				{
					output.Append(baseQuery);
					output.Append(outputParamPrefix);
					output.Append(encodedParamName);
					output.Append('=');
					output.Append(encodedParamValue);
				}
				else
				{
					// There's a hash... append our parameter just before it then append the hash
					output.Append(baseQuery, 0, ichHash);
					output.Append(outputParamPrefix);
					output.Append(encodedParamName);
					output.Append('=');
					output.Append(encodedParamValue);
					output.Append(baseQuery, ichHash, baseQuery.Length - ichHash);
				}
			}

			return output.ToString();
		}

		/// <summary>
		/// Encodes URL querystring key or value for reliable HTTP transmission
		/// from the Web server to a client.
		/// </summary>
		/// <param name="keyOrValueToEncode">The key or value string to encode.</param>
		/// <returns>
		/// The encoded key or value string.  Empty if the keyOrValueToEncode
		/// parameter is empty and null if the keyOrValueToEncode parameter is null.
		/// </returns>
		/// <remarks>
		/// This method canonicalizes all non-alphanumeric characters.
		///
		/// Only use this method when you are constructing the URL yourself.
		/// Do not use this method on a full URL passed to you, such as an
		/// URL entered by user.
		///
		/// </remarks>
		public static string UrlKeyValueEncode(string keyOrValueToEncode)
		{
			const int bufferSize = 255;

			if (string.IsNullOrEmpty(keyOrValueToEncode))
			{
				return keyOrValueToEncode;
			}

			StringBuilder sb = new StringBuilder(bufferSize);
			var sbWriter = new StringWriter(sb, CultureInfo.InvariantCulture);

			//Canonicalizes anything but alphanumeric characters
			bool fUsedNextChar = false;

			int start = 0;
			int length = 0;
			int nkeyOrValueToEncodeLength = keyOrValueToEncode.Length;
			for (int i = 0; i < nkeyOrValueToEncodeLength; i++)
			{
				char ch = keyOrValueToEncode[i];
				if (('0' <= (ch) && (ch) <= '9') ||
					('a' <= (ch) && (ch) <= 'z') ||
					('A' <= (ch) && (ch) <= 'Z'))
				{
					length++;
				}
				else
				{
					if (length > 0)
					{
						sbWriter.Write(keyOrValueToEncode.Substring(start, length));
						length = 0;
					}

					UrlEncodeUnicodeChar(sbWriter,
						(char)(keyOrValueToEncode[i]),
						i < nkeyOrValueToEncodeLength - 1 ? (char)(keyOrValueToEncode[i + 1]) : '\0',
						out fUsedNextChar);
					if (fUsedNextChar)
					{
						i++;
					}
					start = i + 1;
				}
			}

			// This check of output is redundant, but here to satisfy static analyzer.
			if (start < nkeyOrValueToEncodeLength && sbWriter != null)
			{
				sbWriter.Write(keyOrValueToEncode.Substring(start));
			}

			return sb.ToString();
		}

		public static string GetUTF7EncodedUnescapedDataString(string inputData)
		{
			if (string.IsNullOrEmpty(inputData))
			{
				return inputData;
			}

			return Uri.UnescapeDataString(HttpUtility.UrlEncode(inputData, Encoding.UTF7));
		}

		private static void UrlEncodeUnicodeChar(
			TextWriter output,
			char ch,
			char chNext,
			out bool fUsedNextChar)
		{
			bool fInvalidUnicode = false;
			UrlEncodeUnicodeChar(
				output,
				ch,
				chNext,
				ref fInvalidUnicode,
				out fUsedNextChar);
		}

		private static readonly string[] s_crgstrUrlHexValue =
		{
			"%00", "%01", "%02", "%03", "%04", "%05", "%06", "%07", "%08", "%09", "%0A", "%0B", "%0C", "%0D", "%0E", "%0F",
			"%10", "%11", "%12", "%13", "%14", "%15", "%16", "%17", "%18", "%19", "%1A", "%1B", "%1C", "%1D", "%1E", "%1F",
			"%20", "%21", "%22", "%23", "%24", "%25", "%26", "%27", "%28", "%29", "%2A", "%2B", "%2C", "%2D", "%2E", "%2F",
			"%30", "%31", "%32", "%33", "%34", "%35", "%36", "%37", "%38", "%39", "%3A", "%3B", "%3C", "%3D", "%3E", "%3F",
			"%40", "%41", "%42", "%43", "%44", "%45", "%46", "%47", "%48", "%49", "%4A", "%4B", "%4C", "%4D", "%4E", "%4F",
			"%50", "%51", "%52", "%53", "%54", "%55", "%56", "%57", "%58", "%59", "%5A", "%5B", "%5C", "%5D", "%5E", "%5F",
			"%60", "%61", "%62", "%63", "%64", "%65", "%66", "%67", "%68", "%69", "%6A", "%6B", "%6C", "%6D", "%6E", "%6F",
			"%70", "%71", "%72", "%73", "%74", "%75", "%76", "%77", "%78", "%79", "%7A", "%7B", "%7C", "%7D", "%7E", "%7F",
			"%80", "%81", "%82", "%83", "%84", "%85", "%86", "%87", "%88", "%89", "%8A", "%8B", "%8C", "%8D", "%8E", "%8F",
			"%90", "%91", "%92", "%93", "%94", "%95", "%96", "%97", "%98", "%99", "%9A", "%9B", "%9C", "%9D", "%9E", "%9F",
			"%A0", "%A1", "%A2", "%A3", "%A4", "%A5", "%A6", "%A7", "%A8", "%A9", "%AA", "%AB", "%AC", "%AD", "%AE", "%AF",
			"%B0", "%B1", "%B2", "%B3", "%B4", "%B5", "%B6", "%B7", "%B8", "%B9", "%BA", "%BB", "%BC", "%BD", "%BE", "%BF",
			"%C0", "%C1", "%C2", "%C3", "%C4", "%C5", "%C6", "%C7", "%C8", "%C9", "%CA", "%CB", "%CC", "%CD", "%CE", "%CF",
			"%D0", "%D1", "%D2", "%D3", "%D4", "%D5", "%D6", "%D7", "%D8", "%D9", "%DA", "%DB", "%DC", "%DD", "%DE", "%DF",
			"%E0", "%E1", "%E2", "%E3", "%E4", "%E5", "%E6", "%E7", "%E8", "%E9", "%EA", "%EB", "%EC", "%ED", "%EE", "%EF",
			"%F0", "%F1", "%F2", "%F3", "%F4", "%F5", "%F6", "%F7", "%F8", "%F9", "%FA", "%FB", "%FC", "%FD", "%FE", "%FF"
		};

		private static void UrlEncodeUnicodeChar(
			TextWriter output,
			char ch,
			char chNext,
			ref bool fInvalidUnicode,
			out bool fUsedNextChar)
		{   // based on the logic from escapeProperly() in common.jss
			// this function performs an aggressive unicode URL-encoding
			// convert non alphanum character into UTF-8 code string
			// in format %XX%XX%XX
			int UTF8_1ST_OF_2 = 0xc0;     // 110x xxxx
			int UTF8_1ST_OF_3 = 0xe0;     // 1110 xxxx
			int UTF8_1ST_OF_4 = 0xf0;     // 1111 0xxx
			int UTF8_TRAIL = 0x80;     // 10xx xxxx
			int HIGH_SURROGATE_BITS = 0xD800;
			int SURROGATE_6_BIT = 0xFC00;
			int SURROGATE_OFFSET = 0x10000;

			int iByte;

			fUsedNextChar = false;

			int charCode = (int)ch;
			if (charCode <= 0x7f)
			{
				output.Write(s_crgstrUrlHexValue[charCode]);
			}
			else if (charCode <= 0x07ff)
			{
				iByte = UTF8_1ST_OF_2 | (charCode >> 6);
				output.Write(s_crgstrUrlHexValue[iByte]);
				iByte = UTF8_TRAIL | (charCode & 0x003f);
				output.Write(s_crgstrUrlHexValue[iByte]);
			}
			else if ((charCode & SURROGATE_6_BIT) != HIGH_SURROGATE_BITS)
			{
				iByte = UTF8_1ST_OF_3 | (charCode >> 12);
				output.Write(s_crgstrUrlHexValue[iByte]);
				// middle 6 bits
				iByte = UTF8_TRAIL | ((charCode & 0x0fc0) >> 6);
				output.Write(s_crgstrUrlHexValue[iByte]);
				// lower 6 bits
				iByte = UTF8_TRAIL | (charCode & 0x003f);
				output.Write(s_crgstrUrlHexValue[iByte]);
			}
			else if (chNext != '\0')
			{
				// lower 10 bits of first char
				charCode = (charCode & 0x03FF) << 10;
				fUsedNextChar = true;

				// lower 10 bits of second char
				charCode |= ((int)chNext) & 0x03FF;
				charCode += SURROGATE_OFFSET;

				iByte = UTF8_1ST_OF_4 | (charCode >> 18);
				output.Write(s_crgstrUrlHexValue[iByte]);
				// upper 6 bits
				iByte = UTF8_TRAIL | ((charCode & 0x3f000) >> 12);
				output.Write(s_crgstrUrlHexValue[iByte]);
				// middle 6 bits
				iByte = UTF8_TRAIL | ((charCode & 0x0fc0) >> 6);
				output.Write(s_crgstrUrlHexValue[iByte]);
				// lower 6 bits
				iByte = UTF8_TRAIL | (charCode & 0x003f);
				output.Write(s_crgstrUrlHexValue[iByte]);
			}
			else
			{
				fInvalidUnicode = true;
			}
		}
	}
}
