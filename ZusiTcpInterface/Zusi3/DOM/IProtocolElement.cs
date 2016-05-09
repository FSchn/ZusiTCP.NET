using System.IO;

namespace ZusiTcpInterface.Zusi3.DOM
{
  public interface IProtocolElement
  {
    void Serialise(BinaryWriter binaryWriter);
  }
}