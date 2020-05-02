using System;

namespace TPlayer.Model
{
	/// <summary>
	/// This class stores basic information about a module such as how many maximum
	/// channels/tracks can it hold, how many maximum samples can it hold etc...
	/// </summary>
	public class TPModuleTypeInfo {
		public TPModuleTypeInfo() {
		}

		public override string ToString()	{
			return String.Format("Module Type:\t{0}\r\n" +
                           "Max Channels/Tracks:\t{1}\r\n" + 
                           "Max Samples:\t{2}\r\n" + 
                           "Rows Per Pattern:\t{3}", 
                           m_formatName, 
                           m_channels, 
                           m_maxSamples, 
                           m_rowsPerPattern);
		}

		#region Member Variables
		
		/// <summary>
		/// Stores the maximum channels for this module type
		/// </summary>
		public byte m_channels;

		/// <summary>
		/// Stores the maximum samples for this module type
		/// </summary>
		public byte m_maxSamples;

		/// <summary>
		/// The number of rows per pattern for the sample
		/// </summary>
		public byte m_rowsPerPattern;

		/// <summary>
		/// The name of the tracker that composed the module
		/// </summary>
		public string m_formatName;
		
		#endregion
	}
}
