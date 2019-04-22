// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Office.WopiValidator.Core;
using System;

namespace Microsoft.Office.WopiValidator
{
	/// <summary>
	/// Options for the discovery command.
	/// </summary>
	[Verb("discovery", HelpText = "Provide XML that describes the supported abilities of this WOPI client")]
	internal class DiscoveryOptions : OptionsBase
	{
		[Option("port", Required = true, HelpText = "Port number used for discovery")]
		public string Port { get; set; }

		[Option("progid", Required = false, HelpText = "progid that identifies a folder as being associated with a specific application")]
		public string ProgId { get; set; }

		[Option('p', "ProofKey", Required = true, HelpText = "Public key used to decrypt X-WOPI-Proof HTTP header")]
		public string ProofKey { get; set; }

		[Option('o', "ProofKeyOld", Required = true, HelpText = "Public key used to decrypt X-WOPI-ProofOld HTTP header")]
		public string ProofKeyOld { get; set; }

		public static ExitCode DiscoveryCommand(DiscoveryOptions options)
		{
			int port;
			if (!Int32.TryParse(options.Port, out port))
			{
				throw new ArgumentException(string.Format("Value for argument 'port' must be an integer, actual value '{0}'.", options.Port));
			}

			DiscoveryListener listener = new DiscoveryListener(options.ProofKey, options.ProofKeyOld, port);
			listener.Start();

			return ExitCode.Success;
		}
	}
}
