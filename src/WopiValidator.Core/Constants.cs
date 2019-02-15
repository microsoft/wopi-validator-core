// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Office.WopiValidator.Core
{
	public static class Constants
	{
		public static class ResponseCodes
		{
			public const int Success = 200;
			public const int TokenIsInvalid = 401;
			public const int FileUnknown = 404;
			public const int UserUnauthorized = 404;
			public const int LockMismatch = 409;
			public const int ServerError = 500;
			public const int Unsupported = 501;
		}

		public static class Headers
		{
			public const string Authorization = "Authorization";

			public const string Override = "X-WOPI-Override";
			public const string Lock = "X-WOPI-Lock";
			public const string OldLock = "X-WOPI-OldLock";
			public const string WopiSrc = "X-WOPI-WopiSrc";
			public const string SuggestedTarget = "X-WOPI-SuggestedTarget"; // UTF7 encoded
			public const string RelativeTarget = "X-WOPI-RelativeTarget"; // UTF7 encoded
			public const string RequestedName = "X-WOPI-RequestedName"; // UTF7 encoded
			public const string FileExtensionFilterList = "X-WOPI-FileExtensionFilterList";
			public const string Size = "X-WOPI-Size";
			public const string ProofKey = "X-WOPI-Proof";
			public const string ProofKeyOld = "X-WOPI-ProofOld";
			public const string WopiTimestamp = "X-WOPI-TimeStamp";
			public const string OverwriteRelative = "X-WOPI-OverwriteRelativeTarget";
			public const string Version = "X-WOPI-ItemVersion";
			public const string UrlType = "X-WOPI-UrlType";
			public const string RestrictedLink = "X-WOPI-RestrictedLink";
			public const string UsingRestrictedScenario = "X-WOPI-UsingRestrictedScenario";
			public const string ApplicationId = "X-WOPI-ApplicationId";
			public const string PerfTraceRequested = "X-WOPI-PerfTraceRequested";

			// This is not an official WOPI header; it is used to pass exception information
			// back to the validator UI. See the ExceptionHelper class for more details.
			public const string ValidatorError = "X-WOPI-ValidatorError";
		}

		public static class HeaderValues
		{
			public const string OfficeNativeClientUserAgent = "Microsoft Office WOPI Validator/1.0";
		}

		public static class Overrides
		{
			public const string Lock = "LOCK";
			public const string Unlock = "UNLOCK";
			public const string RefreshLock = "REFRESH_LOCK";
			public const string GetLock = "GET_LOCK";
			public const string Put = "PUT";
			public const string PutRelative = "PUT_RELATIVE";
			public const string Delete = "DELETE";
			public const string GetNewAccessToken = "GET_NEW_ACCESS_TOKEN";
			public const string GetRootContainer = "GET_ROOT_CONTAINER";
			public const string CreateChildContainer = "CREATE_CHILD_CONTAINER";
			public const string CreateChildFile = "CREATE_CHILD_FILE";
			public const string DeleteContainer = "DELETE_CONTAINER";
			public const string RenameContainer = "RENAME_CONTAINER";
			public const string RenameFile = "RENAME_FILE";
			public const string GetShareUrl = "GET_SHARE_URL";
			public const string AddActivities = "ADD_ACTIVITIES";
			public const string PutUserInfo = "PUT_USER_INFO";
			public const string GetRestrictedLink = "GET_RESTRICTED_LINK";
			public const string RevokeRestrictedLink = "REVOKE_RESTRICTED_LINK";
			public const string ReadSecureStore = "READ_SECURE_STORE";
		}

		public static class RequestMethods
		{
			public const string Post = "POST";
			public const string Get = "GET";
		}

		public static class Requests
		{
			public const string CheckFile = "CheckFileInfo";
			public const string GetFile = "GetFile";
			public const string PutFile = "PutFile";
			public const string PutRelativeFile = "PutRelativeFile";
			public const string Lock = "Lock";
			public const string Unlock = "Unlock";
			public const string RefreshLock = "RefreshLock";
			public const string UnlockAndRelock = "UnlockAndRelock";
			public const string GetLock = "GetLock";
			public const string CheckEcosystem = "CheckEcosystem";
			public const string GetNewAccessToken = "GetNewAccessToken";
			public const string GetRootContainer = "GetRootContainer";
			public const string CheckContainer = "CheckContainerInfo";
			public const string EnumerateChildren = "EnumerateChildren";
			public const string EnumerateAncestors = "EnumerateAncestors";
			public const string GetEcosystem = "GetEcosystem";
			public const string CreateChildContainer = "CreateChildContainer";
			public const string CreateChildFile = "CreateChildFile";
			public const string DeleteFile = "DeleteFile";
			public const string DeleteContainer = "DeleteContainer";
			public const string RenameContainer = "RenameContainer";
			public const string RenameFile = "RenameFile";
			public const string GetFromFileUrl = "GetFromFileUrl";
			public const string GetShareUrl = "GetShareUrl";
			public const string AddActivities = "AddActivities";
			public const string PutUserInfo = "PutUserInfo";
			public const string GetRestrictedLink = "GetRestrictedLink";
			public const string RevokeRestrictedLink = "RevokeRestrictedLink";
			public const string ReadSecureStore = "ReadSecureStore";
			public const string CheckFolderInfo = "CheckFolderInfo";
		}

		public static class Validators
		{
			public const string And = "And";
			public const string JsonResponseContent = "JsonResponseContentValidator";
			public const string JsonSchema = "JsonSchemaValidator";
			public const string LockMismatch = "LockMismatchValidator";
			public const string Or = "Or";
			public const string ResponseCode = "ResponseCodeValidator";
			public const string ResponseContent = "ResponseContentValidator";
			public const string ResponseHeader = "ResponseHeaderValidator";
			public const string ContentLength = "ContentLengthValidator";

			public static class Properties
			{
				public const string AbsoluteUrlProperty = "AbsoluteUrlProperty";
				public const string ArrayProperty = "ArrayProperty";
				public const string BooleanProperty = "BooleanProperty";
				public const string IntegerProperty = "IntegerProperty";
				public const string LongProperty = "LongProperty";
				public const string StringRegexProperty = "StringRegexProperty";
				public const string StringProperty = "StringProperty";
			}
		}

		public static class StateOverrides
		{
			public const string StateToken = "$State:";
			public const string EcosystemUrl = "EcosystemUrl";
			public const string OriginalAccessToken = "OriginalAccessToken";
			public const string OriginalWopiSrc = "OriginalWopiSrc";
		}

		public static class Mutators
		{
			public const string AccessToken = "AccessToken";
			public const string ProofKey = "ProofKey";
		}
	}
}
