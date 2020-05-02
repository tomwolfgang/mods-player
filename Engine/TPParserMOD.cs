using System;
using System.IO;
using System.Text;
using TPlayer.Model;

namespace TPlayer.Engine {
	/// <summary>
	/// This class is a simple MOD parser that supports the MOD extension, that is
	/// ProTracker Module files in its different forms
	/// </summary>
	public class TPParserMOD : ITPParser {
		#region Constants
		
		private const int MOD_TYPE_SIGNATURE_OFFSET = 1080;
		
		#endregion

		/// <summary>
		/// If we have a note without a valid sample number, we will take the last 
		/// </summary>
		//private int m_lastValidSampleNumber = 0;
		
		#region Public Methods

		public TPParserMOD() {
		}

		/// <summary>
		/// Checks if the current parser supports the format.
		/// Note: this function is called by the LoadModule function, so there is no need
		/// to call this function before loading the module.
		/// </summary>
		/// <param name="moduleBuffer">Stream of the module</param>
		/// <param name="typeInfo">optional Module Type Information class that will be filled by the function if a supported format exists</param>
		/// <returns>true for exists, else false</returns>
		public bool IsFormatSupported(Stream moduleBuffer, ref TPModuleTypeInfo typeInfo)	{
			if (null != moduleBuffer) {
				TPModuleTypeInfo tmpTypeInfo = new TPModuleTypeInfo();

        // check for file format tag signature:
				//
				// 1. check trivial stream capabilities
				if (moduleBuffer.CanRead && moduleBuffer.CanSeek)	{
					// 2. see if the buffer is large enough
					if ((MOD_TYPE_SIGNATURE_OFFSET + 4) < moduleBuffer.Length) {
						if (MOD_TYPE_SIGNATURE_OFFSET == moduleBuffer.Seek(MOD_TYPE_SIGNATURE_OFFSET, SeekOrigin.Begin)) {
							// 3. check for known signatures
							byte[] signature = new byte[4];

							if (4 == moduleBuffer.Read(signature, 0, 4)) {
								bool foundType = false;

								string stringSignature = ASCIIEncoding.ASCII.GetString(signature, 0, 4);

								// Mahoney & Kaktus - 4 channel/31 samples
								if ((0 == stringSignature.CompareTo("M.K.")) || 
									  (0 == stringSignature.CompareTo("M!K!")))	{
									tmpTypeInfo.m_formatName		= "ProTracker";
									tmpTypeInfo.m_channels			= 4;
									tmpTypeInfo.m_maxSamples		= 31;
									tmpTypeInfo.m_rowsPerPattern	= 64;

									foundType = true;
								} 
								else if (0 == stringSignature.Substring(0, 3).CompareTo("FLT")) {
                  // Startrekker FLT4/FLT8
                  int channels = stringSignature[3] - '0'; // number delta

									if ((4 == channels) || (8 == channels))	{
										tmpTypeInfo.m_formatName		= "Startrekker";
										tmpTypeInfo.m_channels			= (byte)channels;
										tmpTypeInfo.m_maxSamples		= 31;
										tmpTypeInfo.m_rowsPerPattern	= 64;

										foundType = true;
									}
								} else if ((0 == stringSignature.Substring(1,3).CompareTo("CHN")) ||
										       (0 == stringSignature.Substring(2,2).CompareTo("CH"))) {
                  // FastTracker xCHN/yyCH
                  int channels = stringSignature[0] - '0'; // number delta

									if ((0 == stringSignature.Substring(2,2).CompareTo("CH"))) {
                    // number delta
                    channels = System.Convert.ToInt32(stringSignature.Substring(0,2));
									}
									
									tmpTypeInfo.m_formatName		= "FastTracker";
									tmpTypeInfo.m_channels			= (byte)channels;
									tmpTypeInfo.m_maxSamples		= 31;
									tmpTypeInfo.m_rowsPerPattern	= 64;

									foundType = true;
								} else {
									// if no tag found, then it might be a 15 sample 
									// Soundtracker module - we can only tell if it is valid after 
									// we parse all 15 samples
									tmpTypeInfo.m_formatName		= "Soundtracker";
									tmpTypeInfo.m_channels			= (Byte)4;
									tmpTypeInfo.m_maxSamples		= 15;
									tmpTypeInfo.m_rowsPerPattern	= 64;

									foundType = true;
								}

								if (null != typeInfo) {
									typeInfo = tmpTypeInfo;
								}

								moduleBuffer.Seek(0, System.IO.SeekOrigin.Begin);
								return foundType;
							}
						}

						moduleBuffer.Seek(0, System.IO.SeekOrigin.Begin);
					}
				}

			}

			return false;
		}


