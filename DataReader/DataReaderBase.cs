#region header
// /*************************************************************************
//  * DataReaderBase.cs
//  * Contains logic for the TCP interface.
//  * 
//  * (C) 2013-2013 Andreas Karg, <Clonkman@gmx.de>
//  * 
//  * This file is part of Zusi TCP Interface.NET.
//  *
//  * Zusi TCP Interface.NET is free software: you can redistribute it and/or
//  * modify it under the terms of the GNU General Public License as
//  * published by the Free Software Foundation, either version 3 of the
//  * License, or (at your option) any later version.
//  *
//  * Zusi TCP Interface.NET is distributed in the hope that it will be
//  * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  * GNU General Public License for more details.
//  *
//  * You should have received a copy of the GNU General Public License
//  * along with Zusi TCP Interface.NET. 
//  * If not, see <http://www.gnu.org/licenses/>.
//  * 
//  *************************************************************************/
#endregion

using System;
using Zusi_Datenausgabe.EventManager;
using Zusi_Datenausgabe.NetworkIO;

namespace Zusi_Datenausgabe.DataReader
{
  public interface IDataReader
  {
    string InputType { get; }
    Type OutputType { get; }

    //TODO: Find a way to get rid of the references as parameter.
    int ReadDataAndInvokeEvents(int id, IBinaryReader binaryReader, IEventInvocator<int> eventInvocator);
  }

  public abstract class DataReaderBase<TOutput> : IDataReader
  {
    #region Implementation of IDataReader<out TOutput>

    public abstract TOutput HandleData(IBinaryReader reader, out int bytesRead);

    #endregion

    #region Implementation of IDataReader

    public abstract string InputType { get; }

    public Type OutputType
    {
      get
      {
        return typeof(TOutput);
      }
    }

    public int ReadDataAndInvokeEvents(int id, IBinaryReader binaryReader, IEventInvocator<int> eventInvocator)
    {
      int bytesRead;
      var data = HandleData(binaryReader, out bytesRead);

      eventInvocator.Invoke(id, this, new DataReceivedEventArgs<TOutput>(id, data));

      return bytesRead;
    }

    #endregion
  }
}