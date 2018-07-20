// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.Mutators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Office.WopiValidator.Core.Requests
{
	/// <summary>
	/// Base class for all WOPI requests.
	/// </summary>
	abstract class WopiRequest : RequestBase
	{
		/// <summary>
		/// OverrideUrl property from the XML
		/// </summary>
		protected override string OverrideUrl { get; set; }

		public IEnumerable<IMutator> Mutators { get; private set; }

		/// <summary>
		/// X-WOPI-Override header value
		/// </summary>
		protected virtual string WopiOverrideValue
		{
			get { return null; }
		}

		/// <summary>
		/// Flag indicating if request has content.
		///
		/// GetRequestContent method has to be implemented when it returns true.
		/// </summary>
		protected virtual bool HasRequestContent
		{
			get { return false; }
		}

		/// <summary>
		/// Collection of request-specific headers that should be included into the request.
		/// </summary>
		protected virtual IEnumerable<KeyValuePair<string, string>> DefaultHeaders
		{
			get { return Enumerable.Empty<KeyValuePair<string, string>>(); }
		}

		protected WopiRequest(WopiRequestParam param)
		{
			OverrideUrl = param.OverrideUrl;
			Validators = (param.Validators ?? Enumerable.Empty<IValidator>()).ToArray();
			State = (param.StateSavers ?? Enumerable.Empty<IStateEntry>()).ToArray();
			Mutators = (param.Mutators ?? Enumerable.Empty<IMutator>()).ToArray();
		}

		/// <summary>
		/// Executes WOPI request at given WOPI endpoint address against provided wopi FileRep.
		/// </summary>
		public override IResponseData Execute(string endpointAddress,
			string accessToken,
			long accessTokenTtl,
			ITestCase testCase,
			Dictionary<string, string> savedState,
			IResourceManager resourceManager,
			string userAgent,
			RSACryptoServiceProvider proofKeyProviderNew,
			RSACryptoServiceProvider proofKeyProviderOld)
		{
			// Get the url of the WOPI endpoint that we'll call - either the normal endpoint, or a SavedState override.
			// If it's an override it might change the accessToken that we're using because it probably already has a token on it.
			Uri uri = GetRequestUri(endpointAddress, ref accessToken, accessTokenTtl, savedState);

			// Run any access token mutators defined on this request.
			string accessTokenToUse = GetMutatedAccessToken(accessToken);
			if (accessToken != accessTokenToUse)
			{
				// The access token changed so update our uri with the new one
				uri = new Uri(UrlHelper.AppendOrReplaceQueryParameter(uri.AbsoluteUri, "access_token", accessTokenToUse));
			}
			// At this point we have the final uri and accessTokenToUse values.  We'll use them later in proof key signing

			List<KeyValuePair<string, string>> headers = DefaultHeaders.ToList();
			IEnumerable<KeyValuePair<string, string>> customHeaders = GetCustomHeaders(savedState, resourceManager);
			if (customHeaders != null)
				headers.AddRange(customHeaders);

			if (!string.IsNullOrEmpty(WopiOverrideValue))
				headers.Add(new KeyValuePair<string, string>(Constants.Headers.Override, WopiOverrideValue));

			if (proofKeyProviderNew != null && proofKeyProviderOld != null)
			{
				Dictionary<string, string> originalProofKeyHeaders =
					GetProofKeyHeaders(accessTokenToUse, uri, proofKeyProviderNew, proofKeyProviderOld);
				Dictionary<string, string> proofKeyHeadersToUse =
					GetMutatedProofKeyHeaders(originalProofKeyHeaders, timestamp => GetProofKeyHeaders(accessTokenToUse, uri, proofKeyProviderNew, proofKeyProviderOld, timestamp));
				headers.AddRange(proofKeyHeadersToUse);
			}

			headers.Add(new KeyValuePair<string, string>(Constants.Headers.Authorization, "Bearer " + accessTokenToUse));

			MemoryStream contentStream = HasRequestContent ? GetRequestContent(resourceManager) : null;
			RequestExecutionData executionData = new RequestExecutionData(uri, headers, contentStream);

			return ExecuteRequest(executionData, userAgent: userAgent);
		}

		protected Uri GetRequestUri(string endpointAddress, ref string accessToken, long accessTokenTtl, Dictionary<string, string> savedState)
		{
			Uri uri;

			string endpointAddressOverride = GetEndpointAddressOverride(savedState);
			if (String.IsNullOrEmpty(endpointAddressOverride))
			{
				// If override url is not present or is set to null while on a WOPI request, build the WOPI url from the base endpoint + base access token,
				// except for Delete requests to prevent the original ".wopitest" file from being deleted.
				uri = (this is DeleteFileRequest || this is DeleteContainerRequest) ? null : BuildWopiUri(endpointAddress, accessToken, accessTokenTtl);
			}
			else
			{
				// The state dictionary has an OverrideUrl for us to use.
				// We'll just use the override as-is but we also have to parse it to get the override's access token
				// so that we have the correct access token around when we do proof key signing
				uri = new Uri(endpointAddressOverride);
				accessToken = UrlHelper.GetQueryParameterValue(uri.AbsoluteUri, "access_token");
			}

			return uri;
		}

		protected virtual IEnumerable<KeyValuePair<string, string>> GetCustomHeaders(Dictionary<string, string> savedState, IResourceManager resourceManager)
		{
			return null;
		}

		/// <summary>
		/// Combines WOPI endpoint address and FileRep to get URI that request should be send to.
		/// </summary>
		protected Uri BuildWopiUri(string url, string accessToken, long accessTokenTtl)
		{
			StringBuilder sb = new StringBuilder(url);

			sb.Append(PathOverride);
			// add any required connecting stuff to make it valid to add
			// a query string param key=value pair
			if (!url.Contains("?"))
			{
				// no query string found, add the query string mark
				sb.Append("?");
			}
			else if (!url.EndsWith("&", StringComparison.Ordinal))
			{
				// have a query string already, but need a delimeter at the end
				sb.Append("&");
			}

			sb.Append(String.Format(CultureInfo.InvariantCulture,
				"{0}={1}&{2}={3}",
				"access_token",
				UrlHelper.UrlKeyValueEncode(accessToken),
				"access_token_ttl",
				accessTokenTtl));

			return new Uri(sb.ToString());
		}

		/// <summary>
		/// Method that derived classes can override to provide Content for the request.
		/// </summary>
		protected virtual MemoryStream GetRequestContent(IResourceManager resourceManager)
		{
			throw new NotImplementedException();
		}

		private Dictionary<string, string> GetProofKeyHeaders(string accessToken,
			Uri endpointUri,
			RSACryptoServiceProvider proofKeyProviderNew,
			RSACryptoServiceProvider proofKeyProviderOld,
			long timestamp)
		{
			Dictionary<string, string> proofKeyHeaders = new Dictionary<string, string>();

			ProofKeyInput input = new ProofKeyInput(accessToken, timestamp, endpointUri.AbsoluteUri);

			// X-WOPI-Proof
			ProofKeyOutput currentProof = ProofKeysHelper.GetSignedProofData(input, proofKeyProviderNew);
			proofKeyHeaders.Add(Constants.Headers.ProofKey, currentProof.SignedBase64ProofKey);

			// X-WOPI-ProofOld
			ProofKeyOutput oldProof = ProofKeysHelper.GetSignedProofData(input, proofKeyProviderOld);
			proofKeyHeaders.Add(Constants.Headers.ProofKeyOld, oldProof.SignedBase64ProofKey);

			// X-WOPI-TimeStamp
			proofKeyHeaders.Add(Constants.Headers.WopiTimestamp, timestamp.ToString(System.Globalization.CultureInfo.InvariantCulture));

			// save the proof key output to help partners investigate proof key implementation bugs
			this.CurrentProofData = currentProof;
			this.OldProofData = oldProof;

			return proofKeyHeaders;
		}

		private Dictionary<string, string> GetProofKeyHeaders(string accessToken,
			Uri endpointUri,
			RSACryptoServiceProvider proofKeyProviderNew,
			RSACryptoServiceProvider proofKeyProviderOld)
		{
			return GetProofKeyHeaders(accessToken, endpointUri, proofKeyProviderNew, proofKeyProviderOld, DateTime.UtcNow.Ticks);
		}

		private string GetMutatedAccessToken(string original)
		{
			AccessTokenMutator mutator = Mutators.OfType<AccessTokenMutator>().FirstOrDefault();
			return mutator == null ? original : mutator.Mutate(original);
		}

		private Dictionary<string, string> GetMutatedProofKeyHeaders(
			Dictionary<string, string> original,
			Func<long, Dictionary<string, string>> proofKeyGeneration)
		{
			ProofKeyMutator mutator = Mutators.OfType<ProofKeyMutator>().FirstOrDefault();
			return mutator == null ? original : mutator.Mutate(original, proofKeyGeneration);
		}
	}
}
