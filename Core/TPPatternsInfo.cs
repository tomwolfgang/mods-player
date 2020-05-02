using System;

namespace TPlayer.Model {
	/// <summary>
	/// Stores information about the patterns in the song - order array, length, how many
	/// patterns exist
	/// </summary>
	public class TPPatternsInfo	{
		public TPPatternsInfo()	{
		}

		#region Member Variables
		
		/// <summary>
		/// Not used anymore, but still exists in modules
		/// </summary>
		public int m_songEndJumpPos;

		/// <summary>
		/// How many patterns are to be played in this song (this doesn't mean there are
		/// m_SongLengthInPatterns number of patterns in the file, there can be patterns
		/// that are played more than once and patterns that aren't played at all - not
		/// likely)
		/// </summary>
		public int m_songLengthInPatterns;

		/// <summary>
		/// Stores the order of the patterns to be played in the song
		/// </summary>
		public byte[] m_orderArray = null;
		
		#endregion

	}
}
