using System;
using System.Collections;
using TPlayer.Model;

namespace TPlayer.Engine {
	/// <summary>
	/// Plays MOD files
	/// </summary>
	public class TPPlayer {
		public TPPlayer()	{
			SetGlobalVolume(64);
			m_playing = false;
			m_bufferPlayCount = 0;

			// Create the interpolation processors hash
			m_interpolationProcessorsHash[TPOutputFormat.TPInterpolationMethod.NoInterpolation]		= new InterpolationProcessor(NoInterpolation);
			m_interpolationProcessorsHash[TPOutputFormat.TPInterpolationMethod.LinearInterpolation] = new InterpolationProcessor(LinearInterpolation);
			m_interpolationProcessorsHash[TPOutputFormat.TPInterpolationMethod.CubicInterpolation]	= new InterpolationProcessor(CubicInterpolation);

			// Create the effect processors hash table
			m_effectProcessorsHash[TP_EFFECT_ARPEGGIO]							= new EffectProcessor(EffectProcessorArpeggio);
			m_effectProcessorsHash[TP_EFFECT_PORTAMENTO_UP]						= new EffectProcessor(EffectProcessorPortamento);
			m_effectProcessorsHash[TP_EFFECT_PORTAMENTO_DOWN]					= new EffectProcessor(EffectProcessorPortamento);
			m_effectProcessorsHash[TP_EFFECT_TONE_PORTAMENTO]					= new EffectProcessor(EffectProcessorTonePortamento);
			m_effectProcessorsHash[TP_EFFECT_VIBRATO]							= new EffectProcessor(EffectProcessorVibrato);
			m_effectProcessorsHash[TP_EFFECT_VOLUME_SLIDE]						= new EffectProcessor(EffectProcessorVolumeSlide);
			m_effectProcessorsHash[TP_EFFECT_SET_SPEED]							= new EffectProcessor(EffectProcessorSetSpeed);
			m_effectProcessorsHash[TP_EFFECT_PATTERN_BREAK]						= new EffectProcessor(EffectProcessorPatternBreak);
			m_effectProcessorsHash[TP_EFFECT_SET_VOLUME]						= new EffectProcessor(EffectProcessorSetVolume);
			m_effectProcessorsHash[TP_EFFECT_POSITION_JUMP]						= new EffectProcessor(EffectProcessorPositionJump);
			m_effectProcessorsHash[TP_EFFECT_VOLUME_SLIDE_AND_TONE_PORTAMENTO]	= new EffectProcessor(EffectProcessorVolumeSlideAndTonePortamento);
			m_effectProcessorsHash[TP_EFFECT_VOLUME_SLIDE_AND_VIBRATO]			= new EffectProcessor(EffectProcessorVolumeSlideAndVibrato);
			m_effectProcessorsHash[TP_EFFECT_TREMOLO]							= new EffectProcessor(EffectProcessorTremolo);
			m_effectProcessorsHash[TP_EFFECT_SET_OFFSET]						= new EffectProcessor(EffectProcessorSampleOffset);
		}

		#region Constants

		// Fixed-precision integer for re-sampling positions.
		private const int FP_FRAC_BITS = 12;

		// Effects
		public const int TP_EFFECT_ARPEGGIO							= 0x00;
		public const int TP_EFFECT_PORTAMENTO_UP					= 0x01;
		public const int TP_EFFECT_PORTAMENTO_DOWN					= 0x02;
		public const int TP_EFFECT_TONE_PORTAMENTO					= 0x03;
		public const int TP_EFFECT_VIBRATO							= 0x04;
		public const int TP_EFFECT_VOLUME_SLIDE_AND_TONE_PORTAMENTO	= 0x05;
		public const int TP_EFFECT_VOLUME_SLIDE_AND_VIBRATO			= 0x06;
		public const int TP_EFFECT_TREMOLO							= 0x07;
		public const int TP_EFFECT_SET_PANNING						= 0x08;
		public const int TP_EFFECT_SET_OFFSET						= 0x09;
		public const int TP_EFFECT_VOLUME_SLIDE						= 0x0A;
		public const int TP_EFFECT_POSITION_JUMP					= 0x0B;
		public const int TP_EFFECT_SET_VOLUME						= 0x0C;
		public const int TP_EFFECT_PATTERN_BREAK					= 0x0D;
		public const int TP_EFFECT_SET_SPEED						= 0x0F;

    #endregion

    #region Member variables

    //////////////////////////////////////////////////////////////////////////
    // Editing fields (thread safe)
    // ============================
    // In order to be able to change different fields of the player while 
    // playing, we will have two private members for each field: one will
    // be the private member that the player will use to get retreive information,
    // the other will be used to interact with the public fields (for setting
    // and getting values). Each time the player fires the OnNextBuffer event, 
    // we will safe copy the public members to the private ones - because
    // we know that the player isn't trying to access these values when the 
    // event fires.

		/// <summary>
		/// Sets the module to play
		/// </summary>
		private TPModule m_module = null;
		public TPModule Module {
			get {
				return m_module;
			}
			set {
				if (Playing) {
					throw new TPFieldChangeException("Can't Change This Field While In Playing Mode!");
				}

				m_module = value;
			}
		}

		/// <summary>
		/// Beats per minute
		/// </summary>
		private int m_privBPM	= 125;
		private int m_pubBPM	= 125;
		public int BPM {
			get {
				int retBPM = 0;

				lock(this) {
					retBPM = m_pubBPM;
				}
				
				return retBPM;
			}
			set	{
				if ((value <= 500) && (value > 0)) {
					lock(this) {
						m_pubBPM = value;
					}

					if (!Playing) {
						m_privBPM = m_pubBPM;
					}
				} else {
					throw new ArgumentOutOfRangeException("BPM Must Range Between 1 And 500!");
				}
			}
		}