		/// <summary>
		/// Loads the module and returns it for play.
		/// Note: this function uses the IsFormatSupported function, so there is no need
		/// to call it beforehand!
		/// </summary>
		/// <param name="moduleBuffer"></param>
		/// <returns></returns>
		public TPModule LoadModule(Stream moduleBuffer)	{
      if (null == moduleBuffer)
        return null;

			TPModuleTypeInfo typeInfo = new TPModuleTypeInfo();

      if (!IsFormatSupported(moduleBuffer, ref typeInfo)) {
        return null;
      }

			TPModule module = new TPModule();

			module.m_moduleTypeInfo = typeInfo;

			// easier to work with a binary reader
			BinaryReader binReader = new BinaryReader(moduleBuffer);

			bool ret = true;

			// ok, so we've got a valid module, now we have to parse it:
			// 1. Parse the name of the module
			if (ret) { ret = ParseModuleName(binReader, ref module); };
					
			// 2. Parse the sample info
			if (ret) { ret = ParseSampleInfo(binReader, ref module); };

			// 3. Parse the order info
			if (ret) { ret = ParseOrderInfo(binReader, ref module); };

			// 4. Skip type signature
			if (typeInfo.m_maxSamples > 15)
			{
				if (ret) { binReader.ReadBytes(4); }
			}

			// 5. Parse patterns
			if (ret) { ret = ParsePatterns(binReader, ref module); };

			// 6. Parse the sample data
			if (ret) { ret = ParseSampleData(binReader, module); };

			binReader.Close();

			if (ret) {
				return module;
			}

      return null;
		}

		public MemoryStream SaveModule(TPModule module) {
			return null;
		}

