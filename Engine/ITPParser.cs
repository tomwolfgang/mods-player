using System;
using TPlayer.Model;

namespace TPlayer.Engine {
	/// <summary>
	/// Interface from which a module parser must inherit
	/// </summary>
	public interface ITPParser {
		/// <summary>
		/// This function returns true if the parser supports the format, it will also fill
		/// a TPModuleTypeInfo class with the module's type information (as the parser
		/// sees it)
		/// </summary>
		/// <param name="ModuleBuffer">The buffer of the module file - stream from any underlying source</param>
		/// <param name="TypeInfo">A class of type TPModuleTypeInfo to be filled with information about the given module (if supported)</param>
		/// <returns>True if supported, false if not</returns>
		bool IsFormatSupported(System.IO.Stream moduleBuffer, ref TPModuleTypeInfo typeInfo);

		/// <summary>
		/// Loads a module from a buffer stream
		/// </summary>
		/// <param name="ModuleBuffer">Stream source of module</param>
		/// <returns>On fail, returns null, else, returns a module</returns>
		TPModule LoadModule(System.IO.Stream moduleBuffer);

		/// <summary>
		/// Serializes the module into a memory stream
		/// </summary>
		/// <param name="Module"></param>
		/// <returns></returns>
		System.IO.MemoryStream SaveModule(TPModule module);

		/// <summary>
		/// Returns a string keyed hash table of extensions of formats supported by 
		/// this parser.  The values are the description of the extensions
		/// </summary>
		/// <returns></returns>
		System.Collections.Hashtable GetSupportedExtensions();
	}
}
