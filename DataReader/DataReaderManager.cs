﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Zusi_Datenausgabe.TcpCommands;

namespace Zusi_Datenausgabe.DataReader
{
  public class DataReaderManager : IDataReaderManager
  {
    private readonly IDataReaderDictionary _readersByType;
    private readonly ITcpCommandDictionary _commandDictionary;
    private readonly IDictionary<int, IDataReader> _readersById = new Dictionary<int, IDataReader>();

    public DataReaderManager(IDataReaderDictionary readersByType, ITcpCommandDictionary commandDictionary)
    {
      _readersByType = readersByType;
      _commandDictionary = commandDictionary;
    }

    public bool GetOutputType(int id, out Type eventtype)
    {
      var reader = GetReader(id);

      eventtype = reader.OutputType;

      // Todo: Make sure missing IDs don't break anything.
      return true;
    }

    public IDataReader GetReader(int id)
    {
      IDataReader result;

      if (_readersById.TryGetValue(id, out result))
        return result;

      AcquireNewReader(id);

      bool readerNowExists = _readersById.TryGetValue(id, out result);
      Debug.Assert(readerNowExists); // Todo: Make sure this can't be broken in release builds

      return result;
    }

    private void AcquireNewReader(int id)
    {
      var command = _commandDictionary[id];
      var reader = _readersByType[command.Type];

      _readersById[id] = reader;
    }
  }

  public interface IDataReaderManager
  {
    IDataReader GetReader(int id);
    bool GetOutputType(int id, out Type eventtype);
  }
}