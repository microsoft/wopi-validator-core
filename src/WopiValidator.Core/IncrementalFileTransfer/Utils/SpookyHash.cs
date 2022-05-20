using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Office.WopiValidator.Core.IncrementalFileTransfer
{
	/// <summary>
	/// This is .Net implementation of the public domain spookyhash algorithm (native version is already in use office).
	/// More information about this algorithm can be found in //depot/devmain/opensource/spookyhash/public/SpookyHash.h or by visiting
	/// the web site http://burtleburtle.net/bob/hash/spooky.html
	/// This class returns only 128-bit hashes.
	/// Thread safety: It is thread safe. The only state that is stored in the class are the seeds. They cannot be modified after 
	/// creation of the SpookyHash object
	/// </summary>
	public unsafe class SpookyHash
	{
		// Constant to be used as one of the seeds
		private const UInt64 ConstantSeed = 0xdeadbeefdeadbeef;

		// Optional seeds. By default, they are set to 0
		private readonly UInt64 _seed1;
		private readonly UInt64 _seed2;

		// This will be used to store the hash value of data of zero length so as to avoid unnecessary recalculation
		private readonly Lazy<string> _zeroLengthDataHashValue;

		/*
			These values are no longer necessary since all loops have been unrolled manually. Leaving them here for the sake of reference 

			// Values that are used during the different stages of calculating the hash
			private static readonly int[] RotationValuesForMix = { 11, 32, 43, 31, 17, 28, 39, 57, 55, 54, 22, 46 };
			private static readonly int[] RotationValuesForEndPartial = { 44, 15, 34, 21, 38, 33, 10, 13, 38, 53, 42, 54 };
			private static readonly int[][] ShortEndRotationValues =
			{
				new[] {15, 52, 26, 51},
				new[] {28, 9, 47, 54},
				new[] {32, 25, 63}
			};

			private static readonly int[][] ShortMixRotationValues =
			{
				new[] {50, 52, 30, 41},
				new[] {54, 48, 38, 37},
				new[] {62, 34, 5, 36}
			};
		*/

		// Constructors
		public SpookyHash() : this(0, 0)
		{
		}

		public SpookyHash(UInt64 seed) : this(seed, 0)
		{
		}

		public SpookyHash(UInt64 seed1, UInt64 seed2)
		{
			_seed1 = seed1;
			_seed2 = seed2;
			_zeroLengthDataHashValue = new Lazy<string>(() => CalculateHashAndConvertToString(new MemoryStream(new byte[0])));
		}

		/// <summary>
		/// Entry point for calculating the hash of the given data. Takes a byte array as input
		/// and returns a 128-bit base-64 encoded string as hash.
		/// 
		/// Be aware that the implementation of this function does differ from the Native implementation!
		/// %SDXROOT%\tenantppt\ppt\core\types\util.cpp
		/// 
		/// Taking a look at the definition of SpookyHash as defined on http://www.burtleburtle.net/bob/hash/spooky.html
		/// SpookyHash should return a 128b hash value. Looking at the code however, it is never defined how to use those
		/// 128b, as the code returns two uint64 values, and does not define on how to combine them to a single hex string.
		/// Depending on endianness and interpretations on how to combine those two numbers, multiple implementations 
		/// are possible, and it just so happens that the one that is already in use in managed differs from the one
		/// used here.
		/// It might be necessary to reimplement this method if the goal is to have same hashes across managed and native.
		/// This implementation was not changed because we don't want existing hashes (already stored in pptx files) to 
		/// change.
		/// 
		/// Example:
		/// Given the input "foobar"
		/// upper will be 0x86-C0-57-A5-03-ED-DE-99
		/// lower will be 0x65-17-8F-E2-4E-37-62-9A
		/// 
		/// Because of little endian CPU architecture, those two numbers will in memory be written as:
		/// 99-DE-ED-03-A5-57-C0-86
		/// 9A-62-37-4E-E2-8F-17-65
		/// 
		/// How those two values are consumed in native:
		/// A single 16B array is created to look like this (concatenate upper and lower):
		/// 86-C0-57-A5-03-ED-DE-99-65-17-8F-E2-4E-37-62-9A
		/// 
		/// How those two values are consumed in managed:
		/// The two numbers are converted to a byte array by keeping their in memory order and than concatenated into a single 16B array:
		/// 99-DE-ED-03-A5-57-C0-86-9A-62-37-4E-E2-8F-17-65
		/// 
		/// I was also able to find a third implementation that does the following:
		/// The two numbers are written one after another in memory (as seen in managed), as the value should be 128b
		/// it is read like a little endian 128b value should be read, resulting in:
		/// 65-17-8F-E2-4E-37-62-9A-86-C0-57-A5-03-ED-DE-99
		/// 
		/// </summary>
		/// <param name="data">Data to be hashed</param>
		/// <returns>128 bit base 64 encoded string</returns>
		public string CalculateHash(byte[] data)
		{
			using (var dataStream = new MemoryStream(data, writable: false))
				return CalculateHash(dataStream);
		}

		public string CalculateHash(Stream data)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			if (data.Length == 0)
			{
				return _zeroLengthDataHashValue.Value;
			}

			return CalculateHashAndConvertToString(data);
		}

		private string CalculateHashAndConvertToString(Stream data)
		{
			UInt64 first, second;
			CalculateHashImp(data, out first, out second);

			byte[] byteHash1 = BitConverter.GetBytes(first);
			byte[] byteHash2 = BitConverter.GetBytes(second);

			// If system is not little endian, convert the bytes to little endian order
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(byteHash1);
				Array.Reverse(byteHash2);
			}

			byte[] finalHashInBytes = new byte[16];
			Array.Copy(byteHash1, 0, finalHashInBytes, 0, 8);
			Array.Copy(byteHash2, 0, finalHashInBytes, 8, 8);
			string finalHashString = Convert.ToBase64String(finalHashInBytes);

			return finalHashString;
		}

		/// <summary>
		/// This method does majority of the work in calculating the hash.
		/// We use 64-bit integer variables to track the different values while calculating the hash.
		/// This method can be used for callers who want to match the native implementation, as mentioned
		/// in the description for CalculateHash.
		/// </summary>
		/// <param name="data">Data to be hashed</param>
		/// <returns>Two 64-bit integers that contain the hashes</returns>
		public void CalculateHashImp(Stream data, out UInt64 first, out UInt64 second)
		{
			data.Position = 0;

			int dataLength = (int)data.Length;
			if (dataLength < 192)
			{
				var smallDataBuffer = new byte[dataLength];
				data.Read(smallDataBuffer, 0, smallDataBuffer.Length);
				CalculateHashForSmallData(smallDataBuffer, out first, out second);
				return;
			}

			UInt64[] hashes = new UInt64[12];
			fixed (UInt64* hashesPtr = hashes)
			{
				*hashesPtr = _seed1;
				*(hashesPtr + 3) = _seed1;
				*(hashesPtr + 6) = _seed1;
				*(hashesPtr + 9) = _seed1;

				*(hashesPtr + 1) = _seed2;
				*(hashesPtr + 4) = _seed2;
				*(hashesPtr + 7) = _seed2;
				*(hashesPtr + 10) = _seed2;

				*(hashesPtr + 2) = ConstantSeed;
				*(hashesPtr + 5) = ConstantSeed;
				*(hashesPtr + 8) = ConstantSeed;
				*(hashesPtr + 11) = ConstantSeed;

				var buffer = new byte[96];
				fixed (byte* bufferBytePtr = buffer)
				{
					UInt64* bufferPtr = (UInt64*)bufferBytePtr;

					// Manipulate data in 96-byte chunks
					int amountReadFromStream = 0;
					while (dataLength - amountReadFromStream >= 96)
					{
						int amountRead = data.Read(buffer, 0, 96);
						if (amountRead != 96)
							throw new InvalidOperationException("Did not read the appropriate number of bytes");

						Mix(bufferPtr, hashesPtr);
						amountReadFromStream += 96;
					}

					// Handle the remaining data
					int remainderLength = dataLength - amountReadFromStream;
					Array.Clear(buffer, 0, buffer.Length);
					data.Read(buffer, 0, remainderLength);
					buffer[95] = (byte)remainderLength;

					// Calculate hash for the remainder
					End(bufferPtr, hashesPtr);
				}

				// Final 128 bit hash
				first = *hashesPtr;
				second = *(hashesPtr + 1);
			}
		}

		// This method is used when the data size < 192 bytes
		private void CalculateHashForSmallData(byte[] data, out UInt64 first, out UInt64 second)
		{
			UInt64[] hashes = new UInt64[4];
			fixed (UInt64* hashesPtr = hashes)
			fixed (byte* dataBytePtr = data)
			{
				UInt64* dataPtr = (UInt64*)dataBytePtr;

				UInt64* hashesPtrPlusOne = hashesPtr + 1;
				UInt64* hashesPtrPlusTwo = hashesPtr + 2;
				UInt64* hashesPtrPlusThree = hashesPtr + 3;

				int dataLength = data.Length;
				int remainderLength = dataLength % 32;

				*hashesPtr = _seed1;
				*hashesPtrPlusOne = _seed2;
				*hashesPtrPlusTwo = *hashesPtrPlusThree = ConstantSeed;

				int iterator = 0;
				UInt64* dataPtrWithIterator;
				if (dataLength > 15)
				{
					// manipulate the data in 32 byte chunks
					for (; iterator + 31 < dataLength; iterator += 32)
					{
						dataPtrWithIterator = dataPtr + (iterator / 8);
						*hashesPtrPlusTwo += *dataPtrWithIterator;
						*hashesPtrPlusThree += *(dataPtrWithIterator + 1);
						ShortMix(hashesPtr);
						*hashesPtr += *(dataPtrWithIterator + 2);
						*hashesPtrPlusOne += *(dataPtrWithIterator + 3);
					}

					if (remainderLength >= 16)
					{
						dataPtrWithIterator = dataPtr + (iterator / 8);
						*hashesPtrPlusTwo += *dataPtrWithIterator;
						*hashesPtrPlusThree += *(dataPtrWithIterator + 1);
						ShortMix(hashesPtr);
						iterator += 16;
						remainderLength -= 16;
					}
				}

				// Handle last 15 bytes
				*hashesPtrPlusThree += ((ulong)dataLength) << 56;
				dataPtrWithIterator = dataPtr + (iterator / 8);

				switch (remainderLength)
				{
					case 15:
						*hashesPtrPlusThree += ((ulong)data[iterator + 14]) << 48;
						goto case 14;
					case 14:
						*hashesPtrPlusThree += ((ulong)data[iterator + 13]) << 40;
						goto case 13;
					case 13:
						*hashesPtrPlusThree += ((ulong)data[iterator + 12]) << 32;
						goto case 12;
					case 12:
						*hashesPtrPlusTwo += *dataPtrWithIterator;
						*hashesPtrPlusThree += *((UInt32*)(dataPtrWithIterator + 1));
						break;
					case 11:
						*hashesPtrPlusThree += ((ulong)data[iterator + 10]) << 16;
						goto case 10;
					case 10:
						*hashesPtrPlusThree += ((ulong)data[iterator + 9]) << 8;
						goto case 9;
					case 9:
						*hashesPtrPlusThree += ((ulong)data[iterator + 8]);
						goto case 8;
					case 8:
						*hashesPtrPlusTwo += *dataPtrWithIterator;
						break;
					case 7:
						*hashesPtrPlusTwo += ((ulong)data[iterator + 6]) << 48;
						goto case 6;
					case 6:
						*hashesPtrPlusTwo += ((ulong)data[iterator + 5]) << 40;
						goto case 5;
					case 5:
						*hashesPtrPlusTwo += ((ulong)data[iterator + 4]) << 32;
						goto case 4;
					case 4:
						*hashesPtrPlusTwo += *((UInt32*)dataPtrWithIterator);
						break;
					case 3:
						*hashesPtrPlusTwo += ((ulong)data[iterator + 2]) << 16;
						goto case 2;
					case 2:
						*hashesPtrPlusTwo += ((ulong)data[iterator + 1]) << 8;
						goto case 1;
					case 1:
						*hashesPtrPlusTwo += (ulong)data[iterator];
						break;
					case 0:
						*hashesPtrPlusTwo += ConstantSeed;
						*hashesPtrPlusThree += ConstantSeed;
						break;
					default:
						throw new Exception("Unexpected!");
				}

				ShortEnd(hashesPtr);
				first = *hashesPtr;
				second = *hashesPtrPlusOne;
			}
		}

		private static void Mix(UInt64* dataPtr, UInt64* hashesPtr)
		{
			/*
				Manually unrolled loop:

				for (int i = 0; i < 12; i++)
				{
					hashArray[i] += BitConverter.ToUInt64(data, (i * 8));
					hashArray[(i + 2) % 12] ^= hashArray[(i + 10) % 12];
					hashArray[(i + 11) % 12] ^= hashArray[i];
					hashArray[i] = LeftRotate64(hashArray[i], RotationValuesForMix[i]);
					hashArray[(i + 11) % 12] += hashArray[(i + 1) % 12];
				}
			*/

			// Assumption is that there are a valid 96 bytes present in 'data'
			// and hashArray points to our 12 item UInt64 array
			UInt64* currentHashPtr = hashesPtr + 0;

			*currentHashPtr += *dataPtr;
			*(hashesPtr + ((0 + 2) % 12)) ^= *(hashesPtr + ((0 + 10) % 12));
			*(hashesPtr + ((0 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 11);
			*(hashesPtr + ((0 + 11) % 12)) += *(hashesPtr + ((0 + 1) % 12));

			currentHashPtr = hashesPtr + 1;

			*currentHashPtr += *(dataPtr + 1);
			*(hashesPtr + ((1 + 2) % 12)) ^= *(hashesPtr + ((1 + 10) % 12));
			*(hashesPtr + ((1 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 32);
			*(hashesPtr + ((1 + 11) % 12)) += *(hashesPtr + ((1 + 1) % 12));

			currentHashPtr = hashesPtr + 2;

			*currentHashPtr += *(dataPtr + 2);
			*(hashesPtr + ((2 + 2) % 12)) ^= *(hashesPtr + ((2 + 10) % 12));
			*(hashesPtr + ((2 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 43);
			*(hashesPtr + ((2 + 11) % 12)) += *(hashesPtr + ((2 + 1) % 12));

			currentHashPtr = hashesPtr + 3;

			*currentHashPtr += *(dataPtr + 3);
			*(hashesPtr + ((3 + 2) % 12)) ^= *(hashesPtr + ((3 + 10) % 12));
			*(hashesPtr + ((3 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 31);
			*(hashesPtr + ((3 + 11) % 12)) += *(hashesPtr + ((3 + 1) % 12));

			currentHashPtr = hashesPtr + 4;

			*currentHashPtr += *(dataPtr + 4);
			*(hashesPtr + ((4 + 2) % 12)) ^= *(hashesPtr + ((4 + 10) % 12));
			*(hashesPtr + ((4 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 17);
			*(hashesPtr + ((4 + 11) % 12)) += *(hashesPtr + ((4 + 1) % 12));

			currentHashPtr = hashesPtr + 5;

			*currentHashPtr += *(dataPtr + 5);
			*(hashesPtr + ((5 + 2) % 12)) ^= *(hashesPtr + ((5 + 10) % 12));
			*(hashesPtr + ((5 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 28);
			*(hashesPtr + ((5 + 11) % 12)) += *(hashesPtr + ((5 + 1) % 12));

			currentHashPtr = hashesPtr + 6;

			*currentHashPtr += *(dataPtr + 6);
			*(hashesPtr + ((6 + 2) % 12)) ^= *(hashesPtr + ((6 + 10) % 12));
			*(hashesPtr + ((6 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 39);
			*(hashesPtr + ((6 + 11) % 12)) += *(hashesPtr + ((6 + 1) % 12));

			currentHashPtr = hashesPtr + 7;

			*currentHashPtr += *(dataPtr + 7);
			*(hashesPtr + ((7 + 2) % 12)) ^= *(hashesPtr + ((7 + 10) % 12));
			*(hashesPtr + ((7 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 57);
			*(hashesPtr + ((7 + 11) % 12)) += *(hashesPtr + ((7 + 1) % 12));

			currentHashPtr = hashesPtr + 8;

			*currentHashPtr += *(dataPtr + 8);
			*(hashesPtr + ((8 + 2) % 12)) ^= *(hashesPtr + ((8 + 10) % 12));
			*(hashesPtr + ((8 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 55);
			*(hashesPtr + ((8 + 11) % 12)) += *(hashesPtr + ((8 + 1) % 12));

			currentHashPtr = hashesPtr + 9;

			*currentHashPtr += *(dataPtr + 9);
			*(hashesPtr + ((9 + 2) % 12)) ^= *(hashesPtr + ((9 + 10) % 12));
			*(hashesPtr + ((9 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 54);
			*(hashesPtr + ((9 + 11) % 12)) += *(hashesPtr + ((9 + 1) % 12));

			currentHashPtr = hashesPtr + 10;

			*currentHashPtr += *(dataPtr + 10);
			*(hashesPtr + ((10 + 2) % 12)) ^= *(hashesPtr + ((10 + 10) % 12));
			*(hashesPtr + ((10 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 22);
			*(hashesPtr + ((10 + 11) % 12)) += *(hashesPtr + ((10 + 1) % 12));

			currentHashPtr = hashesPtr + 11;

			*currentHashPtr += *(dataPtr + 11);
			*(hashesPtr + ((11 + 2) % 12)) ^= *(hashesPtr + ((11 + 10) % 12));
			*(hashesPtr + ((11 + 11) % 12)) ^= *currentHashPtr;
			*currentHashPtr = LeftRotate64(*currentHashPtr, 46);
			*(hashesPtr + ((11 + 11) % 12)) += *(hashesPtr + ((11 + 1) % 12));
		}

		private static void EndPartial(UInt64* hashesPtr)
		{
			/*
				Manually unrolled loop:
		
				for (int i = 0; i < 12; i++)
				{
					hashArray[(i + 11) % 12] += hashArray[(i + 1) % 12];
					hashArray[(i + 2) % 12] ^= hashArray[(i + 11) % 12];
					hashArray[(i + 1) % 12] = LeftRotate64(hashArray[(i + 1) % 12], RotationValuesForEndPartial[i]);
					first = *hashesPtr;
					second = *hashesPtrPlusOne;
				}
			*/

			*(hashesPtr + ((0 + 11) % 12)) += *(hashesPtr + ((0 + 1) % 12));
			*(hashesPtr + ((0 + 2) % 12)) ^= *(hashesPtr + ((0 + 11) % 12));
			*(hashesPtr + ((0 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((0 + 1) % 12)), 44);

			*(hashesPtr + ((1 + 11) % 12)) += *(hashesPtr + ((1 + 1) % 12));
			*(hashesPtr + ((1 + 2) % 12)) ^= *(hashesPtr + ((1 + 11) % 12));
			*(hashesPtr + ((1 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((1 + 1) % 12)), 15);

			*(hashesPtr + ((2 + 11) % 12)) += *(hashesPtr + ((2 + 1) % 12));
			*(hashesPtr + ((2 + 2) % 12)) ^= *(hashesPtr + ((2 + 11) % 12));
			*(hashesPtr + ((2 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((2 + 1) % 12)), 34);

			*(hashesPtr + ((3 + 11) % 12)) += *(hashesPtr + ((3 + 1) % 12));
			*(hashesPtr + ((3 + 2) % 12)) ^= *(hashesPtr + ((3 + 11) % 12));
			*(hashesPtr + ((3 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((3 + 1) % 12)), 21);

			*(hashesPtr + ((4 + 11) % 12)) += *(hashesPtr + ((4 + 1) % 12));
			*(hashesPtr + ((4 + 2) % 12)) ^= *(hashesPtr + ((4 + 11) % 12));
			*(hashesPtr + ((4 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((4 + 1) % 12)), 38);

			*(hashesPtr + ((5 + 11) % 12)) += *(hashesPtr + ((5 + 1) % 12));
			*(hashesPtr + ((5 + 2) % 12)) ^= *(hashesPtr + ((5 + 11) % 12));
			*(hashesPtr + ((5 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((5 + 1) % 12)), 33);

			*(hashesPtr + ((6 + 11) % 12)) += *(hashesPtr + ((6 + 1) % 12));
			*(hashesPtr + ((6 + 2) % 12)) ^= *(hashesPtr + ((6 + 11) % 12));
			*(hashesPtr + ((6 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((6 + 1) % 12)), 10);

			*(hashesPtr + ((7 + 11) % 12)) += *(hashesPtr + ((7 + 1) % 12));
			*(hashesPtr + ((7 + 2) % 12)) ^= *(hashesPtr + ((7 + 11) % 12));
			*(hashesPtr + ((7 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((7 + 1) % 12)), 13);

			*(hashesPtr + ((8 + 11) % 12)) += *(hashesPtr + ((8 + 1) % 12));
			*(hashesPtr + ((8 + 2) % 12)) ^= *(hashesPtr + ((8 + 11) % 12));
			*(hashesPtr + ((8 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((8 + 1) % 12)), 38);

			*(hashesPtr + ((9 + 11) % 12)) += *(hashesPtr + ((9 + 1) % 12));
			*(hashesPtr + ((9 + 2) % 12)) ^= *(hashesPtr + ((9 + 11) % 12));
			*(hashesPtr + ((9 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((9 + 1) % 12)), 53);

			*(hashesPtr + ((10 + 11) % 12)) += *(hashesPtr + ((10 + 1) % 12));
			*(hashesPtr + ((10 + 2) % 12)) ^= *(hashesPtr + ((10 + 11) % 12));
			*(hashesPtr + ((10 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((10 + 1) % 12)), 42);

			*(hashesPtr + ((11 + 11) % 12)) += *(hashesPtr + ((11 + 1) % 12));
			*(hashesPtr + ((11 + 2) % 12)) ^= *(hashesPtr + ((11 + 11) % 12));
			*(hashesPtr + ((11 + 1) % 12)) = LeftRotate64(*(hashesPtr + ((11 + 1) % 12)), 54);
		}

		private static void End(UInt64* dataPtr, UInt64* hashesPtr)
		{
			for (int i = 0; i < 12; i++)
			{
				*(hashesPtr + i) += *(dataPtr + i);
			}

			EndPartial(hashesPtr);
			EndPartial(hashesPtr);
			EndPartial(hashesPtr);
		}

		private static void ShortMix(UInt64* hashesPtr)
		{
			/*
				Manually unrolled two nested loops, assuming hashArray.Length == 4 (which is always true)

				int numHashes = hashArray.Length;
				for (int j = 0; j < ShortMixRotationValues.Length; j++)
				{
					for (int i = 0; i < ShortMixRotationValues[j].Length; i++)
					{
						hashArray[(i + 2) % numHashes] = LeftRotate64(hashArray[(i + 2) % numHashes], ShortMixRotationValues[j][i]);
						hashArray[(i + 2) % numHashes] += hashArray[(i + 3) % numHashes];
						hashArray[i] ^= hashArray[(i + 2) % numHashes];
					}
				}
			*/

			*(hashesPtr + ((0 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((0 + 2) % 4)), 50);
			*(hashesPtr + ((0 + 2) % 4)) += *(hashesPtr + ((0 + 3) % 4));
			*hashesPtr ^= *(hashesPtr + ((0 + 2) % 4));

			*(hashesPtr + ((1 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((1 + 2) % 4)), 52);
			*(hashesPtr + ((1 + 2) % 4)) += *(hashesPtr + ((1 + 3) % 4));
			*(hashesPtr + 1) ^= *(hashesPtr + ((1 + 2) % 4));

			*(hashesPtr + ((2 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((2 + 2) % 4)), 30);
			*(hashesPtr + ((2 + 2) % 4)) += *(hashesPtr + ((2 + 3) % 4));
			*(hashesPtr + 2) ^= *(hashesPtr + ((2 + 2) % 4));

			*(hashesPtr + ((3 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((3 + 2) % 4)), 41);
			*(hashesPtr + ((3 + 2) % 4)) += *(hashesPtr + ((3 + 3) % 4));
			*(hashesPtr + 3) ^= *(hashesPtr + ((3 + 2) % 4));

			*(hashesPtr + ((0 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((0 + 2) % 4)), 54);
			*(hashesPtr + ((0 + 2) % 4)) += *(hashesPtr + ((0 + 3) % 4));
			*hashesPtr ^= *(hashesPtr + ((0 + 2) % 4));

			*(hashesPtr + ((1 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((1 + 2) % 4)), 48);
			*(hashesPtr + ((1 + 2) % 4)) += *(hashesPtr + ((1 + 3) % 4));
			*(hashesPtr + 1) ^= *(hashesPtr + ((1 + 2) % 4));

			*(hashesPtr + ((2 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((2 + 2) % 4)), 38);
			*(hashesPtr + ((2 + 2) % 4)) += *(hashesPtr + ((2 + 3) % 4));
			*(hashesPtr + 2) ^= *(hashesPtr + ((2 + 2) % 4));

			*(hashesPtr + ((3 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((3 + 2) % 4)), 37);
			*(hashesPtr + ((3 + 2) % 4)) += *(hashesPtr + ((3 + 3) % 4));
			*(hashesPtr + 3) ^= *(hashesPtr + ((3 + 2) % 4));

			*(hashesPtr + ((0 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((0 + 2) % 4)), 62);
			*(hashesPtr + ((0 + 2) % 4)) += *(hashesPtr + ((0 + 3) % 4));
			*hashesPtr ^= *(hashesPtr + ((0 + 2) % 4));

			*(hashesPtr + ((1 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((1 + 2) % 4)), 34);
			*(hashesPtr + ((1 + 2) % 4)) += *(hashesPtr + ((1 + 3) % 4));
			*(hashesPtr + 1) ^= *(hashesPtr + ((1 + 2) % 4));

			*(hashesPtr + ((2 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((2 + 2) % 4)), 5);
			*(hashesPtr + ((2 + 2) % 4)) += *(hashesPtr + ((2 + 3) % 4));
			*(hashesPtr + 2) ^= *(hashesPtr + ((2 + 2) % 4));

			*(hashesPtr + ((3 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((3 + 2) % 4)), 36);
			*(hashesPtr + ((3 + 2) % 4)) += *(hashesPtr + ((3 + 3) % 4));
			*(hashesPtr + 3) ^= *(hashesPtr + ((3 + 2) % 4));
		}

		private static void ShortEnd(UInt64* hashesPtr)
		{
			/*
				Manually unrolled two nested loops, assuming hashArray.Length == 4 (which is always true)

				int numHashes = hashArray.Length;
				for (int j = 0; j < ShortEndRotationValues.Length; j++)
				{
					for (int i = 0; i < ShortEndRotationValues[j].Length; i++)
					{
						hashArray[(i + 3) % numHashes] ^= hashArray[(i + 2) % numHashes];
						hashArray[(i + 2) % numHashes] = LeftRotate64(hashArray[(i + 2) % numHashes], ShortEndRotationValues[j][i]);
						hashArray[(i + 3) % numHashes] += hashArray[(i + 2) % numHashes];
					}
				}
			*/

			*(hashesPtr + ((0 + 3) % 4)) ^= *(hashesPtr + ((0 + 2) % 4));
			*(hashesPtr + ((0 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((0 + 2) % 4)), 15);
			*(hashesPtr + ((0 + 3) % 4)) += *(hashesPtr + ((0 + 2) % 4));

			*(hashesPtr + ((1 + 3) % 4)) ^= *(hashesPtr + ((1 + 2) % 4));
			*(hashesPtr + ((1 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((1 + 2) % 4)), 52);
			*(hashesPtr + ((1 + 3) % 4)) += *(hashesPtr + ((1 + 2) % 4));

			*(hashesPtr + ((2 + 3) % 4)) ^= *(hashesPtr + ((2 + 2) % 4));
			*(hashesPtr + ((2 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((2 + 2) % 4)), 26);
			*(hashesPtr + ((2 + 3) % 4)) += *(hashesPtr + ((2 + 2) % 4));

			*(hashesPtr + ((3 + 3) % 4)) ^= *(hashesPtr + ((3 + 2) % 4));
			*(hashesPtr + ((3 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((3 + 2) % 4)), 51);
			*(hashesPtr + ((3 + 3) % 4)) += *(hashesPtr + ((3 + 2) % 4));

			*(hashesPtr + ((0 + 3) % 4)) ^= *(hashesPtr + ((0 + 2) % 4));
			*(hashesPtr + ((0 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((0 + 2) % 4)), 28);
			*(hashesPtr + ((0 + 3) % 4)) += *(hashesPtr + ((0 + 2) % 4));

			*(hashesPtr + ((1 + 3) % 4)) ^= *(hashesPtr + ((1 + 2) % 4));
			*(hashesPtr + ((1 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((1 + 2) % 4)), 9);
			*(hashesPtr + ((1 + 3) % 4)) += *(hashesPtr + ((1 + 2) % 4));

			*(hashesPtr + ((2 + 3) % 4)) ^= *(hashesPtr + ((2 + 2) % 4));
			*(hashesPtr + ((2 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((2 + 2) % 4)), 47);
			*(hashesPtr + ((2 + 3) % 4)) += *(hashesPtr + ((2 + 2) % 4));

			*(hashesPtr + ((3 + 3) % 4)) ^= *(hashesPtr + ((3 + 2) % 4));
			*(hashesPtr + ((3 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((3 + 2) % 4)), 54);
			*(hashesPtr + ((3 + 3) % 4)) += *(hashesPtr + ((3 + 2) % 4));

			*(hashesPtr + ((0 + 3) % 4)) ^= *(hashesPtr + ((0 + 2) % 4));
			*(hashesPtr + ((0 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((0 + 2) % 4)), 32);
			*(hashesPtr + ((0 + 3) % 4)) += *(hashesPtr + ((0 + 2) % 4));

			*(hashesPtr + ((1 + 3) % 4)) ^= *(hashesPtr + ((1 + 2) % 4));
			*(hashesPtr + ((1 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((1 + 2) % 4)), 25);
			*(hashesPtr + ((1 + 3) % 4)) += *(hashesPtr + ((1 + 2) % 4));

			*(hashesPtr + ((2 + 3) % 4)) ^= *(hashesPtr + ((2 + 2) % 4));
			*(hashesPtr + ((2 + 2) % 4)) = LeftRotate64(*(hashesPtr + ((2 + 2) % 4)), 63);
			*(hashesPtr + ((2 + 3) % 4)) += *(hashesPtr + ((2 + 2) % 4));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static UInt64 LeftRotate64(UInt64 input, int placesToRotate)
		{
			return (input << placesToRotate) | (input >> (64 - placesToRotate));
		}
	}
}
