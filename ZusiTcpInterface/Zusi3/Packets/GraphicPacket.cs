using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using System.IO;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3.Packets
{
  internal struct GraphicPacket
  {
    private const NodeCategory NodeCategory = Zusi3.DOM.NodeCategory.CabInfo;
    private const short GraphicNodeId = 0x010C;

    private readonly byte[] _imageData;
    private readonly byte _forViewID;
    private readonly string _melderName;
    private readonly System.Int16 _subIndex;
    private readonly byte _textureIndex;

    /// <summary>
    ///   Sends a texture to the simulator.
    /// </summary>
    /// <param name="imageData">The bytes representing the image.</param>
    /// <param name="forView">Index of the view.</param>
    /// <param name="melderName">The name of the destination polygon.</param>
    /// <param name="subIndex">The index of the "Stellung".</param>
    /// <param name="textureIndex">In case of multiple textures mixed via transparency: The index of the texture.</param>
    public GraphicPacket(byte[] imageData, byte forView, string melderName, System.Int16 subIndex, byte textureIndex /*= 0*/)
    {
      _imageData = imageData;
      _forViewID = forView;
      _melderName = melderName;
      _subIndex = subIndex;
      _textureIndex = textureIndex;
    }
    
    [Contracts.Pure]
    public byte[] ImageData
    {
      get { return _imageData; }
    }

    [Contracts.Pure]
    public byte ForViewID
    {
      get { return _forViewID; }
    }

    [Contracts.Pure]
    public string MelderName
    {
      get { return _melderName; }
    }

    [Contracts.Pure]
    public System.Int16 SubIndex
    {
      get { return _subIndex; }
    }

    [Contracts.Pure]
    public byte TextureIndex
    {
      get { return _textureIndex; }
    }

    public void Serialise(BinaryWriter binaryWriter)
    {
      var attributes = new Dictionary<short, Attribute>
      {
        { 0x01, new Attribute(0x01, ForViewID)},
        { 0x02, new Attribute(0x02, MelderName)},
        { 0x03, new Attribute(0x03, SubIndex)}, 
        { 0x04, new Attribute(0x04, TextureIndex)}, 
        { 0x05, new Attribute(0x05, ImageData)}
      };

      var graphicNode = new Node(GraphicNodeId, attributes);

      var topLevelNode = new Node((short)NodeCategory, graphicNode);

      topLevelNode.Serialise(binaryWriter);
    }
  }
}
