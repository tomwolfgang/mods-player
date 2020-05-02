using System;

namespace TPlayer.Model {
	/// <summary>
	/// This class is yet another POCO used to store information for a single note.  
  /// A parser will fill information for this class
	/// and players will read from it.
	/// </summary>
	public class TPNote {
		//					  C      C#      D   D#     E  F    F#    G    G#    A   A#     B
		public enum TPNotes { C = 0, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B, None};

		public TPNote()	{
			this.m_note = TPNotes.None;
			this.m_octave = 0;
			this.m_periodTableIndex = -1;
			this.m_frequency = 0;
			this.m_sampleNumber = 0;
			this.m_effect = 0;
			this.m_effectParams = 0;
		}

		#region Member Variables
		
		/// <summary>
		/// The actual calculated frequency for the note.  The parser calculates this with
		/// a format proprietary algorithm
		/// </summary>
		public long m_frequency;

		/// <summary>
		/// Each note is, when played, transformed into a frequency.  The frequency
		/// is calculated by the parsers depending on the format of the music file played.
		/// Usually, the parsers use a conversion table to do this.  In order to make the
		/// conversion more efficient, the parsers can store the conversion table's index
		/// for the specific note (also, it may store the frequency in the frequency member
		/// see below)
		/// </summary>
		public short m_periodTableIndex;

		/// <summary>
		/// Stores the note's octave (for gui to engine and engine to gui conversion)
		/// </summary>
		public short m_octave;

		/// <summary>
		/// Stores the note (for gui to engine and engine to gui conversion)
		/// </summary>
		public TPNotes m_note;

		/// <summary>
		/// Stores the effect of the note
		/// </summary>
		public byte m_effect;

		/// <summary>
		/// Stores the parameters/arguments of the current effect
		/// </summary>
		public byte m_effectParams;

		/// <summary>
		/// Stores the sample for this note's context
		/// </summary>
		public byte m_sampleNumber;

		#endregion
	}
}