		/// <summary>
		/// Speed of song
		/// </summary>
		private int m_pubSpeed	= 6;
		private int m_privSpeed = 6;
		public int Speed {
			get {
				int retSpeed = 6;

				lock(this) {
					retSpeed = m_pubSpeed;
				}
				
				return retSpeed;
			}
			set {
				if ((value <= 30) && (value > 0))	{
					lock(this) {
						m_pubSpeed = value;
					}

					if (!Playing) {
						m_privSpeed = m_pubSpeed;
					}
				}	else {
					throw new ArgumentOutOfRangeException("Speed Must Range Between 1 And 500!");
				}
			}
		}

		private int m_curPattern = 0;
		public int CurrentPattern {
			get {
				int retCurPatten = 0;

				lock(this) {
					retCurPatten = m_curPattern;
				}
				
				return retCurPatten;
			}
			set {
				lock(this) {
					m_curPattern = value;
				}
			}
		}
    		
		private int m_curRow	 = 0;
		public int CurrentRow {
			get {
				int retCurRow = 0;

				lock(this) {
					retCurRow = m_curRow;
				}

				return retCurRow;
			}
			set {
				lock(this) {
					m_curRow = value;
				}
			}
		}

		private TPTrack[] m_tracks = null;
		public TPTrack[] Tracks {
			get {
				return m_tracks;
			}
		}

		private TPOutputDeviceDirectSound m_OutputDevice = null;
		public TPOutputDeviceDirectSound OutputDevice	{
			get	{
				return m_OutputDevice;
			}
			set	{
				if (Playing) {
					throw new TPFieldChangeException("Can't Change This Field While In Playing Mode!");
				}

				m_OutputDevice = value;
			}
		}
	
    private bool m_playing = false;
		public bool Playing {
			get	{
				return m_playing;
			}
		}


		private int m_samplesLeftToMix = 0;
		private int m_currentTick = 0;
		private int m_bufferPlayCount = 0;
		private int m_curMixingRow = 0;
		private int m_curMixingPattern = 0;

		private int[] m_preMixedBufferLeft = null;
		private int[] m_preMixedBufferRight = null;
		private byte[] m_outputBuffer = null;


		/// <summary>
		/// Volume look-up table, instead of using arithmetic operators during
		/// the resource-consuming mixing process, we will use a look-up table.
		/// i.e. to mix each note, we multiply the note by the volume, we can save
		/// this by calculating all couples of volume/notes
		/// </summary>
		private int[,] m_volumeTable = null;

		/// <summary>
		/// This is a delegate to an interpolation method, it will be used by the MixTick method
		/// to determine which interpolation we should run
		/// </summary>
		private delegate int InterpolationProcessor(ref byte[] sampleData, int samplePosition, int fixedPercisionPosition, int volume);

		/// <summary>
		/// Hash to interpolation processors.  The key is the enum of the interpolation
		/// processor, and the value is a delegate to the interpolation method.
		/// </summary>
		private Hashtable m_interpolationProcessorsHash = new Hashtable();

    // We store delegates to different effect processing functions in a 
    // hash, who's key will be the effect's identifier

		/// <summary>
		/// A delegate to a effect processor
		/// </summary>
		/// <param name="track"></param>
		/// <param name="isNewRow">true when called from UpdateCurrentRows - so we know we need to initialize the effects or treat as a row effect</param>
		private delegate void EffectProcessor(ref TPTrack track, bool isNewRow);

		/// <summary>
		/// Stores a hash table who's key is an effect number and value
		/// is a delegate to a effect processor function
		/// </summary>
		private Hashtable m_effectProcessorsHash = new Hashtable();

    #endregion

		#region Mixing Methods
		private void SetGlobalVolume(int globalVolume) {
			// Initialize the volume look-up table
			m_volumeTable = null;
			m_volumeTable = new int[65,256];

			for (int i=0; i<65; i++)
				for (int j=0; j<256; j++)
				{
					unchecked
					{
						m_volumeTable[i,j] = globalVolume * i * (int)(SByte)j / 64;
					}
				}
		}

		/// <summary>
		/// This function will clip our pcm data (single sample) according to the output 
    /// format - will also fix signed/unsigned issues (for 8 bit samples)
		/// </summary>
		/// <returns></returns>
		private int PostProcessSample(int sampleValue, short lowerClip, short upperClip, short bitShift, short signedValue) {
			sampleValue = (sampleValue >> bitShift);
			sampleValue += signedValue;

			if (sampleValue < lowerClip) {
				sampleValue = lowerClip;
			} else {
				if (sampleValue > upperClip) {
					sampleValue = upperClip;
				}
			}

			return sampleValue;
		}
		
