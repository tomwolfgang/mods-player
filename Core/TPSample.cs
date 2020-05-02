using System;

namespace TPlayer.Model {
	/// <summary>
	/// This class represents a single sample - which is the actual "instrument" or
	/// sound that the modules use to create songs.  Therefore, it stores a sample data
	/// buffer.
	/// 
	/// The idea with this class, as with other classes of this type (TPNote, TPPattern etc..)
	/// is that it is a model class that has no functions, it only stores data in a logical
	/// way.  The different parsers will use these classes to store data, the different
	/// players will use these classes to retrieve the data.
	/// </summary>
	public class TPSample {
		public TPSample()	{
		}

		#region Member Variables
		
		/// <summary>
		/// Stores the display name of the sample.  Max is 22 bytes, so one more for null
		/// terminated string.
		/// </summary>
		public string m_sampleName;

		/// <summary>
		/// The length of the buffer of the sample.  See m_Data.
		/// </summary>
		public long m_length;

		/// <summary>
		/// The finetune value of the sample (what frequency was it recorded at).
		/// </summary>
		public byte m_finetune;

		/// <summary>
		/// Stores the calculated frequency that this sample was recorded at.
		/// The calculation is done using the finetune value.
		/// </summary>
		public short m_C2SPDFrequencyValue;

		/// <summary>
		/// The sample's volume, ranges from 0-64 (stored as byte to save memory).
		/// </summary>
		public byte m_volume;

		/// <summary>
		/// The repeat offset of the sample - when mixing the sample, we check for this
		/// value.
		/// </summary>
		public long m_repeatPos;
	
		/// <summary>
		/// The length of the repeat.
		/// </summary>
		public long m_repeatLength;

		/// <summary>
		/// The actual buffer of the sample (for ProTracker modules this will be an 8 bit 
		/// signed PCM data).
		/// </summary>
		public byte[] m_data = null;
		
		#endregion
	}
}
