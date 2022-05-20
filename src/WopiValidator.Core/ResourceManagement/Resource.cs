// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer;
using Microsoft.Office.WopiValidator.Core.Logging;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.Office.WopiValidator.Core.ResourceManagement
{
	class Resource
	{
		public string ResourceId { get; private set; }
		public string FilePath { get; private set; }
		public string FileName { get; private set; }

		internal Resource(string resourceId, string filePath, string fileName)
		{
			ResourceId = resourceId;
			FilePath = filePath;
			FileName = fileName;
		}

		internal MemoryStream GetContentStream(ILogger logger)
		{
			try
			{
				// Use the filename as the actual content of the stream, unless the FileName is
				// "ZeroByteFile.wopitest". This way we can still write out zero-byte files.
				MemoryStream result = new MemoryStream();
				StreamWriter sw = new StreamWriter(result);
				string fileContent = FileName == "ZeroByteFile.wopitest" ? string.Empty : ResourceId + FilePath + FileName;
				sw.Write(fileContent);
				sw.Flush();
				result.Seek(0, SeekOrigin.Begin);
				return result;
			}
			catch (IOException ex)
			{

				logger.Log("IO Exception when trying to get resource content.");
				logger.Log(ex.Message);
				return null;
			}
		}

		internal void GetZipChunkingBlobs(ILogger logger, out string[] blobIds, out IReadOnlyDictionary<string, IBlob> blobs)
		{
			if (ResourceId.Equals(Constants.ZipChunkingResourceFiles.ZeroByteOfficeDocumentResourceId))
			{
				blobIds = new string[ 0 ];
				blobs = new Dictionary<string, IBlob>();
				return;
			}

			try
			{
				List<string> offsets = new List<string>();

				string chunkIdsResourcePath = FilePath + Constants.ZipChunkingResourceFiles.ChunkIds;
				using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(chunkIdsResourcePath))
				using (StreamReader streamReader = new StreamReader(stream))
				{
					while (!streamReader.EndOfStream)
					{
						offsets.Add(streamReader.ReadLine());
					}
				}

				string[] blobIdsToReturn = new string[ offsets.Count ];
				Dictionary<string, IBlob> blobsToReturn = new Dictionary<string, IBlob>();
				IBlobAllocator blobAllocator = new BlobAllocator();
				string fileStreamResourcePath = FilePath + Constants.ZipChunkingResourceFiles.FileStream;
				using (Stream fs = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileStreamResourcePath))
				{
					for (int i=0; i<offsets.Count; i++)
					{
						if (!int.TryParse(offsets[ i ], out int offset_1))
						{
							throw new IOException($"In {nameof(Resource.GetZipChunkingBlobs)}, FileName:{FileName}, FilePath:{FilePath}, Offset:{offsets[ i ]} parsing failed");
						}

						if (fs.Position != offset_1)
						{
							throw new IOException("Filestream position not equal to offset parsed from ChunkIds.txt");
						}

						int offset_2 = (int)fs.Length;
						if (i + 1 < offsets.Count)
						{
							if (!int.TryParse(offsets[ i + 1 ], out offset_2))
							{
								throw new IOException($"In {nameof(Resource.GetZipChunkingBlobs)}, FileName:{FileName}, FilePath:{FilePath}, Offset:{offsets[ i + 1 ]} parsing failed");
							}
						}

						IBlob blob = blobAllocator.CreateBlob(fs, offset_2 - offset_1);
						blobIdsToReturn[ i ] = blob.BlobId;
						if (!blobsToReturn.ContainsKey(blob.BlobId))
						{
							blobsToReturn.Add(blob.BlobId, blob);
						}
					}
				}

				blobIds = blobIdsToReturn;
				blobs = blobsToReturn;
				return;
			}
			catch (IOException ex)
			{
				logger.Log($"{nameof(Resource.GetZipChunkingBlobs)} IO Exception when trying to get resource content.");
				logger.Log(ex.Message);

				blobIds = new string[0];
				blobs = new Dictionary<string, IBlob>();
				return;
			}
		}

		internal Stream GetZipChunkingResourceStream(ILogger logger)
		{
			if (ResourceId.Equals(Constants.ZipChunkingResourceFiles.ZeroByteOfficeDocumentResourceId))
			{
				return new MemoryStream(new byte[0]);
			}

			try
			{
				string resourcePath = FilePath + Constants.ZipChunkingResourceFiles.FileStream;
				Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
				return stream;
			}
			catch (IOException ex)
			{
				logger.Log($"{nameof(Resource.GetZipChunkingResourceStream)} IO Exception when trying to get resource content.");
				logger.Log(ex.Message);
				return null;
			}
		}
	}
}
