namespace TPlayer.Model {
	/// <summary>
	/// This class stores a single pattern of a module.  A pattern is built of rows - 
	/// usually 64 rows, and each row is built of channels (aka tracks)
	/// </summary>
	public class TPPattern {
		public TPPattern() {
		}

		#region Member Variables
		
		/// <summary>
		/// Stores the rows of notes of this pattern in the form of m_Rows[row, note], so
		/// you iterate over each row (usually 64) and for each row you iterate the notes:
		/// i.e.:
		/// int Row = 0; // first row is zero-based
		/// 
		/// for (int noteIndex = 0; noteIndex < 4; noteIndex++)
		/// {
		///		TPNote curNote = m_rows[row, noteIndex];
		/// }
		/// </summary>
		public TPNote[,] m_rows = null;
		
		#endregion

	}
}
