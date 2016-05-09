using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal class TopLevelNodeConverter : INodeConverter
  {
    private readonly Dictionary<short, INodeConverter> _topLevelNodeConverters = new Dictionary<short, INodeConverter>();

    public Dictionary<short, INodeConverter> TopLevelNodeConverters
    {
      get { return _topLevelNodeConverters; }
    }

    /// <summary>
    /// Equivalent to indexing TopLevelNodeConverters
    /// </summary>
    /// <param name="i">Node Id</param>
    /// <returns>The INodeConverter stored for this id</returns>
    public INodeConverter this[short i]
    {
      get { return _topLevelNodeConverters[i]; }
      set { _topLevelNodeConverters[i] = value; }
    }

    public IEnumerable<IProtocolChunk> Convert(Node node)
    {
      return (_topLevelNodeConverters.ContainsKey(node.Id))
        ? _topLevelNodeConverters[node.Id].Convert(node)
        : Enumerable.Empty<IProtocolChunk>();
    }
  }
}