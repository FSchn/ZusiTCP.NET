namespace ZusiTcpInterface.Zusi3
{
  /// <summary>
  ///   Represents a action performed to a switch.
  /// </summary>
  public enum SwitchAction
  {
    /// <summary>
    ///   Sets the switch to off.
    /// </summary>
    Off = 0,

    /// <summary>
    ///   Sets the switch to on.
    /// </summary>
    On = 1,

    /// <summary>
    ///   Toogles the switch.
    /// </summary>
    Toogle = -1
  }
}