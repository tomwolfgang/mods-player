using System;

namespace TPlayer.Engine {
	/// <summary>
	/// 
	/// </summary>
	public class TPParserUtilsMOD	{
		#region Public static members
		
		public struct FinetuneC2PSDItem	{
			public FinetuneC2PSDItem(short Finetune, short Frequency) 
			{
				this.FinetuneNumber			= Finetune;
				this.C2PSDFrequencyValue	= Frequency;
			}

			private short FinetuneNumber; //-8..7
			private short C2PSDFrequencyValue;

			public short Finetune	{
				get {
					return FinetuneNumber;
				}
			}

			public short C2PSDFrequency	{
				get	{
					return C2PSDFrequencyValue;
				}
			}
		}
		
		/// <summary>
		/// A conversion table from a sample's finetune to it's C2PSD value
		/// </summary>
		public static readonly FinetuneC2PSDItem[] MOD_FINE_TUNE_TO_FREQUENCY_CONVERSION_TABLE = new FinetuneC2PSDItem[] {
			new FinetuneC2PSDItem(0, 8363),
			new FinetuneC2PSDItem(1, 8413),
			new FinetuneC2PSDItem(2, 8463),
			new FinetuneC2PSDItem(3, 8529),
			new FinetuneC2PSDItem(4, 8581),
			new FinetuneC2PSDItem(5, 8651),
			new FinetuneC2PSDItem(6, 8723),
			new FinetuneC2PSDItem(7, 8757),
			new FinetuneC2PSDItem(-8, 7895),
			new FinetuneC2PSDItem(-7, 7941),
			new FinetuneC2PSDItem(-6, 7985),
			new FinetuneC2PSDItem(-5, 8046),
			new FinetuneC2PSDItem(-4, 8107),
			new FinetuneC2PSDItem(-3, 8169),
			new FinetuneC2PSDItem(-2, 8232),
			new FinetuneC2PSDItem(-1, 8280)
		};

		/// <summary>
		/// s3m period table - the table was taken from fs3mdoc.txt:
		/// "When loading in the periods from the file when reading in MOD pattern data,
		/// there is no need..."
		///
		/// we basically search from octave 0 (i.e. C-0 - 6848) but then we will subtract
		/// 24 (i.e. s3m octave 0) so that we can calculate the correct frequency and calculate																																			   
		/// the correct octave (round down(noteindex / 12)) to show the note (noteindex % 12)
		/// </summary>
		public static readonly short C4Index	= 48;

		public static readonly ushort[] S3M_PERIOD_TABLE = new ushort[]	{
			// octave
			// ======
			//	  C      C#     D     D#      E      F    F#     G    G#    A     A#    B
				27392, 25856, 24384, 23040, 21696, 20480,19328,18240,17216,16256,15360,14496,
				13696, 12928, 12192, 11520, 10848, 10240, 9664, 9120, 8606, 8128, 7680, 7248,

				6848,  6464,  6096,  5760,  5424,  5120, 4832, 4560, 4306, 4064, 3840, 3624, // 0
				3424,  3232,  3048,  2880,  2712,  2560, 2416, 2280, 2152, 2032, 1920, 1812, // 1
				1712,  1616,  1525,  1440,  1357,  1281, 1209, 1141, 1077, 1017,  961,  907, // 2
				856,   808,   762,   720,   678,   640,  604,  570,  538,  508,  480,  453,  // 3
				428,   404,   381,   360,   339,   320,  302,  285,  269,  254,  240,  226,  // 4
				214,   202,   190,   180,   170,   160,  151,  143,  135,  127,  120,  113,  // 5
				107,   101,    95,    90,    85,    80,   76,   71,   67,   64,   60,   57,  // 6
				54,    51,    48,    45,    42,    40,   38,   36,   34,   32,    30,  28,   // 7
	
				27,    25,    24,    22,    21,    20,   19,   18,   17,   16,    15,  14,
				13,    13,    12,    11,    11,    10,    9,    9
		};


		/// <summary>
		/// This table is used for the vibrato/tremolo effect
		/// </summary>
		public static readonly int[] MOD_EFFECTS_SINE_TABLE = new int[]	{
			0, 24, 49, 74, 97,120,141,161,
			180,197,212,224,235,244,250,253,
			255,253,250,244,235,224,212,197,
			180,161,141,120, 97, 74, 49, 24,0
		};

		#endregion

		public TPParserUtilsMOD()	{
		}

		#region Public static methods

		public static short GetPeriodTableIndex(short sNotePeriod) {
			// why 24? See the notes of period_table[]
			int nArraySize = S3M_PERIOD_TABLE.Length;
			for (short i = 24; i < nArraySize; i++)
			{
				if (sNotePeriod >= S3M_PERIOD_TABLE[i])
				{
					return System.Convert.ToInt16(i - 24);
				}
			}

			return -1;
		}

		/// <summary>
		/// Get notes frequency according to the following algorithem:
		/// taken from fs3mdoc.txt
		/// +--------------------------------------------------------------+
		/// | current_period = 8363 * periodtab[note] / instrument's C2SPD |
		/// +--------------------------------------------------------------+
		///
		/// To get the rate in hz, then the following formula is used:
		///
		/// +-------------------------------+
		/// | speed_hz = 14317056L / period |
		/// +-------------------------------+
		/// </summary>
		/// <param name="notePeriodTableIndex">The period table of the note</param>
		/// <param name="sampleC2SPD">The original rate of the sample</param>
		/// <returns></returns>
		public static int GetNoteFrequency(int notePeriodTableIndex, int sampleC2SPD)	{
			if ((-1 < notePeriodTableIndex) && (notePeriodTableIndex < S3M_PERIOD_TABLE.Length)) {
				int currentPeriod = (8363 * S3M_PERIOD_TABLE[notePeriodTableIndex]) / sampleC2SPD;
				return (int)((14317056L / currentPeriod) + 0.5);
			}

			return 0;
		}

		/// <summary>
		/// Used by effects that affect the frequency value - playing with the amiga period values
		/// </summary>
		/// <param name="period"></param>
		/// <param name="sampleC2SPD"></param>
		/// <returns></returns>
		public static int GetNoteFrequencyByPeriod(int period, int sampleC2SPD) {
			if ((period < S3M_PERIOD_TABLE[0]) &&
				  (period > S3M_PERIOD_TABLE[S3M_PERIOD_TABLE.Length - 1])) {
				int currentPeriod = (8363 * period) / sampleC2SPD;
				return (int)((14317056L / currentPeriod) + 0.5);
			}

			return 8363;
		}

		public static int GetAmigaPeriodValue(short notePeriodTableIndex)	{
			if ((-1 < notePeriodTableIndex) && (notePeriodTableIndex < S3M_PERIOD_TABLE.Length)) {
				return S3M_PERIOD_TABLE[notePeriodTableIndex];
			}

			return 0;
		}

		
		/// <summary>
		/// Helper function to convert a short (16 bit) value from little-endian to big-endian and 
		/// vice-versa.
		/// </summary>
		/// <param name="nValue"></param>
		/// <returns></returns>
		public static short ConvertEndianess(short nValue) { 
			// create a word value with the first byte only (by shifting right 1 byte and masking with
			// 0xFF, then add (with the or sign) the second byte shifted to the left and isolated
			// by masking with 0xFF00.
			return (short)(((nValue >> 8) & 0xFF) | ((nValue << 8) & 0xFF00)); 
		}

		#endregion
	}
}
