﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using ZusiTcpInterface.Zusi3.DOM;
using Attribute = ZusiTcpInterface.Zusi3.DOM.Attribute;

namespace ZusiTcpInterface.Zusi3.Packets
{
  internal static class HelloPacket
  {
    private const NodeCategory NodeCategory = DOM.NodeCategory.Handshake;
    private const short HelloNodeId = 0x01;
    private const short ProtocolVersion = 2;

    public static void Serialise(BinaryWriter binaryWriter, ClientType clientType, string clientName, string clientVersion)
    {
      var attributes = new Dictionary<short, Attribute>
      {
        { 0x01, new Attribute(0x01, ProtocolVersion)},
        { 0x02, new Attribute(0x02, (short) clientType)},
        { 0x03, new Attribute(0x03, clientName)}, 
        { 0x04, new Attribute(0x04, clientVersion)}
      };

      var helloNode = new Node(HelloNodeId, attributes);
      var topLevelNode = new Node((short)NodeCategory, helloNode);

      topLevelNode.Serialise(binaryWriter);
    }
  }
}
