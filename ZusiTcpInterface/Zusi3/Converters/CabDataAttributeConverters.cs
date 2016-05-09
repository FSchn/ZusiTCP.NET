using System;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal static class CabDataAttributeConverters
  {
    public static IProtocolChunk ConvertSingle(short id, byte[] payload)
    {
      return new CabDataChunk<float>(id, BitConverter.ToSingle(payload, 0));
    }

    public static IProtocolChunk ConvertBoolAsSingle(short id, byte[] payload)
    {
      return new CabDataChunk<bool>(id, BitConverter.ToSingle(payload, 0) != 0f);
    }
    
    public static IProtocolChunk ConvertFlashableValueAsSingle(short id, byte[] payload)
    {
      return new CabDataChunk<FlashableValue>(id, (FlashableValue)(int)BitConverter.ToSingle(payload, 0));
    }
  }
}