namespace TPlayer.Model {
  /// <summary>
  /// This class represents a basic module with the basic features a module
  /// needs: Patterns, PatternsInfo, ModuleTypeInfo and Samples.
  /// You may want to inherit from this class in order to create a more richer
  /// module implementation
  /// </summary>
  public class TPModule {
    public TPModule() {
    }

    public override string ToString() {
      return m_moduleName;
    }

    #region Member Variables

    /// <summary>
    /// Name of the module
    /// </summary>
    public string m_moduleName;

    /// <summary>
    /// Information about the module type
    /// </summary>
    public TPModuleTypeInfo m_moduleTypeInfo;

    /// <summary>
    /// Pattern play order
    /// </summary>
    public TPPatternsInfo m_patternsInfo;

    /// <summary>
    /// Samples
    /// </summary>
    public TPSample[] m_samples;

    /// <summary>
    /// Patterns
    /// </summary>
    public TPPattern[] m_patterns;

    #endregion
  }
}
