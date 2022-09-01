// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core
{
	public class ProofKeyInput
	{
		public string AccessToken { get; private set; }
		public long Timestamp { get; private set; }
		public string Url { get; private set; }

		public ProofKeyInput(string accessToken, long timestamp, string url)
		{
			AccessToken = accessToken;
			Timestamp = timestamp;
			Url = url;
		}
	}

	public class ProofKeyOutput
	{
		public string AccessToken { get; private set; }
		public byte[] AccessTokenBytes;
		public int AccessTokenLength { get; private set; }
		public byte[] AccessTokenLengthBytes;

		public string Url { get; private set; }
		public byte[] UrlBytes;
		public int UrlLength { get; private set; }
		public byte[] UrlLengthBytes;

		public long TimeStamp { get; private set; }
		public byte[] TimeStampBytes;
		public int TimeStampLength { get; private set; }
		public byte[] TimeStampLengthBytes;

		public int PreSigningLength { get; private set; }
		public byte[] PreSigningBytes;

		public string SignedBase64ProofKey { get; set; }

		public ProofKeyOutput(
			string accessToken,
			byte[] accessTokenBytes,
			int accessTokenLength,
			byte[] accessTokenLengthBytes,
			string url,
			byte[] urlBytes,
			int urlLength,
			byte[] urlLengthBytes,
			long timestamp,
			byte[] timestampBytes,
			int timestampLength,
			byte[] timestampLengthBytes,
			byte[] preSigningBytes)
		{
			AccessToken = accessToken;
			AccessTokenBytes = accessTokenBytes;
			AccessTokenLength = accessTokenLength;
			AccessTokenLengthBytes = accessTokenLengthBytes;
			Url = url;
			UrlBytes = urlBytes;
			UrlLength = urlLength;
			UrlLengthBytes = urlLengthBytes;
			TimeStamp = timestamp;
			TimeStampBytes = timestampBytes;
			TimeStampLength = timestampLength;
			TimeStampLengthBytes = timestampLengthBytes;
			PreSigningLength = preSigningBytes.Length;
			PreSigningBytes = preSigningBytes;
		}
	}

	public class ProofKeysHelper
	{
		private static ProofKeyOutput GetProofData(ProofKeyInput proofData)
		{
			if (proofData.AccessToken == null) {
				throw new ProofKeySigningException(nameof(proofData.AccessToken));
			}

			// Get the final values we'll operate on
			string accessToken = proofData.AccessToken;
			string hostUrl = GetUriStringWithEscapedAccessToken(proofData.Url).ToUpperInvariant();
			long timeStamp = proofData.Timestamp;

			// Encode values from headers into byte[]
			byte[] accessTokenBytes = Encoding.UTF8.GetBytes(accessToken);
			byte[] hostUrlBytes = Encoding.UTF8.GetBytes(hostUrl);
			byte[] timeStampBytes = EncodeNumber(timeStamp);

			int accessTokenLength = accessTokenBytes.Length;
			int hostUrlLength = hostUrlBytes.Length;
			int timeStampLength = timeStampBytes.Length;

			byte[] accessTokenLengthBytes = EncodeNumber(accessTokenLength);
			byte[] hostUrlLengthBytes = EncodeNumber(hostUrlLength);
			byte[] timeStampLengthBytes = EncodeNumber(timeStampLength);

			// prepare a list that will be used to combine all those arrays together
			List<byte> expectedProof = new List<byte>(
				accessTokenLengthBytes.Length + accessTokenLength +
				hostUrlLengthBytes.Length + hostUrlLength +
				timeStampLengthBytes.Length + timeStampLength);

			expectedProof.AddRange(accessTokenLengthBytes);
			expectedProof.AddRange(accessTokenBytes);
			expectedProof.AddRange(hostUrlLengthBytes);
			expectedProof.AddRange(hostUrlBytes);
			expectedProof.AddRange(timeStampLengthBytes);
			expectedProof.AddRange(timeStampBytes);

			// create another byte[] from that list
			byte[] preSigningBytes = expectedProof.ToArray();

			return new ProofKeyOutput(
				accessToken,
				accessTokenBytes,
				accessTokenLength,
				accessTokenLengthBytes,
				hostUrl,
				hostUrlBytes,
				hostUrlLength,
				hostUrlLengthBytes,
				timeStamp,
				timeStampBytes,
				timeStampLength,
				timeStampLengthBytes,
				preSigningBytes);
		}

		public static ProofKeyOutput GetSignedProofData(ProofKeyInput proofData, RSACryptoServiceProvider rsaAlg)
		{
			ProofKeyOutput output = GetProofData(proofData);

			using (SHA256 hashAlg = SHA256.Create())
			{
				byte[] signedProofBytes = rsaAlg.SignData(output.PreSigningBytes, hashAlg);
				output.SignedBase64ProofKey = Convert.ToBase64String(signedProofBytes);

				return output;
			}
		}

		private static byte[] EncodeNumber(int value)
		{
			return BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(value));
		}

		private static byte[] EncodeNumber(long value)
		{
			return BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(value));
		}

		private static string GetUriStringWithEscapedAccessToken(string requestUriString)
		{
			//For the purposes of calculating proof keys, the access_token in the URL is aggressively escaped.
			//All non-alphanumeric characters (that is, those outside the range [a-zA-Z0-9]) are URL-encoded
			//(see https://github.com/microsoft/Office-Online-Test-Tools-and-Documentation/issues/129).
			//Hence, take the request's left part from the current URL and escape all non-alphanumeric characters in the access_token query parameter
			//except %: in this case we assume that a character is already escaped.

			var requestUri = new Uri(requestUriString);
			var query = requestUri.Query;
			if (string.IsNullOrEmpty(query) || !query.Contains("access_token"))
			{
				return requestUriString;
			}

			var keyValuePairs = query.Substring(1).Split('&').Select(keyValuePair =>
			{
				if (!keyValuePair.Contains("access_token"))
				{
					return keyValuePair;
				}

				var pair = keyValuePair.Split('=');
				var (key, value) = (pair[0], pair[1]);

				var escapedAccessToken = UrlHelper.UrlKeyValueEncode(value);

				return $"{key}={escapedAccessToken}";
			});

			var originalUrlString = requestUri.GetLeftPart(UriPartial.Path);
			originalUrlString += $"?{string.Join("&", keyValuePairs)}";

			return originalUrlString;
		}
	}

	public class ProofKeySigningException : ArgumentNullException
	{
		public ProofKeySigningException()
			: base()
		{
		}

		public ProofKeySigningException(string paramName)
			: base(paramName)
		{
		}
	}
}
