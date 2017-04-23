using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZusiTcpInterface.Enums;
using ZusiTcpInterface.Enums.Lzb;
using ZusiTcpInterface.TypeDescriptors;

namespace ZusiTcpInterface.Converters
{
  internal static class AttributeConverters
  {
    private static readonly Dictionary<string, Func<Address, byte[], IProtocolChunk>> AttrConverterMap =
      new Dictionary<string, Func<Address, byte[], IProtocolChunk>>(StringComparer.InvariantCultureIgnoreCase)
      {
        {"single", ConvertSingle},
        {"boolassingle", ConvertBoolAsSingle},
        {"boolasbyte", ConvertBoolAsByte},
        {"string", ConvertString},
        //{"string-utf8", ConvertString},
        {"zugart", ConvertEnumAsShort<Zugart>},
        {"switchstate", ConvertEnumAsByte<SwitchState>},
        {"aktivezugdaten", ConvertEnumAsShort<AktiveZugdaten>},
        {"statussifahupe", ConvertEnumAsByte<StatusSifaHupe>},
        {"zustandzugsicherung", ConvertEnumAsShort<ZustandZugsicherung>},
        {"grundzwangsbremsung", ConvertEnumAsShort<GrundZwangsbremsung>},
        {"lzbzustand", ConvertEnumAsShort<LzbZustand>},
        {"statuslzbuebertragungsausfall", ConvertEnumAsShort<StatusLzbUebertragungsausfall>},
        {"indusihupe", ConvertEnumAsByte<IndusiHupe>},
        {"zusatzinfomelderbild", ConvertEnumAsByte<ZusatzinfoMelderbild>},
        {"pilotlightstate", ConvertEnumAsByte<PilotLightState>},
        {"statusendeverfahren", ConvertEnumAsByte<StatusEndeVerfahren>},
        {"statusauftrag", ConvertEnumAsByte<StatusAuftrag>},
        {"statusvorsichtauftrag", ConvertEnumAsByte<StatusVorsichtauftrag>},
        {"statusnothalt", ConvertEnumAsByte<StatusLzbNothalt>},
        {"statusrechnerausfall", ConvertEnumAsByte<StatusRechnerausfall>},
        {"statuselauftrag", ConvertEnumAsByte<StatusElAuftrag>},
        {"short", ConvertShort},
        {"fail", (s, bytes) => { throw new NotSupportedException("Unsupported data type received"); }}
      };
    private static readonly Dictionary<string, Func<Address, bool, IProtocolChunk>> NodeConverterMap =
      new Dictionary<string, Func<Address, bool, IProtocolChunk>>(StringComparer.InvariantCultureIgnoreCase)
      {
        {"start", ConvertStart},
        {"end", ConvertEnd},
        {"start;end", ConvertStartEnd},
        {"fail", (s, bytes) => { throw new NotSupportedException("Unsupported node type received"); }}
      };

    public static Dictionary<Address, Func<Address, byte[], IProtocolChunk>> MapToAttributeDescriptors(IEnumerable<AttributeOrNodeDescriptor> attributeDescriptors)
    {
      return MapToDescriptorsInternal<Func<Address, byte[], IProtocolChunk>>(attributeDescriptors, AttrConverterMap, false, "Attribute converter");
    }
    public static Dictionary<Address, Func<Address, bool, IProtocolChunk>> MapToNodeDescriptors(IEnumerable<AttributeOrNodeDescriptor> attributeDescriptors)
    {
      return MapToDescriptorsInternal<Func<Address, bool, IProtocolChunk>>(attributeDescriptors, NodeConverterMap, true, "Node notifier");
    }
    private static Dictionary<Address, FuncType> MapToDescriptorsInternal<FuncType>
        (IEnumerable<AttributeOrNodeDescriptor> attributeDescriptors, 
        Dictionary<string, FuncType> ConverterMap, bool selectNodes, string errorMessageConverter)
    {
      var mappedConverters = new Dictionary<Address, FuncType>();

      foreach (var descriptor in attributeDescriptors)
      {
        try
        {
          if (descriptor.IsNode == selectNodes)
            mappedConverters.Add(descriptor.Address, ConverterMap[descriptor.Type]);
        }
        catch (KeyNotFoundException e)
        {
          throw new InvalidDescriptorException(
            String.Format("Could not found {3} for type '{0}', used in descriptor 0x{1:x4} - {2}.", descriptor.Type, descriptor.Address, descriptor.Name, errorMessageConverter), e);
        }
      }

      return mappedConverters;
    }

    public static IProtocolChunk ConvertSingle(Address id, byte[] payload)
    {
      return new DataChunk<float>(id, BitConverter.ToSingle(payload, 0));
    }

    public static IProtocolChunk ConvertBoolAsSingle(Address id, byte[] payload)
    {
      return new DataChunk<bool>(id, BitConverter.ToSingle(payload, 0) != 0f);
    }

    public static IProtocolChunk ConvertString(Address id, byte[] payload)
    {
      return new DataChunk<string>(id, Encoding.Default.GetString(payload));
    }
    //public static IProtocolChunk ConvertStringUtf8(Address id, byte[] payload)
    //{
    //  return new DataChunk<string>(id, Encoding.UTF8.GetString(payload));
    //}

    public static IProtocolChunk ConvertBoolAsByte(Address id, byte[] payload)
    {
      return new DataChunk<bool>(id, payload.Single() != 0);
    }

    public static IProtocolChunk ConvertEnumAsByte<T>(Address id, byte[] payload)
    {
      var enumValue = CastToEnum<T>(payload.Single());

      return new DataChunk<T>(id, enumValue);
    }

    public static IProtocolChunk ConvertEnumAsShort<T>(Address id, byte[] payload)
    {
      var enumValue = CastToEnum<T>(BitConverter.ToInt16(payload, 0));

      return new DataChunk<T>(id, enumValue);
    }

    public static IProtocolChunk ConvertShort(Address id, byte[] payload)
    {
      return new DataChunk<short>(id, BitConverter.ToInt16(payload, 0));
    }

    private static T CastToEnum<T>(int value)
    {
      return (T) (object) value;
    }
    
    public static IProtocolChunk ConvertStart(Address id, bool isStart)
    {
      if (isStart)
        return new DataChunk<bool>(id, isStart);
      else
        return null;
    }
    public static IProtocolChunk ConvertEnd(Address id, bool isStart)
    {
      if (!isStart)
        return new DataChunk<bool>(id, isStart);
      else
        return null;
    }
    public static IProtocolChunk ConvertStartEnd(Address id, bool isStart)
    {
      return new DataChunk<bool>(id, isStart);
    }
  }
}