namespace ZusiTcpInterface.Zusi3
{
  /// <summary>
  ///   Represents a boolean-like value exteded with flashing and not available.
  /// </summary>
  public enum FlashableValue
  {
    /// <summary>
    ///   The value is not available.
    /// </summary>
    NotAvailable = -1,
    /// <summary>
    ///   The value is false
    /// </summary>
    Off = 0,
    /// <summary>
    ///   The value is true.
    /// </summary>
    On = 1,
    /// <summary>
    ///   The value is flashing.
    /// </summary>
    Flashing = 2
  }
}