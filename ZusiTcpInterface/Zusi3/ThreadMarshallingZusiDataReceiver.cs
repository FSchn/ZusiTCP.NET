﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZusiTcpInterface.Zusi3
{
  public class ThreadMarshallingZusiDataReceiver : CallbackBasedZusiDataReceiverBase, IDisposable
  {
    private readonly IBlockingCollection<DataChunkBase> _blockingCollection;
    private readonly SynchronizationContext _synchronizationContext;
    private Task _marshallingTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private bool _disposed;

    public ThreadMarshallingZusiDataReceiver(IBlockingCollection<DataChunkBase> blockingCollection, SynchronizationContext synchronizationContext = null)
    {
      _blockingCollection = blockingCollection;
      _synchronizationContext = synchronizationContext ?? SynchronizationContext.Current;

      _marshallingTask = Task.Run((Action) MainMarshallingLoop);
    }

    private void MainMarshallingLoop()
    {
      var cancellationToken = _cancellationTokenSource.Token;

      try
      {
        while (true)
        {
          var chunk = _blockingCollection.Take(cancellationToken);
          _synchronizationContext.Post(RaiseEventFor, chunk);
        }
      }
      catch (OperationCanceledException)
      {
        // Teardown requested
      }
    }

    private void RaiseEventFor(object chunk)
    {
      base.RaiseEventFor((DataChunkBase)chunk);
    }

    public void Dispose()
    {
      if (_disposed)
        return;

      _cancellationTokenSource.Cancel();

      if (_marshallingTask != null && !_marshallingTask.Wait(500))
        throw new TimeoutException("Failed to shut down message recption task within timeout.");
      _marshallingTask = null;

      _disposed = true;
    }
  }
}