		public System.Collections.Hashtable GetSupportedExtensions() {
			return null;
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Helper function for parsing the name of the module
		/// </summary>
		/// <param name="binReader"></param>
		/// <param name="module"></param>
		/// <returns></returns>
		private bool ParseModuleName(BinaryReader binReader, ref TPModule module) {
			if ((null != binReader) && 
				  (null != module)) {
				// the name of the module is the first 20 bytes
				byte[] nameInBytes = binReader.ReadBytes(20);
				module.m_moduleName = System.Text.Encoding.ASCII.GetString(nameInBytes,0,20);
				module.m_moduleName = module.m_moduleName.TrimEnd('\0');
				return true;
			}

			return false;
		}

		/// <summary>
		/// Parses the sample information.
		/// A sample information looks like so:
		/// 
		/// Number of bytes			Information
		/// ====================================
		/// 22						Name of sample
		/// 2						Length (in big-endian) / 2
		/// 1 (first 4 bits)		Finetune
		///	1						Volume (0-64)
		/// 2						Repeat Offset (in big-endian) / 2 
		/// 2						Repeat Length (in big-endian) / 2 
		/// 
		/// </summary>
		/// <param name="binReader"></param>
		/// <param name="ModuleObject"></param>
		/// <returns></returns>
		private bool ParseSampleInfo(BinaryReader binReader, ref TPModule module) {
			if ((null != binReader) && 
				  (null != module)) {
				TPModuleTypeInfo typeInfo = module.m_moduleTypeInfo;

				int maxSamplesCount = (int)typeInfo.m_maxSamples;

				module.m_samples = new TPSample[maxSamplesCount];

				// for each sample try to parse the information
				for (int i = 0; i < maxSamplesCount; i++)	{
					module.m_samples[i] = new TPSample();
					TPSample curSample = module.m_samples[i];
						
					// parse name
					byte[] sampleNameInBytes = binReader.ReadBytes(22);
					curSample.m_sampleName = System.Text.Encoding.ASCII.GetString(sampleNameInBytes, 0, 22);														
					curSample.m_sampleName = curSample.m_sampleName.TrimEnd('\0');

					// parse length
					curSample.m_length = TPParserUtilsMOD.ConvertEndianess(binReader.ReadInt16()) * 2;

					// parse finetune index
					curSample.m_finetune = binReader.ReadByte();

					// get lower four bits (nibble)
					curSample.m_finetune &= 0x0F;

					// calculate the C2SPD of the sample (that is the middle c-4) - frequency
					curSample.m_C2SPDFrequencyValue = TPParserUtilsMOD.MOD_FINE_TUNE_TO_FREQUENCY_CONVERSION_TABLE[curSample.m_finetune].C2PSDFrequency;

					// parse volume
					curSample.m_volume = binReader.ReadByte();

					// parse repeat offset
					curSample.m_repeatPos = TPParserUtilsMOD.ConvertEndianess(binReader.ReadInt16()) * 2;

					// parse repeat length
					curSample.m_repeatLength = TPParserUtilsMOD.ConvertEndianess(binReader.ReadInt16()) * 2;
				}

				return true;
			}
	
			return false;
		}
		
		/// <summary>
		/// The song order is actually the song map of patterns to play.
		///
		///	To parse:
		///	 
		/// Number of bytes			Information
		/// ====================================
		/// 1						Song length - how many patterns are played
		/// 1						Ignored value (Song end jump position)
		/// 128						Patterns order array
		/// 
		/// We then calculate how many actual patterns we have in the file by
		/// iterating over each item in the patterns order and saving the largest
		/// pattern number.  This way, even if we play patterns more than once, we
		/// know how many actual patterns exist.
		/// 
		/// </summary>
		/// <param name="binReader"></param>
		/// <param name="ModuleObject"></param>
		/// <returns></returns>
		private bool ParseOrderInfo(BinaryReader binReader, ref TPModule module) {
			if ((null != binReader) && (null != module)) {
				TPModuleTypeInfo typeInfo = module.m_moduleTypeInfo;

				module.m_patternsInfo = new TPPatternsInfo();

				// parse song length in patterns
				module.m_patternsInfo.m_songLengthInPatterns = (int)binReader.ReadByte();

				// parse end jump position
				module.m_patternsInfo.m_songEndJumpPos = (int)binReader.ReadByte();

				// parse patterns order array
				module.m_patternsInfo.m_orderArray = binReader.ReadBytes(128);

				// calculate the real number of patterns and then allocate them
				int maxValue = 0;

				for (int i = 0; i < 128; i++)	{
					if (maxValue < (int)module.m_patternsInfo.m_orderArray[i]) {
						maxValue = (int)module.m_patternsInfo.m_orderArray[i];
					}
				}

				maxValue++; // add one because patterns is a zero-based array.  
							// So we actually have patten number 0, which means we have 1 pattern.
				
				module.m_patterns = new TPPattern[maxValue];
				for (int i = 0; i < maxValue; i++) {
					module.m_patterns[i] = new TPPattern();
				}

				return true;
			}

			return false;
		}
			
		/// <summary>
		/// Parse the patterns in the module. 
		/// NOTE: we must call this function after ParseOrderInfo because the
		/// ParseOrderInfo function is responsible for allocating the pattern's
		/// array.
		/// </summary>
		/// <param name="binReader"></param>
		/// <param name="ModuleObject"></param>
		/// <returns></returns>
		private bool ParsePatterns(BinaryReader binReader, ref TPModule module) {
			TPModuleTypeInfo typeInfo = module.m_moduleTypeInfo;

			if ((null != binReader) && 
				  (null != module) && 
				  (null != typeInfo))	{
				if (null != module.m_patterns) {
					// because we can no longer count on the IsFormatSupported 
					// method (it only checks up to offset 1084) we need to check
					// that we have valid data to parse.
					// to do this we calculate the amount of bytes we need according
					// to the following algorithm:
					//
					// Number of Patterns * Number of rows * Number of Channels * 4 (4 is constant 4 byte notes)
					long lPatternsBufferSize = (module.m_patterns.Length * 
												(int)typeInfo.m_rowsPerPattern *
												(int)typeInfo.m_channels * 4);

					if (lPatternsBufferSize < binReader.BaseStream.Length - 
													binReader.BaseStream.Position) {
						// parse each pattern
						for (int i = 0; i < module.m_patterns.Length; i++) {
							TPPattern curPattern = module.m_patterns[i];
							
							// first allocate the number of rows and the number of channels
							// for each row.
							int rowCount		= (int)typeInfo.m_rowsPerPattern;
							int channelCount	= (int)typeInfo.m_channels;

							curPattern.m_rows = new TPNote[rowCount, channelCount];

							// start parsing notes
							for (int Row = 0; Row < rowCount; Row++) {
								for (int channel = 0; channel < channelCount; channel++) {
									curPattern.m_rows[Row, channel] = ParseNote(binReader, module); 
								}
							}
						}
					}

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Parses a single note.
		/// 
		/// Each note is represented in four bytes, see the function for more
		/// information about the parsing of a note
		/// </summary>
		/// <param name="binReader"></param>
		/// <param name="ModuleObject"></param>
		/// <returns></returns>
		private TPNote ParseNote(BinaryReader binReader, TPModule ModuleObject) {
			if ((null != binReader) && (null != ModuleObject)) {
				TPModuleTypeInfo typeInfo = ModuleObject.m_moduleTypeInfo;

				TPNote note = new TPNote();

				byte[] noteBuffer = binReader.ReadBytes(4);

				//	BYTE 1   BYTE 2   BYTE 3   BYTE 4
				// aaaaBBBB CCCCCCCC DDDDeeee FFFFFFFF

				// How to calculate the sample number:
				//
				//aaaaBBBB - noteBuffer[0]
				//  AND
				//11110000 - 0xF0
				//========
				//aaaa0000 - we separate aaaa
				//  +/OR
				//0000DDDD - (DDDDeeee >> 4) = 0000DDDD
				//========
				//aaaaDDDD - we connect aaaa with DDDD
				note.m_sampleNumber = Convert.ToByte((short)((noteBuffer[0] & 0xF0) + (noteBuffer[2] >> 4)));

				// How to calculate the note (period):
				//
				// aaaaBBBB
				//	 AND
				// 00001111 - 0x0F
				//=========
				// 0000BBBB
				//	  ||
				//    \/
				// 00000000-0000BBBB - convert to unsigned short
				//	  ||
				//	  \/
				// 0000BBBB-00000000 - shift left 8 (00000000-0000BBBB << 8)
				//	  +
				// CCCCCCCC
				//=========
				// 0000BBBB-CCCCCCCC
				note.m_periodTableIndex = Convert.ToInt16((((ushort)noteBuffer[0] & 0x0F) << 8) + noteBuffer[1]);

				// find the period index in the period_table array:
				note.m_periodTableIndex = TPParserUtilsMOD.GetPeriodTableIndex(note.m_periodTableIndex);

				// get the note and the octave for GUI
				note.m_note = (-1 == note.m_periodTableIndex) ? TPNote.TPNotes.None : (TPNote.TPNotes)(note.m_periodTableIndex % 12);

				note.m_octave = (short)(note.m_periodTableIndex / 12);

		
				// calculate note's frequency and check sample validity
				int sampleNumber = (int)note.m_sampleNumber - 1; // we store it as zero-based, MOD doesn't

				if ((0 <= sampleNumber) && (ModuleObject.m_samples.Length > sampleNumber)) {
					note.m_frequency = TPParserUtilsMOD.GetNoteFrequency(note.m_periodTableIndex, ModuleObject.m_samples[sampleNumber].m_C2SPDFrequencyValue);
				}	else {
					note.m_sampleNumber = 0;
				}

				// How to calculate the effect number:
				//
				// DDDDeeee
				//	 AND
				// 00001111 - 0x0F
				//=========
				// 0000eeee
				note.m_effect = Convert.ToByte(noteBuffer[2] & 0x0F);

				note.m_effectParams = Convert.ToByte(noteBuffer[3]);

				return note;
			}

			return null;
		}

		/// <summary>
		/// Parse each of our sample's data
		/// </summary>
		/// <param name="binReader"></param>
		/// <param name="ModuleObject"></param>
		/// <returns></returns>
		private bool ParseSampleData(BinaryReader binReader, TPModule ModuleObject)	{
			if ((null != binReader) && (null != ModuleObject)) {
				TPModuleTypeInfo typeInfo = ModuleObject.m_moduleTypeInfo;
        for (int i = 0; i < ModuleObject.m_samples.Length; i++)	{
					TPSample curSample = ModuleObject.m_samples[i];

					if (0 < curSample.m_length) {
						// like the ParsePatterns method, we need to check for enough buffer
						// size to parse.
						if (curSample.m_length > binReader.BaseStream.Length - 
													binReader.BaseStream.Position) {
							return false;
						}

						curSample.m_data = binReader.ReadBytes(System.Convert.ToInt32(curSample.m_length));
					}
				}

				return true;
			}

			return false;
		}

		#endregion

	}
}
