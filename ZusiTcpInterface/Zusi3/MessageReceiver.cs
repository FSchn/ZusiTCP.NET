using System.IO;

namespace ZusiTcpInterface.Zusi3
{
  internal class MessageReceiver
  {
    private readonly BinaryReader _binaryReader;
    private readonly Converters.TopLevelNodeConverter _rootNodeConverter;
    private readonly IBlockingCollection<IProtocolChunk> _blockingChunkQueue;

    public MessageReceiver(BinaryReader binaryReader, Converters.TopLevelNodeConverter rootNodeConverter, IBlockingCollection<IProtocolChunk> blockingChunkQueue)
    {
      _binaryReader = binaryReader;
      _rootNodeConverter = rootNodeConverter;
      _blockingChunkQueue = blockingChunkQueue;
    }

    public void ProcessNextPacket()
    {
      var message = DOM.Node.Deserialise(_binaryReader);
      var chunks = _rootNodeConverter.Convert(message);

      foreach (var chunk in chunks)
      {
        _blockingChunkQueue.Add(chunk);
      }
    }
  }
}