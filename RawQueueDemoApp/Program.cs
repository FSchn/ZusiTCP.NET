using System;
using ZusiTcpInterface;
using ZusiTcpInterface.Enums;

namespace RawQueueDemoApp
{
  internal static class Program
  {
    private static void Main(string[] args)
    {
      Console.WriteLine("Connecting...");

      var connectionCreator = new ConnectionCreator();

      var velocityAddress = connectionCreator.Descriptors["Geschwindigkeit"].Address;
      var gearboxPilotLightAddress = connectionCreator.Descriptors["LM Getriebe"].Address;
      var sifaPilotLightAddress = connectionCreator.Descriptors["Status Sifa-Leuchtmelder"].Address;
      var sifaHornAddress = connectionCreator.Descriptors["Status Sifa-Hupe"].Address;
      var rootAddress = connectionCreator.Descriptors["root"].Address;
      
      var trainSettingAddress = connectionCreator.Descriptors["Status Zugverband"].Address;
      var trainSettingAddressFzg = connectionCreator.Descriptors["Status Zugverband:Fahrzeug"].Address;
      var trainSettingAddressFzgFileName = connectionCreator.Descriptors["Status Zugverband:Fahrzeug:Dateiname"].Address;
      var trainSettingAddressFzgDesc = connectionCreator.Descriptors["Status Zugverband:Fahrzeug:Beschreibung"].Address;

      connectionCreator.NeededData.Request(velocityAddress, gearboxPilotLightAddress, sifaHornAddress, sifaHornAddress, trainSettingAddress);

      using (var connection = connectionCreator.CreateConnection())
      {
        Console.WriteLine("Connected!");

        while (!Console.KeyAvailable)
        {
          DataChunkBase chunk;
          bool chunkTaken = connection.ReceivedDataChunks.TryTake(out chunk, 100);
          if(!chunkTaken)
            continue;

          if (chunk.Address == velocityAddress)
          {
            Console.WriteLine("Velocity [km/h] = {0}", ((DataChunk<Single>) chunk).Payload*3.6f);
          }
          else if (chunk.Address == gearboxPilotLightAddress)
          {
            Console.WriteLine("Gearbox pilot light = {0}", ((DataChunk<bool>) chunk).Payload);
          }
          else if (chunk.Address == sifaPilotLightAddress)
          {
            Console.WriteLine("Sifa pilot light = {0}", ((DataChunk<bool>)chunk).Payload);
          }
          else if (chunk.Address == sifaHornAddress)
          {
            Console.WriteLine("Sifa horn state = {0}", ((DataChunk<StatusSifaHupe>)chunk).Payload);
          }
          else if (chunk.Address == rootAddress)
          {
            //if (((DataChunk<bool>)chunk).Payload)
            //  Console.WriteLine("--Begin--");
            //else
            //  Console.WriteLine("--End--");
          }
          else if (chunk.Address == trainSettingAddress)
          {
            if (((DataChunk<bool>)chunk).Payload)
              Console.WriteLine("--Start Train--");
            else
              Console.WriteLine("--End Train--");
          }
          else if (chunk.Address == trainSettingAddressFzg)
          {
            Console.WriteLine("--Vehicle--");
          }
          else if (chunk.Address == trainSettingAddressFzgFileName)
          {
            Console.WriteLine("File = {0}", ((DataChunk<string>)chunk).Payload);
          }
          else if (chunk.Address == trainSettingAddressFzgDesc)
          {
            Console.WriteLine("BR = {0}", ((DataChunk<string>)chunk).Payload);
          }
          else
          {
            //Remarks: programes have to ignore unknown input, but it may be helpful to have a look at it at this tutorial.
            //Console.WriteLine("unknown address = {0}", chunk.Address.ToString());
          }
        }
      }

      Console.WriteLine("Disconnected");
    }
  }
}