		/// <summary>
		/// This function does the specific post mixing processing - clipping, stereo, surround etc...
		/// </summary>
		/// <param name="preMixedBuffer"></param>
		/// <returns></returns>
		private void PostMixBuffer(ref byte[] outputBuffer, int[] preMixedBufferLeft, int[] preMixedBufferRight) {
			// Calculate how many bytes we need - accurding to the mixed buffer's size and the bytes per sample
			int blockAlign = m_OutputDevice.OutputFormat.BlockAlign;
			//byte[] playBuffer = new byte[preMixedBufferLeft.Length * blockAlign];
			
			bool is16Bits = (TPOutputFormat.TPBitsPerSample.Bits16 == m_OutputDevice.OutputFormat.BitsPerSample);

			short lowerClip	= is16Bits ? (short)-0x8000 : (short)0x00;
			short upperClip = is16Bits ? (short)0x7FFF : (short)0xFF;
			short bitShift	= is16Bits ? (short)0 : (short)0x08; //0x06 + 0x02 (4 channels) - should 
			short signedVal	= is16Bits ? (short)0 : (short)0x80;

			// In case we are in stereo, we need to know what half a block is, otherwise, we ignore this value
			int stereoSampleSize = m_OutputDevice.OutputFormat.Stereo ? blockAlign / 2 : 0;

			for (int i = 0; i < preMixedBufferLeft.Length; i++)	{	
				int currentSampleDataLeft	= preMixedBufferLeft[i];
				int currentSampleDataRight	= preMixedBufferRight[i];

				if (!m_OutputDevice.OutputFormat.Stereo) {
					int currentSampleData = currentSampleDataLeft + currentSampleDataRight;

					currentSampleData = PostProcessSample(currentSampleData, lowerClip, upperClip, bitShift, signedVal);

					Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToInt16(currentSampleData)), 
                           0, 
											     outputBuffer, 
                           i * blockAlign, 
                           blockAlign);
				} else {
					currentSampleDataLeft	= PostProcessSample(currentSampleDataLeft, 
                                                    lowerClip, 
                                                    upperClip, 
																                    bitShift, 
                                                    signedVal);

					currentSampleDataRight	= PostProcessSample(currentSampleDataRight, 
                                                      lowerClip, 
                                                      upperClip, 
																                      bitShift, 
                                                      signedVal);

					Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToInt16(currentSampleDataLeft)), 
                           0, 
						               outputBuffer, 
                           i * blockAlign, 
                           stereoSampleSize);

					Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToInt16(currentSampleDataRight)), 
                           0, 
						               outputBuffer, 
                           (i * blockAlign) + stereoSampleSize, 
                           stereoSampleSize);
				}
			}
		}

		/// <summary>
		/// Calculate how many samples we have for each tick.
		/// This will allow us to calculate the most accurate time per tick
		/// (depending on our sound hardware's timer).
		/// Instead of using the OS timer (which in windows is 2 ms precision)
		/// </summary>
		/// <returns></returns>
		private int CalculateSamplesPerTick() {
			// First calculate how many ticks per second:
			// HZ = 2*bpm/5
			int ticksPerSecond = 2 * m_privBPM / 5;

			// Next, we calculate how many samples we have per tick
			//
			// if, for example, bpm is 125 (which is the default value), then
			// it means we have 50 ticks per second ( 2 * 125 / 5 = 50)
			//
			// Ticks		Second
			//  50    ==>      1
			//
			// if our output sample rate is 44100 Hz (CD quality) it means we
			// our output-ing 44100 samples per second
			//
			// Samples		Second
			//  44100  ==>     1
			//
			// if we substitute seconds with tick, we can calculate samples per tick
			// this, of course, according to the:
			//					A = B
			//					B = C
			//					-----
			//					A = c
			// rule :)
			// So:
			//
			//	Ticks		Samples
			//	  50   ==>   44100
			//     1   ==>     X
			//
			// Cross multiply:
			// X = (1 * 44100) / 50 = 44100 / 50 = 882 
			int samplesPerTick = m_OutputDevice.OutputFormat.SampleRate / ticksPerSecond;

			return samplesPerTick;
		}

		/// <summary>
		/// Takes care of resampling - that is, for example, taking our 8,363 Hz samples 
    /// and re-sampling them into a 44100 Hz buffer.
		/// 
		/// mixBuffer[index] = note[i].volume * note[i].data[pos];
		/// </summary>
		private int MixTick(ref int[] mixBufferLeft, ref int[] mixBufferRight, int offset, int mixSize) {
			// in order to keep the output buffer in valid bounds, we will
			// divide each sample we mix by the number of tracks mixed.
			// in order to do this, we will calculate the exact number of 
			// tracks we actually had data to mix
			int mixableTracks = 0;

			// get interpolation method
			InterpolationProcessor interpolationProcessor = (InterpolationProcessor)m_interpolationProcessorsHash[m_OutputDevice.OutputFormat.InterpolationMethod];

			for (int i = 0; i < m_tracks.Length; i++)	{
				// check sample validity
				int sampleNumber = m_tracks[i].Sample;

				if ((0 <= sampleNumber) && 
					  (sampleNumber < m_module.m_samples.Length) &&
					  (m_tracks[i].Mute != true)) {
					TPSample curSample = m_module.m_samples[sampleNumber];

					// check we have data
					if (0 < curSample.m_length)	{
						int curSamplePosition = (m_tracks[i].CurrentPosition >> FP_FRAC_BITS);

						// if the repeat length is bigger than 2 bytes we know this is a loopable sample
						// dunno why 2 and not 0...
						bool isLoopSample = (curSample.m_repeatLength > 2);

						// calculate the end of the loop
						int repeatEndOffset = (int)(curSample.m_repeatPos + curSample.m_repeatLength);

						// get the correct buffer - for stereo and surround (see post processing)
						// LEFT RIGHT RIGHT LEFT RIGHT RIGHT LEFT ....
						int[] mixBuffer = (((i & 3) == 0) || ((i & 3) == 3)) ? mixBufferLeft : mixBufferRight;
							
						// calculate re-sampling scale
						int reSampleScale = (int)(m_tracks[i].Frequency * (1 << FP_FRAC_BITS) / m_OutputDevice.OutputFormat.SampleRate);

						// get the volume
						int volume = m_tracks[i].Volume;

						// mixed buffer index
						int mixBufPos = offset;

						// check if we have valid data to mix in this track
						if ((curSamplePosition < curSample.m_length) &&
							  ((mixBufPos - offset) < mixSize)) {
							mixableTracks++;
						}

						// start mixing, stop when either:
						// 1. we reach the end of the sample
						// 2. we reach the end of the mixed buffer - use mix size for this
						while ((curSamplePosition < curSample.m_length) &&
							   ((mixBufPos - offset) < mixSize)) {							
							mixBuffer[mixBufPos++] += interpolationProcessor(ref curSample.m_data, curSamplePosition, m_tracks[i].CurrentPosition, volume);
								
							m_tracks[i].CurrentPosition += reSampleScale;
							curSamplePosition = (m_tracks[i].CurrentPosition >> FP_FRAC_BITS);

							// check for loops
							if (isLoopSample)	{
								if (curSamplePosition >= repeatEndOffset)	{
									// reset the track position back to the repeat position
									curSamplePosition = (int)curSample.m_repeatPos;
									m_tracks[i].CurrentPosition = ((int)curSample.m_repeatPos << FP_FRAC_BITS);
								}
							}
						}
					}
				}
			}

			return mixableTracks;
		}
		
		/// INTERPOLATION
		/// <summary>
		/// The simplest form of interpolation:
		/// Each loop, the samplePosition is calculated by rounding to the integer part of the
		/// fixed integer, and then, the unaligned blocks of the resampled data are filled with
		/// the original blocks of the sample.  No calculations are done to the original data.
		/// </summary>
		int NoInterpolation(ref byte[] sampleData, int samplePosition, int fixedPercisionPosition, int volume) {
			int curSampleData = (int)sampleData[samplePosition];
			return (m_volumeTable[volume, curSampleData]);
		}

		/// <summary>
		/// This form of interpolation calculates the unaligned blocks with respect to the nearest
		/// integer.
		/// The linear interpolation usually sounds good when upscaling by a large difference.
		/// </summary>
		int LinearInterpolation(ref byte[] sampleData, int samplePosition, int fixedPercisionPosition, int volume) {
			// get the first sample
			int firstSampleData = (int)sampleData[samplePosition];
			int secondSampleData = firstSampleData;

			// if we can, get the next sample
			if ((samplePosition + 1) < sampleData.Length)
				secondSampleData = (int)sampleData[samplePosition + 1];
							
			// calculate our fraction value.  For example: .25
			int fraction = fixedPercisionPosition & ((1 << FP_FRAC_BITS) - 1);

			// calculate the left over: 1 - fraction.  For example: 1 - .25 = .75
			int leftOver = (1 << FP_FRAC_BITS) - fraction;

			// we multiply the leftOver value by the sample value and add to it the fraction multiplied 
			// by the second sample data so we get an average
			return (int)(((leftOver*(m_volumeTable[volume, firstSampleData])) + 
				  		 (fraction*(m_volumeTable[volume, secondSampleData]))) >> FP_FRAC_BITS);
		}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sampleData"></param>
    /// <param name="samplePosition"></param>
    /// <param name="fixedPercisionPosition"></param>
    /// <param name="volume"></param>
    /// <returns></returns>
		int CubicInterpolation(ref byte[] sampleData, int samplePosition, int fixedPercisionPosition, int volume) {
			int firstSampleData  = (int)sampleData[samplePosition];
			int secondSampleData = firstSampleData;
			int thirdSampleData	 = firstSampleData;
			int forthSampleData  = firstSampleData;

			// if we can, get the next sample
			if ((samplePosition + 1) < sampleData.Length)
				secondSampleData = (int)sampleData[samplePosition + 1];

			// if we can, get the next sample
			if ((samplePosition + 2) < sampleData.Length)
				thirdSampleData = (int)sampleData[samplePosition + 2];

			// if we can, get the next sample
			if ((samplePosition + 3) < sampleData.Length)
				forthSampleData = (int)sampleData[samplePosition + 3];

			firstSampleData = m_volumeTable[volume, firstSampleData];
			secondSampleData = m_volumeTable[volume, secondSampleData];
			thirdSampleData = m_volumeTable[volume, thirdSampleData];
			forthSampleData = m_volumeTable[volume, forthSampleData];

			// calculate our fraction value.  For example: .25
			int fraction = fixedPercisionPosition & ((1 << FP_FRAC_BITS) - 1);
			int cubicFraction = (fraction*fraction) >> FP_FRAC_BITS;
			int tripleFraction = (cubicFraction * fraction) >> FP_FRAC_BITS;

			int a0,a1,a2,a3;
			
			a0 = forthSampleData - thirdSampleData - firstSampleData + secondSampleData;
			a1 = firstSampleData - secondSampleData - a0;
			a2 = thirdSampleData - firstSampleData;
			a3 = secondSampleData;

			return(((a0*tripleFraction) >> FP_FRAC_BITS) + 
				     ((a1*cubicFraction) >> FP_FRAC_BITS) +
				     ((a2*fraction) >> FP_FRAC_BITS) + 
               a3);
		}

    #endregion

		#region Effects Processors
		
		/// <summary>
		/// Arpeggio - If a note has an effect number of 0x00, it is only an arpeggio 
		/// if there is at least one non-zero argument.  When there is at least one  
		/// valid argument, this effect means to play the note specified, then 
		/// the note+xxxx half-steps, then the note+yyyy half-steps, and then 
		/// return to the original note.  These changes are evenly spaced within the 
		/// time for a line to be played at the current speed.
		/// </summary>
		/// <param name="track"></param>
		private void EffectProcessorArpeggio(ref TPTrack track, bool isNewRow) {
			if (isNewRow)
				return;

			int xParam = track.EffectParam >> 4;
			int yParam = track.EffectParam & 0x0F;

			if ((0 < xParam) || (0 < yParam)) {
				int newNodePeriodTableIndex = track.PeriodIndex;
							
				switch(m_currentTick % 3) {
					case 1:
						newNodePeriodTableIndex = track.PeriodIndex + xParam;
						break;
					case 2:
						newNodePeriodTableIndex = track.PeriodIndex + yParam;
						break;
					case 0:
					default:
						newNodePeriodTableIndex = track.PeriodIndex;
						break;
				}

				int sampleNumber = track.Sample > -1 ? track.Sample : 0;

				track.Frequency = TPParserUtilsMOD.GetNoteFrequency((short)newNodePeriodTableIndex,
					m_module.m_samples[sampleNumber].m_C2SPDFrequencyValue);
			}
		}

		/// <summary>
		/// Portamento Up - This effect will slide up  the frequency  (decrease 
		/// the period) of  the sample being played on the channel by xxxxyyyy notes 
		/// for every  tick that occurs during the line.
		///
		/// Portamento Down - This effect will slide  down the frequency 
		/// (increase  the period) of  the sample being played on the channel 
		/// by xxxxyyyy tones for every  tick that occurs during the line.
		/// </summary>
		/// <param name="track"></param>
		private void EffectProcessorPortamento(ref TPTrack track, bool isNewRow) {
			if (isNewRow)
				return;

			// Why do we multiply by 4?
			// Because we are using an S3M period value table and because S3M uses
			// "4*bigger period value than MOD does" - 
			// "The 10 steps in converting a MOD player to an S3M player"
			int portaDelta = track.EffectParam * 4;
						
			int newPeriodValue = (TP_EFFECT_PORTAMENTO_UP == track.Effect) ? 
				track.PeriodValue - portaDelta : track.PeriodValue + portaDelta;

			if ((newPeriodValue < TPParserUtilsMOD.S3M_PERIOD_TABLE[0]) &&
				  (newPeriodValue > TPParserUtilsMOD.S3M_PERIOD_TABLE[TPParserUtilsMOD.S3M_PERIOD_TABLE.Length - 1])) {
				track.PeriodValue = newPeriodValue;

				int sampleNumber = track.Sample > -1 ? track.Sample : 0;

				track.Frequency = TPParserUtilsMOD.GetNoteFrequencyByPeriod(track.PeriodValue,
					m_module.m_samples[sampleNumber].m_C2SPDFrequencyValue);
			}
		}

		/// <summary>
		/// Tone Portamento - This effect causes the pitch to slide towards the 
		/// note specified.  If there is no note specified it slides towards the 
		/// last note specified in the Porta to Note effect.  If there is no 
		/// parameter then the last porta speed used for that channel is used again.
		/// </summary>
		/// <param name="track"></param>
		private void EffectProcessorTonePortamento(ref TPTrack track, bool isNewRow) {
			if (isNewRow)
				return;

			if ((0 == track.PortaPeriod) || (0 == track.PortaSpeed))
				return;

			if (track.PeriodValue < track.PortaPeriod) {
				// add to period value
				if ((track.PeriodValue += track.PortaSpeed) > track.PortaPeriod) {
					track.PeriodValue = track.PortaPeriod;
				}
			} else if (track.PeriodValue > track.PortaPeriod) {
				// subtract from period value
				if ((track.PeriodValue -= track.PortaSpeed) < track.PortaPeriod) {
					track.PeriodValue = track.PortaPeriod;
				}
			}	else {
				return;
			}

			int sampleNumber = track.Sample > -1 ? track.Sample : 0;

			track.Frequency = TPParserUtilsMOD.GetNoteFrequencyByPeriod(track.PeriodValue,
				m_module.m_samples[sampleNumber].m_C2SPDFrequencyValue);
		}
		
		/// <summary>
		/// Vibrato - This effect causes the pitch to waver up and down around the 
		/// base note.  If no parameter, use the last vibrato parameters used for 
		/// that channel.
		/// Although FMODDOC2 states that we need to change the frequency with the
		/// calculated vibrato delta - it seems like we actually use the delta on 
		/// the amiga period value and not frequency - which is also easier to
		/// implement and more efficient - since we don't have to recalculate the
		/// frequency back to its original value each tick.
		/// </summary>
		/// <param name="track"></param>
		private void EffectProcessorVibrato(ref TPTrack track, bool isNewRow) {
			if (isNewRow) {
				int xParamSpeed = track.EffectParam >> 4;
				int yParamDepth = track.EffectParam & 0x0F;

				if (xParamSpeed > 0) {
					track.VibratoSpeed = xParamSpeed;
				}

				if (yParamDepth > 0) {
					track.VibratoDepth = yParamDepth;
				}

				// TODO: check waveform parameter
				//track.VibratoPos = 0;

				return;
			}
            
			if ((0 < track.VibratoSpeed) && (0 < track.VibratoDepth))	{
				// first, calculate the vibrato's table index currently used - 
				// because we only store the positive values of the sine table
				// we need to test the sign value of SineEffectPos to know if we
				// are at a negative position of the table, so we keep a signed
				// value in the SineEffectPos and use the absolute value of it to 
				// get the right table index.
				// NOTE: we control the SineEffectPos internally, so there is no
				// need for error checking when getting the table value with the
				// indexer.
				int vibratoTableIndex = Math.Abs(track.VibratoPos);

				// calculate the vibrato table according to table index and then
				// vibrato depth
				int vibratoDeltaVal = TPParserUtilsMOD.MOD_EFFECTS_SINE_TABLE[vibratoTableIndex];

				vibratoDeltaVal *= track.VibratoDepth;

				// we divide by 128 (which is equivalent to right shifting 7 bits)
				// because we don't want a huge vibrato - and because that is what
				// the MOD spec tells us to do
				vibratoDeltaVal >>= 7;

				vibratoDeltaVal *= 4; // we use s3m period values

				// now calculate new period value
				int newPeriodValue = (track.VibratoPos < 0) ? 
										track.PeriodValue - vibratoDeltaVal : track.PeriodValue + vibratoDeltaVal;

				int sampleNumber = track.Sample > -1 ? track.Sample : 0;

				// change frequency
				track.Frequency = TPParserUtilsMOD.GetNoteFrequencyByPeriod(newPeriodValue,
					m_module.m_samples[sampleNumber].m_C2SPDFrequencyValue);

				// update the position (using speed parameter)
				track.VibratoPos += track.VibratoSpeed;

				// check bounds of vibrato pos (according to the VIBRATO_SIN_TABLE)
				if (track.VibratoPos > 31) {
					track.VibratoPos -= 64;
				}				
			}
		}

		/// <summary>
		/// Volume Slide - This effect causes the volume of the track to slide up 
		/// or down.
		/// The x param is for the volume units to slide up and the y param is for
		/// volume units to slide down - if we have both up and down, we don't do
		/// anything.
		/// </summary>
		/// <param name="track"></param>
		private void EffectProcessorVolumeSlide(ref TPTrack track, bool isNewRow)	{
			if (isNewRow)	{
				// TODO: consider checking if both x and y are 0 - maybe change volume delta to zero
				int xParamVolUp = track.EffectParam >> 4;
				int yParamVolDown = track.EffectParam & 0x0F;

				if ((0 < xParamVolUp) && (0 < yParamVolDown)) {
					track.VolumeSlideDelta = 0;
				}	else if (0 < xParamVolUp)	{
					track.VolumeSlideDelta = xParamVolUp;
				}	else if (0 < yParamVolDown)	{
					track.VolumeSlideDelta = yParamVolDown * -1; // we want to subtract volume
				}
		
				return;
			}

			track.Volume += track.VolumeSlideDelta;

			if (64 < track.Volume)
				track.Volume = 64;
			if (0 > track.Volume)
				track.Volume = 0;
		}

		//////////////////////////////////////////////////////////////////////////
		// Volume Slide + Tone Portamento - This is a combination of Porta to Note 
		// (3xy), and volume slide (Axy).  The parameter does not affect the porta, 
		// only the volume.  If no parameter use the last porta to note parameter 
		// used for that channel.
		private void EffectProcessorVolumeSlideAndTonePortamento(ref TPTrack track, bool isNewRow) {
			if (isNewRow) {
        EffectProcessorVolumeSlide(ref track, true);
				return;
			}

			EffectProcessorTonePortamento(ref track, false);
			EffectProcessorVolumeSlide(ref track, false);
		}

    //////////////////////////////////////////////////////////////////////////
		// Volume Slide + Vibrato - This is a combination of Vibrato (4xy), and 
		// volume slide (Axy).  The parameter does not affect the vibrato, only 
		// the volume.  If no parameter use the vibrato parameters used for that 
		// channel.
		private void EffectProcessorVolumeSlideAndVibrato(ref TPTrack track, bool isNewRow)	{
			if (isNewRow) {
				EffectProcessorVolumeSlide(ref track, true);
				return;
			}

			EffectProcessorVibrato(ref track, false);
			EffectProcessorVolumeSlide(ref track, false);
		}

    /// <summary>
		/// This effect causes the volume to oscillate up and down in a fluctuating style	
		/// around the current volume, like vibrato but affecting volume not pitch.
		/// If no parameter use the last tremolo parameter used for that channel.
		/// </summary>
		/// <param name="track"></param>
		/// <param name="isNewRow"></param>
		private void EffectProcessorTremolo(ref TPTrack track, bool isNewRow)	{
			if (isNewRow)	{
				int xParamSpeed = track.EffectParam >> 4;
				int yParamDepth = track.EffectParam & 0x0F;

				if (xParamSpeed > 0) {
					track.TremoloSpeed = xParamSpeed;
				}

				if (yParamDepth > 0) {
					track.TremoloDepth = yParamDepth;
				}

				// TODO: check waveform parameter
				//track.VibratoPos = 0;

				return;
			}
            
			if ((0 < track.TremoloSpeed) && (0 < track.TremoloDepth))	{
				int tremoloTableIndex = Math.Abs(track.TremoloPos);

				// calculate the tremolo table according to table index and then
				// tremolo depth
				int tremoloDeltaVal = TPParserUtilsMOD.MOD_EFFECTS_SINE_TABLE[tremoloTableIndex];

				tremoloDeltaVal *= track.TremoloDepth;

				// we divide by 64 (which is equivalent to right shifting 6 bits)
				// because we don't want a huge tremolo - and because that is what
				// the MOD spec tells us to do
				tremoloDeltaVal /= 64;

				// now calculate new period value
				int newVolumeValue = (track.TremoloPos < 0) ? 
					track.Volume - tremoloDeltaVal : track.Volume + tremoloDeltaVal;

				if (newVolumeValue < 0) {
					newVolumeValue = 0;
				} else if (newVolumeValue > 64)	{
					newVolumeValue = track.Volume + (64 - track.Volume);
				}

				track.Volume = newVolumeValue;

				// update the position (using speed parameter)
				track.TremoloPos += track.TremoloSpeed;

				// check bounds of vibrato pos (according to the VIBRATO_SIN_TABLE)
				if (track.TremoloPos > 31) {
					track.TremoloPos -= 64;
				}				
			}
		}

		/// <summary>
		/// This effect causes the note to start playing at an offset into the sample,
		/// instead of just from the start.  It is used so that the beginning of a sample
		/// can be skipped.  If a parameter is not given, then the last parameter
		/// given is used.
		/// 
		/// Two things that aren't obvious from FMODDOC2.TXT:
		/// 1) We only set the offset effect if we have a note, that is:
		///			00	D-5 03	942 - play note from offset 0x4200
		///			01			942 - do nothing
		///			02	D-5 03	942 - play note from offset 0x4200
		///	2) If a parameter is not given, then we don't run this effect - and
		///	   we don't use the last parameter - funny, even FMOD itself works
		///	   this way (??!)
		/// </summary>
		/// <param name="track"></param>
		/// <param name="isNewRow"></param>
		private void EffectProcessorSampleOffset(ref TPTrack track, bool isNewRow) {
			// this is a row effect only
			if (isNewRow) {
				if (track.HasNewNote)	{
					// we have to multiply the effect param by 0x100 which is equal to << 8 (shift left)
					int offset = track.EffectParam << 8;

					// don't forget we are working with floating point precision
					track.CurrentPosition = offset << FP_FRAC_BITS;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		// Position Jump
		// Jump to a specific pattern (if valid)
		private void EffectProcessorPositionJump(ref TPTrack track, bool isNewRow) {
			// this is a row effect only
			if (isNewRow) {
				int newPattern = track.EffectParam;
								
				// check validity of new pattern jump
				if ((newPattern >= 0) && 
					(newPattern < m_module.m_patternsInfo.m_songLengthInPatterns))
				{
					m_curPattern = newPattern;
					m_curRow	 = 0;
				}
			}
		}
		
		//////////////////////////////////////////////////////////////////////////
		// Set volume
		// Set the current track's volume
		private void EffectProcessorSetVolume(ref TPTrack track, bool isNewRow)	{
			// this is a row effect only
			if (isNewRow) {
				int newVolume = track.EffectParam;

				if (newVolume < 0) 
					newVolume = 0;
				if (newVolume > 64)
					newVolume = 64;

				track.Volume = newVolume;
			}
		}
		
		//////////////////////////////////////////////////////////////////////////
		// Pattern Break - jump to next pattern (to a certain row)
		// The value is stored as a hex number but is to be read as if it 
		// were a decimal number.  This means that if we have D22 - it
		// we don't jump to line 0x22 (34) but actually jump to line 22.
		// To calculate this we separate the effect pattern to two nibbles
		// X and Y then do X * 10 + Y to get our decimal value
		private void EffectProcessorPatternBreak(ref TPTrack track, bool isNewRow) {
			// this is a row effect only
			if (isNewRow)	{
				int xParam = track.EffectParam >> 4;
				int yParam = track.EffectParam & 0x0F;

				int newRow = xParam * 10 + yParam;

				if (newRow < 0)
					newRow = 0;
				if (newRow > 63)
					newRow = 63;

				m_curRow = newRow;
				m_curPattern++;
			}
		}

		/// <summary>
		/// Set Speed - This effect sets the speed of the song or the BPM.
		/// Fxx : speed (0x00-0x1F) / tempo (0x20-0xFF)
		/// </summary>
		/// <param name="track"></param>
		/// <param name="isNewRow"></param>
		private void EffectProcessorSetSpeed(ref TPTrack track, bool isNewRow) {
			// this is a row effect only
			if (isNewRow) {
				if (track.EffectParam < 0x20) {
					m_pubSpeed = m_privSpeed = track.EffectParam;
				} else {
					m_pubBPM = m_privBPM = track.EffectParam;
				}
			}
		}

    #endregion

		#region Private Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="track"></param>
		/// <param name="isNewRow"></param>
		private void ProcessEffect(ref TPTrack track, bool isNewRow) {
			if (m_effectProcessorsHash.Contains(track.Effect)) {
				EffectProcessor processor = (EffectProcessor)m_effectProcessorsHash[track.Effect];
				processor(ref track, isNewRow);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		private void UpdateCurrentEffects()	{
			for (int i = 0; i < m_tracks.Length; i++)	{
				ProcessEffect(ref m_tracks[i], false);
			}
		}


		/// <summary>
		/// Fills the tracks with information about the new row - notes and positions
		/// Then updates specific effects - the ones that are to be processed at the 
		/// beginning of the new tick.
		/// </summary>
		private void UpdateCurrentRow()	{
			// We initialized m_tracks in the Play() method, so we can count
			// on the length of the m_tracks array and not have to check
			// module's type information
			for (int i = 0; i < m_tracks.Length; i++)	{
				// Each track is a channel

				// Get the current pattern - as opposed to samples, the MOD format
				// saves the pattern's order as zero-based - so we don't have to 
				// subtract 1.
				int curPattern = (int)m_module.m_patternsInfo.m_orderArray[m_curMixingPattern];

				// get the current note
				TPNote curNote = m_module.m_patterns[curPattern].m_rows[m_curMixingRow, i];

				m_tracks[i].Effect		= curNote.m_effect;
				m_tracks[i].EffectParam	= curNote.m_effectParams;

				m_tracks[i].HasNewNote	= false;

				// if we have a Tone Portamento effect we aren't supposed to change notes (periods) or change
				// the current frequency because the effect is continuous
				if ((TP_EFFECT_TONE_PORTAMENTO != curNote.m_effect) && 
					  (TP_EFFECT_VOLUME_SLIDE_AND_TONE_PORTAMENTO != curNote.m_effect))	{
					// do we have a new note?
					if (-1 < curNote.m_periodTableIndex) {
						m_tracks[i].HasNewNote = true;

						// check we have a valid sample
						if (0 < curNote.m_sampleNumber)	{
							m_tracks[i].Sample		= curNote.m_sampleNumber - 1;
							m_tracks[i].Volume		= m_module.m_samples[curNote.m_sampleNumber - 1].m_volume;
						}

						m_tracks[i].PeriodIndex	= curNote.m_periodTableIndex;
						m_tracks[i].PeriodValue	= TPParserUtilsMOD.GetAmigaPeriodValue(curNote.m_periodTableIndex);
									
						m_tracks[i].CurrentPosition		= 0;
						//m_tracks[i].SineEffectPos = 0;
					}

					if ((-1 < m_tracks[i].Sample) && (-1 < m_tracks[i].PeriodIndex)) {
						m_tracks[i].Frequency	= TPParserUtilsMOD.GetNoteFrequencyByPeriod(m_tracks[i].PeriodValue, m_module.m_samples[m_tracks[i].Sample].m_C2SPDFrequencyValue);
					}

					// init effects
					m_tracks[i].PortaPeriod	= 0;
					m_tracks[i].PortaSpeed	= 0;

					m_tracks[i].VolumeSlideDelta = 0;
				} else {
					// check we have a valid sample
					if (0 < curNote.m_sampleNumber)	{
						m_tracks[i].Sample		= curNote.m_sampleNumber - 1;
						m_tracks[i].Volume		= m_module.m_samples[curNote.m_sampleNumber - 1].m_volume;
					}

					if (-1 < curNote.m_periodTableIndex) {
						m_tracks[i].PortaPeriod	= TPParserUtilsMOD.GetAmigaPeriodValue(curNote.m_periodTableIndex);
					}

					if ((0 < curNote.m_effectParams) && 
  						(TP_EFFECT_TONE_PORTAMENTO == curNote.m_effect)) {
						// if we have Volume slide and Portamento, the parameter only effects
						// the volume and not the porta
						m_tracks[i].PortaSpeed = (int)curNote.m_effectParams * 4;
					}
				}

				ProcessEffect(ref m_tracks[i], true);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="playBuffer"></param>
		/// <param name="samplesPerChunk"></param>
		/// <returns></returns>
		private bool ProcessNextChunk(ref byte[] outputBuffer, int samplesPerChunk)	{
			for (int i = 0; i < m_preMixedBufferLeft.Length; i++)
				m_preMixedBufferLeft[i] = m_preMixedBufferRight[i] = 0;

			int preMixedBufferPos		= 0;
			int samplesToMix			= samplesPerChunk;

			while (0 < samplesToMix) {
				if (0 == m_samplesLeftToMix) {
					// because we might change the speed as we play - we will test for <= and not just ==
					if (m_privSpeed <= m_currentTick) {
						m_currentTick = 0;

						// we set m_curMixingRow so that even if we have an effect that changes the current
						// row or pattern, we can still finish up this row with the correct row/pattern
						m_curMixingRow		= m_curRow;
						m_curMixingPattern	= m_curPattern;

						// we want to update the next row before we process effects so that if we have an effect
						// that wants to skip a row, we won't change the offset after the effect.
						m_curRow++;

						UpdateCurrentRow();

						if (m_curRow >= (m_module.m_moduleTypeInfo.m_rowsPerPattern))	{
							m_curRow = 0;
							m_curPattern++;
						
							if (m_curPattern >= m_module.m_patternsInfo.m_songLengthInPatterns)	{
								break;
							}
						}
					}	else {
						UpdateCurrentEffects();
					}

					m_currentTick++;
				}

				int samplesPerTick			= CalculateSamplesPerTick();

				// check what we can mix
				int samplesPerTickToMixCount = m_samplesLeftToMix > 0 ? m_samplesLeftToMix : samplesPerTick;

				bool bHasLeftOvers = false;
				if (samplesToMix < samplesPerTickToMixCount) {
					bHasLeftOvers = true;
					m_samplesLeftToMix = samplesPerTickToMixCount - samplesToMix;
					samplesPerTickToMixCount = samplesToMix;
				}

				// start mixing
				MixTick(ref m_preMixedBufferLeft, ref m_preMixedBufferRight, preMixedBufferPos, samplesPerTickToMixCount);

				preMixedBufferPos += samplesPerTickToMixCount;
				samplesToMix -= samplesPerTickToMixCount;

				// we only want to zero out samplesLeftToMix when we actually mixed
				// the left over samples, not beforehand
				if ((m_samplesLeftToMix > 0) && (!bHasLeftOvers)) {
					m_samplesLeftToMix -= samplesPerTickToMixCount;
				}
			}
	
			PostMixBuffer(ref outputBuffer, m_preMixedBufferLeft, m_preMixedBufferRight);

			return true;
		}
		
		private void OutputDevice_OnNextBufferHandler(object source, EventArgs e) {
			//byte[] outputBuffer = null;
			m_bufferPlayCount++;

			// set members
			lock(this) {
				m_privSpeed = m_pubSpeed;
				m_privBPM	= m_pubBPM;
			}

			if (m_curPattern < m_module.m_patternsInfo.m_songLengthInPatterns) {
				ProcessNextChunk(ref m_outputBuffer, m_OutputDevice.SamplesPerMilliseconds);

				m_OutputDevice.Write(m_outputBuffer);		

				if (m_bufferPlayCount == m_OutputDevice.NumberOfBuffers)
					m_bufferPlayCount = 0;

				if (null != OnUpdateHandler) {
					OnUpdateHandler((object)this, new EventArgs());
				}
			} else if (m_bufferPlayCount == m_OutputDevice.NumberOfBuffers)	{
				m_curPattern = m_module.m_patternsInfo.m_songLengthInPatterns-1;
				
				if (null != OnUpdateHandler) {
					OnUpdateHandler((object)this, new EventArgs());
				}

				Stop();
			}
		}

		#endregion

		#region Public Methods

		public delegate void UpdateEventHandler(object source, EventArgs e);
		public event UpdateEventHandler OnUpdateHandler = null;

		public bool Play() {
			// check that we aren't already playing the song
			if (m_playing)
				return false;

			// check that we have a valid module
			if (null == m_module)
				return false;

			if (null == m_OutputDevice)
				return false;

			// initialize members
			m_samplesLeftToMix	= 0;
			m_currentTick		= m_privSpeed;
			m_bufferPlayCount	= 0;

			m_preMixedBufferLeft	= new int[m_OutputDevice.SamplesPerMilliseconds];
			m_preMixedBufferRight	= new int[m_OutputDevice.SamplesPerMilliseconds];

			// initialize tracks
			m_tracks = null;
			m_tracks = new TPTrack[m_module.m_moduleTypeInfo.m_channels];
			for (int i = 0; i < m_tracks.Length; i++) {
				m_tracks[i] = new TPTrack();
			}

			m_outputBuffer = new byte[m_OutputDevice.SamplesPerMilliseconds * m_OutputDevice.OutputFormat.BlockAlign];

			m_OutputDevice.OnNextBufferHandler += 
					new TPOutputDeviceDirectSound.NextBufferEventHandler(OutputDevice_OnNextBufferHandler);

			for (int i = 0; i < m_OutputDevice.NumberOfBuffers; i++) {
				ProcessNextChunk(ref m_outputBuffer, m_OutputDevice.SamplesPerMilliseconds);
				m_OutputDevice.Write(m_outputBuffer);
			}

			m_OutputDevice.Play();
			m_playing = true;

			return true;
		}

		/// <summary>
		/// Stop playing
		/// </summary>
		/// <returns></returns>
		public bool Stop() {
			m_OutputDevice.Stop();
			m_playing = false;
			return true;
		}
		
    #endregion
	}

}