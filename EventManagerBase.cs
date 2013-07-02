﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Zusi_Datenausgabe
{
  public abstract class EventManagerBase<TKey>
  {
    private IDictionary<TKey, ITypedMethodListBase> _eventHandlers = new Dictionary<TKey, ITypedMethodListBase>();
    private readonly ITypedMethodListFactory _methodListFactory;

    protected EventManagerBase(ITypedMethodListFactory methodListFactory)
    {
      _methodListFactory = methodListFactory;
    }

    protected void Subscribe<T>(TKey key, EventHandler<DataReceivedEventArgs<T>> handler)
    {
      var handlerList = GetDictionaryEntry<T>(key);
      handlerList.Subscribe(handler);
    }

    protected void Unsubscribe<T>(TKey key, EventHandler<DataReceivedEventArgs<T>> handler)
    {
      ITypedMethodList<DataReceivedEventArgs<T>> handlerList;
      var entryExists = TryGetDictionaryEntry(key, out handlerList);

      if (!entryExists)
        return;

      handlerList.Unsubscribe(handler);
    }

    protected void Invoke<T>(TKey key, object sender, DataReceivedEventArgs<T> eventArgs)
    {
      ITypedMethodList<DataReceivedEventArgs<T>> handlerList;
      var entryExists = TryGetDictionaryEntry(key, out handlerList);

      if (!entryExists)
        return;

      handlerList.Invoke(sender, eventArgs);
    }

    private ITypedMethodList<DataReceivedEventArgs<T>> GetDictionaryEntry<T>(TKey key)
    {
      ITypedMethodList<DataReceivedEventArgs<T>> typedMethodList;
      if (TryGetDictionaryEntry(key, out typedMethodList))
        return typedMethodList;

      var newList = CreateNewList<T>(key);
      return newList;
    }

    private bool TryGetDictionaryEntry<T>(TKey key, out ITypedMethodList<DataReceivedEventArgs<T>> result)
    {
      ITypedMethodListBase handlerList;
      var entryFound = _eventHandlers.TryGetValue(key, out handlerList);

      if (entryFound)
      {
        result = CastToListType<T>(handlerList);
        return true;
      }

      result = null;
      return false;
    }

    private ITypedMethodList<DataReceivedEventArgs<T>> CastToListType<T>(ITypedMethodListBase handlerList)
    {
      Debug.Assert(handlerList is ITypedMethodList<DataReceivedEventArgs<T>>);

      return handlerList as ITypedMethodList<DataReceivedEventArgs<T>>;
    }

    private ITypedMethodList<DataReceivedEventArgs<T>> CreateNewList<T>(TKey key)
    {
      var newList = _methodListFactory.Create<DataReceivedEventArgs<T>>();
      _eventHandlers[key] = newList;
      return newList;
    }
  }
}