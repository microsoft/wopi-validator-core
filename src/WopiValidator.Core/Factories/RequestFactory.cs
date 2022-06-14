// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.Office.WopiValidator.Core.Requests;
using Microsoft.Office.WopiValidator.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Office.WopiValidator.Core.Factories
{
	class RequestFactory
	{
		/// <summary>
		/// Parses requests information from XML into a collection of IWopiRequest
		/// </summary>
		public static IEnumerable<IRequest> GetRequests(XElement definition, string fileNameGuid)
		{
			return definition.Elements().Select(x => GetRequest(x, fileNameGuid));
		}

		/// <summary>
		/// Parses single request definition and instantiates proper IWopiRequest instance based on element name
		/// </summary>
		private static IRequest GetRequest(XElement definition, string fileNameGuid)
		{
			string elementName = definition.Name.LocalName;
			XElement validatorsDefinition = definition.Element("Validators");
			XElement stateDefinition = definition.Element("SaveState");
			XElement mutatorsDefinition = definition.Element("Mutators");
			XElement requestBodyDefinition = definition.Element("RequestBody");

			IEnumerable<IValidator> validators = validatorsDefinition == null ? null : ValidatorFactory.GetValidators(validatorsDefinition, fileNameGuid);
			IEnumerable<IStateEntry> stateSavers = stateDefinition == null ? null : StateFactory.GetStateExpressions(stateDefinition);
			IEnumerable<IMutator> mutators = mutatorsDefinition == null ? null : MutatorFactory.GetMutators(mutatorsDefinition);

			// GetChunkedFile XML schema parse
			XElement contentPropertiesToReturnDefinition = definition.Element("ContentPropertiesToReturn");
			XElement contentFiltersDefinition = definition.Element("ContentFilters");
			IEnumerable<XMLContentPropertyToReturn> contentPropertiesToReturn = (contentPropertiesToReturnDefinition == null ? null : ContentPropertyToReturnFactory.GetContentPropertiesToReturn(contentPropertiesToReturnDefinition));
			IEnumerable<XMLContentFilter> contentFilters = (contentFiltersDefinition == null ? null : ContentFilterFactory.GetContentFilters(contentFiltersDefinition));

			// PutChunkedFile XML schema parse
			XElement contentPropertiesDefinition = definition.Element("ContentProperties");
			XElement contentStreamsDefinition = definition.Element("ContentStreams");
			XElement uploadSessionTokenToCommitDefinition = definition.Element("UploadSessionTokenToCommit");
			IEnumerable<XMLContentProperty> contentProperties = (contentPropertiesDefinition == null ? null : ContentPropertyFactory.GetContentProperties(contentPropertiesDefinition));
			IEnumerable<XMLContentStream> contentStreams = (contentStreamsDefinition == null ? null : ContentStreamFactory.GetContentStreams(contentStreamsDefinition));
			string uploadSessionTokenToCommit = (uploadSessionTokenToCommitDefinition == null ? null : (string)uploadSessionTokenToCommitDefinition.Attribute("Value"));


			var wopiRequestParams = new WopiRequestParam
			{
				FileExtensionFilterList = (string)definition.Attribute("FileExtensionFilterList"),
				FolderName = (string)definition.Attribute("FolderName"),
				LockString = (string)definition.Attribute("Lock"),
				LockUserVisible = (bool?)definition.Attribute("LockUserVisible"),
				Mutators = mutators,
				NewLockString = (string)definition.Attribute("NewLock"),
				OldLockString = (string)definition.Attribute("OldLock"),
				OverrideUrl = (string)definition.Attribute("OverrideUrl"),
				OverwriteRelative = (bool?)definition.Attribute("OverwriteRelative"),
				RequestedName = (string)definition.Attribute("Name"),
				ResourceId = (string)definition.Attribute("ResourceId"),
				StateSavers = stateSavers,
				UrlType = (string)definition.Attribute("UrlType"),
				Validators = validators ?? GetDefaultValidators(),
				WopiSrc = (string)definition.Attribute("WopiSrc"),
				// Used for request header
				SequenceNumber = (string)definition.Attribute("SequenceNumber"),
				SequenceNumberStateKey = (string)definition.Attribute("SequenceNumberStateKey"),
				CoauthLockMetadata = (string)definition.Attribute("CoauthLockMetadata"),
				Lock = (string)definition.Attribute("Lock"),
				CoauthLockId = (string)definition.Attribute("CoauthLockId"),
				Editors = (string)definition.Attribute("Editors"),
				CoauthTableVersion = (string)definition.Attribute("CoauthTableVersion"),
				CoauthTableVersionStateKey = (string)definition.Attribute("CoauthTableVersionStateKey"),
				// Used for GetChunkedFile request body
				ContentPropertiesToReturn = contentPropertiesToReturn,
				ContentFilters = contentFilters,
				// Used for PutChunkedFile request body
				ContentProperties = contentProperties,
				ContentStreams = contentStreams,
				UploadSessionTokenToCommit = uploadSessionTokenToCommit
			};

			if (requestBodyDefinition != null && !String.IsNullOrEmpty(requestBodyDefinition.Value))
			{
				wopiRequestParams.RequestBody = requestBodyDefinition.Value;
			}

			string fileMode = (string)definition.Attribute("PutRelativeFileMode");
			if (!String.IsNullOrEmpty(fileMode))
			{
				PutRelativeFileMode parsedMode;
				if (Enum.TryParse(fileMode, true, out parsedMode))
					wopiRequestParams.PutRelativeFileMode = parsedMode;
				else
				{
					throw new ArgumentException(String.Format("PutRelativeFileMode expected to be one of '{0}' or '{1}' or '{2}' but is actually '{3}'",
						PutRelativeFileMode.Suggested,
						PutRelativeFileMode.ExactName,
						PutRelativeFileMode.Conflicting,
						parsedMode));
				}
			}

			string delayTimeInSeconds = (string)definition.Attribute("DelayTimeInSeconds");

			if (!string.IsNullOrEmpty(delayTimeInSeconds))
			{
				if (uint.TryParse(delayTimeInSeconds, out uint parsedDelayTimeInSeconds))
				{
					wopiRequestParams.DelayTimeInSeconds = parsedDelayTimeInSeconds;
				}
				else
				{
					throw new ArgumentException(String.Format("DelayTimeInSeconds expected to be uint'{0}'",
						parsedDelayTimeInSeconds));
				}
			}

			string coauthLockExpirationTimeout = (string)definition.Attribute("CoauthLockExpirationTimeout");

			if (!string.IsNullOrEmpty(coauthLockExpirationTimeout))
			{
				if (uint.TryParse(coauthLockExpirationTimeout, out uint parsedCoauthLockExpirationTimeout))
				{
					wopiRequestParams.CoauthLockExpirationTimeout = parsedCoauthLockExpirationTimeout;
				}
				else
				{
					throw new ArgumentException(String.Format("CoauthLockExpirationTimeout expected to be uint'{0}'",
						parsedCoauthLockExpirationTimeout));
				}
			}

			string coauthLockType = (string)definition.Attribute("CoauthLockType");
			if (!string.IsNullOrEmpty(coauthLockType))
			{
				if (Enum.TryParse(coauthLockType, true, out CoauthLockType parsedCoauthLockType))
				{
					wopiRequestParams.CoauthLockType = parsedCoauthLockType;
				}
				else
				{
					throw new ArgumentException(String.Format("CoauthLockType expected to be one of '{0}' or '{1}' or '{2}' but is actually '{3}'",
						CoauthLockType.Coauth,
						CoauthLockType.CoauthExclusive,
						CoauthLockType.None,
						parsedCoauthLockType));
				}
			}

			switch (elementName)
			{
				case Constants.Requests.CheckFile:
					return new CheckFileWopiRequest(wopiRequestParams);
				case Constants.Requests.Lock:
					return new LockWopiRequest(wopiRequestParams);
				case Constants.Requests.Unlock:
					return new UnlockWopiRequest(wopiRequestParams);
				case Constants.Requests.UnlockAndRelock:
					return new UnlockAndRelockWopiRequest(wopiRequestParams);
				case Constants.Requests.RefreshLock:
					return new RefreshLockWopiRequest(wopiRequestParams);
				case Constants.Requests.GetLock:
					return new GetLockWopiRequest(wopiRequestParams);
				case Constants.Requests.GetFile:
					return new GetFileWopiRequest(wopiRequestParams);
				case Constants.Requests.PutFile:
					return new PutFileWopiRequest(wopiRequestParams);
				case Constants.Requests.PutRelativeFile:
					return new PutRelativeFileWopiRequest(wopiRequestParams, fileNameGuid);
				case Constants.Requests.CheckEcosystem:
					return new CheckEcosystemRequest(wopiRequestParams);
				case Constants.Requests.GetNewAccessToken:
					return new GetNewAccessTokenRequest(wopiRequestParams);
				case Constants.Requests.GetRootContainer:
					return new GetRootContainerRequest(wopiRequestParams);
				case Constants.Requests.CheckContainer:
					return new CheckContainerRequest(wopiRequestParams);
				case Constants.Requests.EnumerateChildren:
					return new EnumerateChildrenRequest(wopiRequestParams);
				case Constants.Requests.EnumerateAncestors:
					return new EnumerateAncestorsRequest(wopiRequestParams);
				case Constants.Requests.GetEcosystem:
					return new GetEcosystemRequest(wopiRequestParams);
				case Constants.Requests.CreateChildContainer:
					return new CreateChildContainerRequest(wopiRequestParams);
				case Constants.Requests.CreateChildFile:
					return new CreateChildFileRequest(wopiRequestParams, fileNameGuid);
				case Constants.Requests.DeleteFile:
					return new DeleteFileRequest(wopiRequestParams);
				case Constants.Requests.DeleteContainer:
					return new DeleteContainerRequest(wopiRequestParams);
				case Constants.Requests.RenameContainer:
					return new RenameContainerRequest(wopiRequestParams);
				case Constants.Requests.RenameFile:
					return new RenameFileRequest(wopiRequestParams, fileNameGuid);
				case Constants.Requests.GetFromFileUrl:
					return new GetFromFileUrlRequest(wopiRequestParams);
				case Constants.Requests.GetShareUrl:
					return new GetShareUrlRequest(wopiRequestParams);
				case Constants.Requests.AddActivities:
					return new AddActivitiesRequest(wopiRequestParams);
				case Constants.Requests.PutUserInfo:
					return new PutUserInfoRequest(wopiRequestParams);
				case Constants.Requests.GetCoauthLock:
					return new GetCoauthLockRequest(wopiRequestParams);
				case Constants.Requests.GetCoauthTable:
					return new GetCoauthTableRequest(wopiRequestParams);
				case Constants.Requests.RefreshCoauthLock:
					return new RefreshCoauthLock(wopiRequestParams);
				case Constants.Requests.UnlockCoauthLock:
					return new UnlockCoauthLockRequest(wopiRequestParams);
				case Constants.Requests.GetChunkedFile:
					return new GetChunkedFileRequest(wopiRequestParams);
				case Constants.Requests.PutChunkedFile:
					return new PutChunkedFileRequest(wopiRequestParams);
				case Constants.Requests.GetSequenceNumber:
					return new GetSequenceNumberRequest(wopiRequestParams);
				case Constants.Requests.Delay:
					return new DelayRequest(wopiRequestParams);
				default:
					throw new ArgumentException(string.Format("Unknown request: '{0}'", elementName));
			}
		}

		private static IValidator[] GetDefaultValidators()
		{
			return new IValidator[]
			{
				new ResponseCodeValidator(Constants.ResponseCodes.Success)
			};
		}
	}
}
