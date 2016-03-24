﻿using System.Collections.Generic;
using System.IO;

namespace ZusiTcpInterface.Zusi3
{
  internal class Handshaker
  {
    private readonly BinaryWriter _binaryWriter;

    private readonly ClientType _clientType;
    private readonly string _clientName;
    private readonly string _clientVersion;
    private readonly IEnumerable<short> _neededData;
    private readonly IBlockingCollection<IProtocolChunk> _rxQueue;

    public Handshaker(IBlockingCollection<IProtocolChunk> rxQueue, BinaryWriter binaryWriter, ClientType clientType, string clientName, string clientVersion, IEnumerable<short> neededData)
    {
      _binaryWriter = binaryWriter;
      _clientType = clientType;
      _clientName = clientName;
      _clientVersion = clientVersion;
      _neededData = neededData;
      _rxQueue = rxQueue;
    }

    public void ShakeHands()
    {
      var hello = new HelloPacket(_clientType, _clientName, _clientVersion);

      hello.Serialise(_binaryWriter);

      var handshakeConverter = new BranchingNodeConverter();
      handshakeConverter[0x02] = new AckHelloConverter();

      var rootNodeConverter = new TopLevelNodeConverter();
      rootNodeConverter[0x01] = handshakeConverter;

      var ackHello = (AckHelloPacket)_rxQueue.Take();

      if(!ackHello.ConnectionAccepted)
        throw new ConnectionRefusedException("Connection refused by Zusi.");

      var neededDataPacket = new NeededDataPacket(_neededData);
      neededDataPacket.Serialise(_binaryWriter);

      var ackNeededData = (AckNeededDataPacket) _rxQueue.Take();

      if(!ackNeededData.RequestAccepted)
        throw new ConnectionRefusedException("Needed data rejected by Zusi.");
    }
  }
}