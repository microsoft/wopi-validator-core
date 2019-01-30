// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
		public static IEnumerable<IRequest> GetRequests(XElement definition)
		{
			return definition.Elements().Select(GetRequest);
		}

		/// <summary>
		/// Parses single request definition and instantiates proper IWopiRequest instance based on element name
		/// </summary>
		private static IRequest GetRequest(XElement definition)
		{
			string elementName = definition.Name.LocalName;
			XElement validatorsDefinition = definition.Element("Validators");
			XElement stateDefinition = definition.Element("SaveState");
			XElement mutatorsDefinition = definition.Element("Mutators");
			XElement requestBodyDefinition = definition.Element("RequestBody");

			IEnumerable<IValidator> validators = validatorsDefinition == null ? null : ValidatorFactory.GetValidators(validatorsDefinition);
			IEnumerable<IStateEntry> stateSavers = stateDefinition == null ? null : StateFactory.GetStateExpressions(stateDefinition);
			IEnumerable<IMutator> mutators = mutatorsDefinition == null ? null : MutatorFactory.GetMutators(mutatorsDefinition);

			var wopiRequestParams = new WopiRequestParam
			{
				FileExtensionFilterList = (string)definition.Attribute("FileExtensionFilterList"),
				FolderName = (string)definition.Attribute("FolderName"),
				LockString = (string)definition.Attribute("Lock"),
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
				RestrictedLinkType = (string)definition.Attribute("RestrictedLink")
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

			if (!string.IsNullOrEmpty(ConfigParser.UsingRestrictedScenario))
			{
				wopiRequestParams.UsingRestrictedScenario = ConfigParser.UsingRestrictedScenario;
			}

			if (!string.IsNullOrEmpty(ConfigParser.ApplicationId))
			{
				wopiRequestParams.ApplicationId = ConfigParser.ApplicationId;
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
					return new PutRelativeFileWopiRequest(wopiRequestParams);
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
					return new CreateChildFileRequest(wopiRequestParams);
				case Constants.Requests.DeleteFile:
					return new DeleteFileRequest(wopiRequestParams);
				case Constants.Requests.DeleteContainer:
					return new DeleteContainerRequest(wopiRequestParams);
				case Constants.Requests.RenameContainer:
					return new RenameContainerRequest(wopiRequestParams);
				case Constants.Requests.RenameFile:
					return new RenameFileRequest(wopiRequestParams);
				case Constants.Requests.GetFromFileUrl:
					return new GetFromFileUrlRequest(wopiRequestParams);
				case Constants.Requests.GetShareUrl:
					return new GetShareUrlRequest(wopiRequestParams);
				case Constants.Requests.AddActivities:
					return new AddActivitiesRequest(wopiRequestParams);
				case Constants.Requests.PutUserInfo:
					return new PutUserInfoRequest(wopiRequestParams);
				case Constants.Requests.GetRestrictedLink:
					return new GetRestrictedLinkRequest(wopiRequestParams);
				case Constants.Requests.RevokeRestrictedLink:
					return new RevokeRestrictedLinkRequest(wopiRequestParams);
				case Constants.Requests.ReadSecureStore:
					return new ReadSecureStoreRequest(wopiRequestParams);
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
