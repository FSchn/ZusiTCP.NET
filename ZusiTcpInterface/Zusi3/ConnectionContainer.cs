using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ZusiTcpInterface.Zusi3.Converters;
using ZusiTcpInterface.Zusi3.DOM;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3
{
  public class ConnectionContainer : IDisposable
  {
    private readonly DescriptorCollection _descriptors;
    private readonly HashSet<short> _neededData = new HashSet<short>();
    private static TopLevelNodeConverter _topLevelNodeConverter;
    private readonly IBlockingCollection<CabDataChunkBase> _receivedCabDataChunks = new BlockingCollectionWrapper<CabDataChunkBase>();
    private readonly BlockingCollectionWrapper<IProtocolChunk> _receivedChunks = new BlockingCollectionWrapper<IProtocolChunk>();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private string _clientName = "Unnamed";
    private string _clientVersion = "Unknown";

    #region Fields involved in object disposal

    private TcpClient _tcpClient;
    private Task _cabDataForwardingTask;
    private bool _hasBeenDisposed;
    private Task _messageReceptionTask;

    #endregion Fields involved in object disposal

    public DescriptorCollection Descriptors
    {
      get { return _descriptors; }
    }

    public HashSet<short> NeededData
    {
      get { return _neededData; }
    }

    public IBlockingCollection<CabDataChunkBase> ReceivedCabDataChunks
    {
      get { return _receivedCabDataChunks; }
    }

    public string ClientName
    {
      get { return _clientName; }
      set 
      { 
        if (_messageReceptionTask != null) 
          throw new System.InvalidOperationException(); 
        else 
          _clientName = value;
      }
    }

    public string ClientVersion
    {
      get { return _clientVersion; }
      set 
      { 
        if (_messageReceptionTask != null) 
          throw new System.InvalidOperationException(); 
        else 
          _clientVersion = value;
      }
    }

    private static List<CabInfoTypeDescriptor> ReadCabListSave(string cabInfoTypeDescriptorFilename)
    {
      List<CabInfoTypeDescriptor> cabInfoDescriptors;
      using (var commandSetFileStream = File.OpenRead(cabInfoTypeDescriptorFilename))
      {
        cabInfoDescriptors = CabInfoTypeDescriptorReader.ReadCommandsetFrom(commandSetFileStream).ToList();
      }
      return cabInfoDescriptors;
    }
    public ConnectionContainer(string cabInfoTypeDescriptorFilename = "Zusi3/CabInfoTypes.csv")
      :this(ReadCabListSave(cabInfoTypeDescriptorFilename))
    {
    }
    public ConnectionContainer(IEnumerable<CabInfoTypeDescriptor> cabInfoDescriptors)
    {
      _descriptors = new DescriptorCollection(cabInfoDescriptors);
      var cabInfoConversionFunctions = GenerateConversionFunctions(cabInfoDescriptors);

      SetupNodeConverters(cabInfoConversionFunctions);
      
      try //Default values for ClientName and ClientVersion => Read from the .exe-Assembly
      {
        if ((System.Reflection.Assembly.GetEntryAssembly() != null) &&
            (System.Reflection.Assembly.GetEntryAssembly().GetName() != null))
        {
          object[] assembyTitle = System.Reflection.Assembly.GetEntryAssembly().GetCustomAttributes(
              typeof(System.Reflection.AssemblyTitleAttribute), true);
          if ((assembyTitle != null) && (assembyTitle.Length == 1))
          {
            _clientName = ((System.Reflection.AssemblyTitleAttribute)(assembyTitle[0])).Title;
          }
          else if (System.Reflection.Assembly.GetEntryAssembly().GetName().Name != "")
          {
            _clientName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
          }
          
          if ((System.Reflection.Assembly.GetEntryAssembly().GetName().Version != null) &&
              (System.Reflection.Assembly.GetEntryAssembly().GetName().Version != new Version(0, 0)))
          {
            _clientVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString(); 
          }
        }
      }
      catch
      {
      }
    }

    private static void SetupNodeConverters(Dictionary<short, Func<short, byte[], IProtocolChunk>> cabInfoConversionFunctions)
    {
      var handshakeConverter = new BranchingNodeConverter();
      var ackHelloConverter = new AckHelloConverter();
      var ackNeededDataConverter = new AckNeededDataConverter();
      handshakeConverter[0x02] = ackHelloConverter;

      var cabDataConverter = new CabDataConverter(cabInfoConversionFunctions);
      var userDataConverter = new BranchingNodeConverter();
      userDataConverter[0x04] = ackNeededDataConverter;
      userDataConverter[0x0A] = cabDataConverter;

      _topLevelNodeConverter = new TopLevelNodeConverter();
      _topLevelNodeConverter[0x01] = handshakeConverter;
      _topLevelNodeConverter[0x02] = userDataConverter;
    }

    private Dictionary<short, Func<short, byte[], IProtocolChunk>> GenerateConversionFunctions(IEnumerable<CabInfoTypeDescriptor> cabInfoDescriptors)
    {
      var descriptorToConversionFunctionMap = new Dictionary<string, Func<short, byte[], IProtocolChunk>>()
      {
        {"single", CabDataAttributeConverters.ConvertSingle},
        {"boolassingle", CabDataAttributeConverters.ConvertBoolAsSingle},
        {"fail", (s, bytes) => {throw new NotSupportedException("Unsupported data type received");} }
      };

      return cabInfoDescriptors.ToDictionary(descriptor => descriptor.Id,
                                             descriptor => descriptorToConversionFunctionMap[descriptor.Type.ToLowerInvariant()]);
    }

    public void RequestData(string name)
    {
      _neededData.Add(_descriptors.GetBy(name).Id);
    }

    public void RequestData(params CabInfoTypeDescriptor[] descriptors)
    {
      foreach (var descriptor in descriptors)
      {
        _neededData.Add(descriptor.Id);
      }
    }

    public void Dispose()
    {
      if (_hasBeenDisposed)
        return;

      _cancellationTokenSource.Cancel();

      if(_messageReceptionTask != null && !_messageReceptionTask.Wait(500))
        throw new TimeoutException("Failed to shut down message recption task within timeout.");
      _messageReceptionTask = null;

      if (_cabDataForwardingTask != null && !_cabDataForwardingTask.Wait(500))
        throw new TimeoutException("Failed to shut down message forwarding task within timeout.");
      _cabDataForwardingTask = null;

      if(_tcpClient != null)
        _tcpClient.Close();

      _receivedCabDataChunks.CompleteAdding();
      _receivedChunks.CompleteAdding();

      _hasBeenDisposed = true;
    }

    public void Connect(string hostname = "localhost", int port = 1436)
    {
      if (_messageReceptionTask != null) 
        throw new System.InvalidOperationException(); 
      _tcpClient = new TcpClient(hostname, port);
      Connect(_tcpClient.GetStream());
    }

    public void Connect(System.Net.IPEndPoint endpoint)
    {
      if (_messageReceptionTask != null) 
        throw new System.InvalidOperationException(); 
      _tcpClient = new TcpClient(endpoint);
      Connect(_tcpClient.GetStream());
    }

    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    public void Connect(System.IO.Stream openStream)
    {
      if (_messageReceptionTask != null) 
        throw new System.InvalidOperationException(); 
      var networkStream = new CancellableBlockingStream(openStream, _cancellationTokenSource.Token);
      var binaryReader = new BinaryReader(networkStream);
      var binaryWriter = new BinaryWriter(networkStream);

      var messageReader = new MessageReceiver(binaryReader, _topLevelNodeConverter, _receivedChunks);
      _messageReceptionTask = Task.Run(() =>
      {
        while (true)
        {
          try
          {
            messageReader.ProcessNextPacket();
          }
          catch (OperationCanceledException)
          {
            // Teardown requested
            return;
          }
        }
      });

      var handshaker = new Handshaker(_receivedChunks, binaryWriter, ClientType.ControlDesk, _clientName, _clientVersion,
        _neededData);

      handshaker.ShakeHands();

      _cabDataForwardingTask = Task.Run(() =>
      {
        while (true)
        {
          IProtocolChunk protocolChunk;
          try
          {
            _receivedChunks.TryTake(out protocolChunk, -1, _cancellationTokenSource.Token);
          }
          catch (OperationCanceledException)
          {
            // Teardown requested
            return;
          }
          _receivedCabDataChunks.Add((CabDataChunkBase) protocolChunk);
        }
      });
    }
  }
}