﻿#region header
// /*************************************************************************
//  * IBinaryReader.cs
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

namespace Zusi_Datenausgabe.NetworkIO
{
  public interface IBinaryReader
  {
    int PeekChar();
    int Read();
    bool ReadBoolean();
    byte ReadByte();

    [CLSCompliant(false)]
    sbyte ReadSByte();

    char ReadChar();
    short ReadInt16();

    [CLSCompliant(false)]
    ushort ReadUInt16();

    int ReadInt32();

    [CLSCompliant(false)]
    uint ReadUInt32();

    long ReadInt64();

    [CLSCompliant(false)]
    ulong ReadUInt64();

    float ReadSingle();
    double ReadDouble();
    Decimal ReadDecimal();
    string ReadString();
    int Read(char[] buffer, int index, int count);
    char[] ReadChars(int count);
    int Read(byte[] buffer, int index, int count);
    byte[] ReadBytes(int count);
  }
}