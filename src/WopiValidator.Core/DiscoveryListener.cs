using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Office.WopiValidator.Core
{
	public class DiscoveryListener
	{
		private TcpListener listener = null;
		public string proofKey;
		public string proofKeyOld;
		private string progid = null;
		private int port = -1;

		public DiscoveryListener(string proofKey, string proofKeyOld, int port = 80, string progid = "OneNote.Notebook")
		{
			this.proofKey = proofKey;
			this.proofKeyOld = proofKeyOld;
			this.progid = progid;
			this.port = port;
		}

		public void Start()
		{
			if (listener == null)
			{
				IPAddress address = IPAddress.Any;
				IPEndPoint endPoint = new IPEndPoint(address, this.port);
				listener = new TcpListener(endPoint);
			}

			try
			{
				listener.Start();
				listener.BeginAcceptTcpClient(HandleRequest, null);
			}
			catch (Exception ex)
			{

			}
		}

		private void HandleRequest(IAsyncResult result)
		{
			TcpClient client = listener.EndAcceptTcpClient(result);
			NetworkStream netstream = client.GetStream();

			listener.BeginAcceptTcpClient(HandleRequest, null);

			byte[] buffer = new byte[2048];

			int receivelength = netstream.Read(buffer, 0, 2048);
			string requeststring = Encoding.UTF8.GetString(buffer, 0, receivelength);

			if (!requeststring.StartsWith(@"GET /hosting/discovery", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			string xmlBody = GetDiscoveryResponseXmlString();

			string statusLine = "HTTP/1.1 200 OK\r\n";
			byte[] responseStatusLineBytes = Encoding.UTF8.GetBytes(statusLine);

			string responseHeader =
				string.Format(
					"Content-Type: text/xml; charset=UTf-8\r\nContent-Length: {0}\r\n", xmlBody.Length);
			byte[] responseHeaderBytes = Encoding.UTF8.GetBytes(responseHeader);

			byte[] responseBodyBytes = Encoding.UTF8.GetBytes(xmlBody);

			netstream.Write(responseStatusLineBytes, 0, responseStatusLineBytes.Length);
			netstream.Write(responseHeaderBytes, 0, responseHeaderBytes.Length);
			netstream.Write(new byte[] { 13, 10 }, 0, 2);
			netstream.Write(responseBodyBytes, 0, responseBodyBytes.Length);
			client.Close();
		}

		/// <summary>
		/// Discovery WOPI discovery response xml. It indicates the WOPI client supports 4 types file extensions: ".txt", ".zip", ".one" , ".wopitest" 
		/// </summary>
		/// <returns>Discovery response xml.</returns>
		public string GetDiscoveryResponseXmlString()
        {
			ct_wopidiscovery wopiDiscovery = new ct_wopidiscovery();

			// Add http and https net zone into the wopiDiscovery
			wopiDiscovery.netzone = GetNetZones();

			// ProofKey element
			wopiDiscovery.proofkey = new ct_proofkey();
			wopiDiscovery.proofkey.oldvalue = proofKeyOld;
			wopiDiscovery.proofkey.value = proofKey;
			string xmlStringOfResponseDiscovery = GetDiscoveryXmlFromDiscoveryObject(wopiDiscovery);

			return xmlStringOfResponseDiscovery;
		}

		/// <summary>
		/// Get internal http and internal https ct_netzone.
		/// </summary>
		/// <returns>An array of ct_netzone type instances.</returns>
		private ct_netzone[] GetNetZones()
		{
			string fakedWOPIClientActionHostName = string.Format(@"{0}.com", Guid.NewGuid().ToString("N"));

			// HTTP net zone
			ct_netzone httpNetZone = GetSingleNetZoneForWopiDiscoveryResponse(st_wopizone.internalhttp, fakedWOPIClientActionHostName);

			// HTTPS Net zone
			ct_netzone httpsNetZone = GetSingleNetZoneForWopiDiscoveryResponse(st_wopizone.internalhttps, fakedWOPIClientActionHostName);

			return new ct_netzone[] { httpNetZone, httpsNetZone };
		}

		/// <summary>
		/// Get a single ct_netzone type instance for current test client.
		/// </summary>
		/// <param name="netZoneType">protocol and intended network-type </param>
		/// <param name="fakedWOPIClientActionHostName">Host name for the action of the WOPI client supports.</param>
		/// <returns>A ct_netzone type instance.</returns>
		private ct_netzone GetSingleNetZoneForWopiDiscoveryResponse(st_wopizone netZoneType, string fakedWOPIClientActionHostName)
		{
			string clientName = Dns.GetHostName();

			string transportValue = st_wopizone.internalhttp == netZoneType ? Uri.UriSchemeHttp : Uri.UriSchemeHttps;
			Random radomInstance = new Random((int)DateTime.UtcNow.Ticks & 0x0000FFFF);
			string appName = string.Format(
								@"MSWOPITESTAPP {0} _for {1} WOPIServer_{2}",
								radomInstance.Next(),
								clientName,
								netZoneType);

			Uri favIconUrlValue = new Uri(
							string.Format(@"{0}://{1}/wv/resources/1033/FavIcon_Word.ico", transportValue, fakedWOPIClientActionHostName),
							UriKind.Absolute);

			Uri urlsrcValueOfTextFile = new Uri(
							string.Format(@"{0}://{1}/wv/wordviewerframe.aspx?&lt;ui=UI_LLCC&amp;&gt;&lt;rs=DC_LLCC&amp;&gt;&lt;showpagestats=PERFSTATS&amp;&gt;", transportValue, fakedWOPIClientActionHostName),
							UriKind.Absolute);

			Uri urlsrcValueOfZipFile = new Uri(
							string.Format(@"{0}://{1}/wv/zipviewerframe.aspx?&lt;ui=UI_LLCC&amp;&gt;&lt;rs=DC_LLCC&amp;&gt;&lt;showpagestats=PERFSTATS&amp;&gt;", transportValue, fakedWOPIClientActionHostName),
							UriKind.Absolute);

			Uri urlsrcValueOfUsingprogid = new Uri(
							string.Format(@"{0}://{1}/o/onenoteframe.aspx?edit=0&amp;&lt;ui=UI_LLCC&amp;&gt;&lt;rs=DC_LLCC&amp;&gt;&lt;showpagestats=PERFSTATS&amp;&gt;", transportValue, fakedWOPIClientActionHostName),
							UriKind.Absolute);

			// Setting netZone's sub element's values
			ct_appname appElement = new ct_appname();
			appElement.name = appName;
			appElement.favIconUrl = favIconUrlValue.OriginalString;
			appElement.checkLicense = true;

			// Action element for txt file
			ct_wopiaction actionForTextFile = new ct_wopiaction();
			actionForTextFile.name = st_wopiactionvalues.view;
			actionForTextFile.ext = "txt";
			actionForTextFile.requires = "containers";
			actionForTextFile.@default = true;
			actionForTextFile.urlsrc = urlsrcValueOfTextFile.OriginalString;

			// Action element for txt file
			ct_wopiaction formeditactionForTextFile = new ct_wopiaction();
			formeditactionForTextFile.name = st_wopiactionvalues.formedit;
			formeditactionForTextFile.ext = "txt";
			formeditactionForTextFile.@default = true;
			formeditactionForTextFile.urlsrc = urlsrcValueOfTextFile.OriginalString;

			ct_wopiaction formViewactionForTextFile = new ct_wopiaction();
			formViewactionForTextFile.name = st_wopiactionvalues.formsubmit;
			formViewactionForTextFile.ext = "txt";
			formViewactionForTextFile.@default = true;
			formViewactionForTextFile.urlsrc = urlsrcValueOfTextFile.OriginalString;

			// Action element for zip file
			ct_wopiaction actionForZipFile = new ct_wopiaction();
			actionForZipFile.name = st_wopiactionvalues.view;
			actionForZipFile.ext = "zip";
			actionForZipFile.@default = true;
			actionForZipFile.urlsrc = urlsrcValueOfZipFile.OriginalString;

			// Action elements for one note.
			ct_wopiaction actionForOneNote = new ct_wopiaction();
			actionForOneNote.name = st_wopiactionvalues.view;
			actionForOneNote.ext = "one";
			actionForOneNote.requires = "cobalt";
			actionForOneNote.@default = true;
			actionForOneNote.urlsrc = urlsrcValueOfUsingprogid.OriginalString;

			// Action elements for one note.
			ct_wopiaction actionForOneNoteProg = new ct_wopiaction();
			actionForOneNoteProg.name = st_wopiactionvalues.view;
			actionForOneNoteProg.progid = progid;
			actionForOneNoteProg.requires = "cobalt,containers";
			actionForOneNoteProg.@default = true;
			actionForOneNoteProg.urlsrc = urlsrcValueOfUsingprogid.OriginalString;

			// Action element for wopitest file
			ct_wopiaction actionForWopitestFile = new ct_wopiaction();
			actionForWopitestFile.name = st_wopiactionvalues.view;
			actionForWopitestFile.ext = "wopitest";
			actionForWopitestFile.requires = "containers";
			actionForWopitestFile.@default = true;
			actionForWopitestFile.urlsrc = urlsrcValueOfTextFile.OriginalString;

			ct_wopiaction formeditactionForWopitestFile = new ct_wopiaction();
			formeditactionForWopitestFile.name = st_wopiactionvalues.formedit;
			formeditactionForWopitestFile.ext = "wopitest";
			formeditactionForWopitestFile.@default = true;
			formeditactionForWopitestFile.urlsrc = urlsrcValueOfTextFile.OriginalString;

			ct_wopiaction formViewactionForWopitestFile = new ct_wopiaction();
			formViewactionForWopitestFile.name = st_wopiactionvalues.formsubmit;
			formViewactionForWopitestFile.ext = "wopitest";
			formViewactionForWopitestFile.@default = true;
			formViewactionForWopitestFile.urlsrc = urlsrcValueOfTextFile.OriginalString;

			// Add action elements into the app element.
			appElement.action = new ct_wopiaction[] {
				actionForTextFile,
				actionForOneNote,
				actionForZipFile,
				formeditactionForTextFile,
				formViewactionForTextFile,
				actionForOneNoteProg,
				actionForWopitestFile,
				formeditactionForWopitestFile,
				formViewactionForWopitestFile };

			// Add app element into the netzone element.
			ct_netzone netZoneInstance = new ct_netzone();
			netZoneInstance.app = new ct_appname[] { appElement };
			netZoneInstance.name = netZoneType;
			netZoneInstance.nameSpecified = true;
			return netZoneInstance;
		}

		/// <summary>
		/// Get a xml string from a WOPI Discovery type object.
		/// </summary>
		/// <param name="wopiDiscovery">ct_wopidiscovery instance.</param>
		/// <returns>xml string which contains discovery information.</returns>
		public string GetDiscoveryXmlFromDiscoveryObject(ct_wopidiscovery wopiDiscovery)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ct_wopidiscovery));
			string xmlString = string.Empty;

			MemoryStream memorySteam = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memorySteam, Encoding.UTF8);

			// Remove w3c default namespace prefix in serialize process.
			XmlSerializerNamespaces nameSpaceInstance = new XmlSerializerNamespaces();
			nameSpaceInstance.Add(string.Empty, string.Empty);
			xmlSerializer.Serialize(streamWriter, wopiDiscovery, nameSpaceInstance);

			// Read the MemoryStream to output the xml string.
			memorySteam.Position = 0;
			using (StreamReader streamReader = new StreamReader(memorySteam))
			{
				xmlString = streamReader.ReadToEnd();
			}

			streamWriter.Dispose();
			memorySteam.Dispose();

			// Format the serialized xml string.
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlString);
			return xmlDoc.OuterXml;
		}
	}
}
