using System;
using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.DOM;

namespace ZusiTcpInterface.Converters
{
  internal class FlatteningNodeConverter : INodeConverter
  {
    private Dictionary<Address, Func<Address, byte[], IProtocolChunk>> _attributeConversionFunctions = new Dictionary<Address, Func<Address, byte[], IProtocolChunk>>();
    private Dictionary<Address, Func<Address, bool, IProtocolChunk>> _nodeConversionFunctions = new Dictionary<Address, Func<Address, bool, IProtocolChunk>>();

    public Dictionary<Address, Func<Address, byte[], IProtocolChunk>> AttributeConversionFunctions
    {
      get { return _attributeConversionFunctions; }
      set { _attributeConversionFunctions = value; }
    }
    public Dictionary<Address, Func<Address, bool, IProtocolChunk>> NodeConversionFunctions
    {
      get { return _nodeConversionFunctions; }
      set { _nodeConversionFunctions = value; }
    }

    public IEnumerable<IProtocolChunk> Convert(Address baseAddress, Node node)
    {
      var chunks = new List<IProtocolChunk>();
      var nodeAddress = baseAddress.Concat(node.Id);
      
      
      Func<Address, bool, IProtocolChunk> nodeConverter;
      if (!_nodeConversionFunctions.TryGetValue(nodeAddress, out nodeConverter))
        nodeConverter = null;
      
      if (nodeConverter != null)
      {
        IProtocolChunk result = nodeConverter(nodeAddress, true);
        
        if (result != null)
          chunks.Add(result);
      }


      foreach (KeyValuePair<short, DOM.Attribute> attribute in node.Attributes)
      {
        var attributeAddress = nodeAddress.Concat(attribute.Key);
        Func<Address, byte[], IProtocolChunk> attributeConverter;

        if (!_attributeConversionFunctions.TryGetValue(attributeAddress, out attributeConverter))
          continue;
        
        IProtocolChunk result = attributeConverter(nodeAddress.Concat(attribute.Key), attribute.Value.Payload);
        
        if (result != null)
          chunks.Add(result);
      }

      var childNodeChunks = node.ChildNodes.SelectMany(childNode => Convert(nodeAddress, childNode));
      
      chunks.AddRange(childNodeChunks);
      
      if (nodeConverter != null)
      {
        IProtocolChunk result = nodeConverter(nodeAddress, false);
        
        if (result != null)
          chunks.Add(result);
      }

      return chunks;
    }
  }
}