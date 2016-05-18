class ConverterDemo
{
	private static Zusi_Datenausgabe.ZusiTcpTypeMaster master = null;
	private static Zusi_Datenausgabe.ZusiTcp3TypeClient client = null;

	public static void Main()
	{
		if (! System.IO.File.Exists("commandset3.xml") )
		{
			System.Console.WriteLine("commandset3.xml is not in the current directory. Program will not work. Press Enter to ignore.");
			System.Console.ReadLine();
		}

		System.Console.WriteLine("TCP Converter: Setup...");
		FillTranslatorDicts();
		System.Console.WriteLine("TCP Converter: Start master");
		master = new Zusi_Datenausgabe.ZusiTcpTypeMaster("Demo Converter Zusi 3 -> 2", (System.Threading.SynchronizationContext) null);
		System.Console.WriteLine("Set up Master successfull");
		master.ErrorReceived += TCPConnection_ErrorReceived;
		master.Connect("localhost", 1435);
		while (master.ConnectionState != Zusi_Datenausgabe.ConnectionState.Connected) System.Threading.Thread.Sleep(100);
		System.Console.WriteLine("Connecting successfull");
		
		System.Console.WriteLine("TCP Converter: Start client");
		client = new Zusi_Datenausgabe.ZusiTcp3TypeClient("Demo Converter Zusi 3 -> 2", "1.0.0.0", 
										(System.Threading.SynchronizationContext) null);
		System.Console.WriteLine("Set up Client successfull");
		
		RequestData();
		client.Connect("localhost", 1436);
		while (client.ConnectionState != Zusi_Datenausgabe.ConnectionState.Connected) System.Threading.Thread.Sleep(100);
		System.Console.WriteLine("Connecting successfull");
		client.FloatReceived += On_FloatReceived;
		client.BoolReceived += On_BoolReceived;
		client.ErrorReceived += On_ErrorReceived;
		client.DateTimeReceived += On_DateTimeReceived;
		client.ZugsicherungReceived += On_ZugsicherungReceived;
		client.SifaReceived += On_SifaReceived;
		client.NotbremssystemReceived += On_NotbremssystemReceived;
		client.DoorSystemReceived += On_DoorSystemReceived;
		while(true) System.Threading.Thread.Sleep(100);
	}
	
	private static void RequestData()
	{
		foreach(System.Collections.Generic.KeyValuePair<int, int> v1 in SingleIdClientToMaster)
		{
			if (master.RequestedData.Contains(v1.Value))
			{
				client.RequestData(v1.Key);
				int altStw;
				if (SteuerwagenTranslationDict.TryGetValue(v1.Key, out altStw))
				client.RequestData(altStw);
			}
		}
		foreach(System.Collections.Generic.KeyValuePair<int, int> v1 in BoolIdClientToMaster)
		{
			if (master.RequestedData.Contains(v1.Value))
			{
				client.RequestData(v1.Key);
				int altStw;
				if (SteuerwagenTranslationDict.TryGetValue(v1.Key, out altStw))
				client.RequestData(altStw);
			}
		}
		if (master.RequestedData.Contains(2610)) //Date and Year
		{
				client.RequestData(0x0023);
				client.RequestData(0x004B);
		}
		foreach(int v1 in PZBVars)
		{
			if (master.RequestedData.Contains(v1))
			{
				client.RequestData(0x0065);
				break;
			}
		}
		if (master.RequestedData.Contains(2596)) //Sifa
				client.RequestData(0x0064);
		if (master.RequestedData.Contains(2606)) //Notbremsung
				client.RequestData(0x0022);
		if (master.RequestedData.Contains(2607) || master.RequestedData.Contains(2646) 
				  || master.RequestedData.Contains(2648) || master.RequestedData.Contains(2627)) //Doors
				client.RequestData(0x0066);
		
	}
	
	private static System.Collections.Generic.Dictionary<int, int> SingleIdClientToMaster;
	private static System.Collections.Generic.HashSet<int> SingleIsV;
	private static System.Collections.Generic.Dictionary<int, int> BoolIdClientToMaster;
	private static System.Collections.Generic.HashSet<int> PZBVars;
	private static System.Collections.Generic.Dictionary<string, Zusi_Datenausgabe.PZBSystem> PZBTypes;
	private static System.Collections.Generic.HashSet<string> LZBTypes;
	private static System.Collections.Generic.Dictionary<int, int> SteuerwagenTranslationDict;
	private static System.Collections.Generic.Dictionary<int, int> SteuerwagenTranslationDict_Inverse;
	private static bool IsSteuerwagenMode = false;
	private static bool HasZugkraftStw = false;
	private static bool HasZugkraftLok = false;
	private static void FillTranslatorDicts()
	{
		SingleIdClientToMaster = new System.Collections.Generic.Dictionary<int, int>();
		SingleIsV = new System.Collections.Generic.HashSet<int>();
		BoolIdClientToMaster = new System.Collections.Generic.Dictionary<int, int>();
		PZBVars = new System.Collections.Generic.HashSet<int>();
		PZBTypes = new System.Collections.Generic.Dictionary<string, Zusi_Datenausgabe.PZBSystem>();
		LZBTypes = new System.Collections.Generic.HashSet<string>();
		SteuerwagenTranslationDict = new System.Collections.Generic.Dictionary<int, int>();
		SteuerwagenTranslationDict_Inverse = new System.Collections.Generic.Dictionary<int, int>();
		
		SingleIdClientToMaster.Add(0x0001, 2561);
		SingleIsV.Add(0x0001);
		SingleIdClientToMaster.Add(0x0002, 2562);
		SingleIdClientToMaster.Add(0x0003, 2563);
		SingleIdClientToMaster.Add(0x0004, 2564);
		SingleIdClientToMaster.Add(0x0009, 2565);
		SingleIdClientToMaster.Add(0x000A, 2566);
		SingleIdClientToMaster.Add(0x0062, 2567); //?
		SingleIdClientToMaster.Add(0x000E, 2568);
		SingleIdClientToMaster.Add(0x000F, 2569);
		SingleIdClientToMaster.Add(0x0010, 2570);
		SingleIdClientToMaster.Add(0x0011, 2571);
		SingleIdClientToMaster.Add(0x0012, 2572);
		SingleIdClientToMaster.Add(0x0015, 2576);
		SingleIdClientToMaster.Add(0x0017, 2578);
		SingleIsV.Add(0x0017);
		SingleIdClientToMaster.Add(0x0018, 2579);
		SingleIdClientToMaster.Add(0x0061, 2645);
		SingleIdClientToMaster.Add(0x000D, 2680); //?
		SingleIdClientToMaster.Add(0x005E, 2681); //Extra-Wert
		
		BoolIdClientToMaster.Add(0x0013, 2597);
		BoolIdClientToMaster.Add(0x001A, 2598);
		BoolIdClientToMaster.Add(0x001B, 2599);
		BoolIdClientToMaster.Add(0x001C, 2600);
		BoolIdClientToMaster.Add(0x001D, 2601);
		BoolIdClientToMaster.Add(0x001E, 2602);
		BoolIdClientToMaster.Add(0x001F, 2603);
		BoolIdClientToMaster.Add(0x0020, 2604);
		BoolIdClientToMaster.Add(0x0021, 2605);
		BoolIdClientToMaster.Add(0x0024, 2631);
		BoolIdClientToMaster.Add(0x0025, 2632);
		BoolIdClientToMaster.Add(0x0026, 2633);
		BoolIdClientToMaster.Add(0x0014, 2682);
		BoolIdClientToMaster.Add(0x0008, 2683);
		
		PZBVars.Add(2581);
		PZBVars.Add(2580);
		PZBVars.Add(2582);
		PZBVars.Add(2583);
		PZBVars.Add(2584);
		PZBVars.Add(2585);
		PZBVars.Add(2586);
		PZBVars.Add(2587);
		PZBVars.Add(2588);
		PZBVars.Add(2589);
		PZBVars.Add(2590);
		PZBVars.Add(2591);
		PZBVars.Add(2592);
		PZBVars.Add(2593);
		PZBVars.Add(2594);
		PZBVars.Add(2595);
		PZBVars.Add(2573);
		PZBVars.Add(2574); //ToDo: LZB-Soll != LZB/AFB-Soll
		PZBVars.Add(2635); //Zielweg 2575 macht keinen Sinn...
		PZBVars.Add(2649); //PZB-System.
		
		SteuerwagenTranslationDict.Add(0x001A, 0x0058);
		SteuerwagenTranslationDict.Add(0x001B, 0x0059);
		SteuerwagenTranslationDict.Add(0x001C, 0x005A);
		SteuerwagenTranslationDict.Add(0x001E, 0x005B);
		SteuerwagenTranslationDict.Add(0x001F, 0x005C);
		SteuerwagenTranslationDict.Add(0x0024, 0x005D);
		
		SteuerwagenTranslationDict.Add(0x0008, 0x007B);
		SteuerwagenTranslationDict.Add(0x0009, 0x007C);
		SteuerwagenTranslationDict.Add(0x000A, 0x007D);
		SteuerwagenTranslationDict.Add(0x000B, 0x007E);
		SteuerwagenTranslationDict.Add(0x000C, 0x007F);
		SteuerwagenTranslationDict.Add(0x000D, 0x0080);
		SteuerwagenTranslationDict.Add(0x000E, 0x0081);
		SteuerwagenTranslationDict.Add(0x000F, 0x0082);
		SteuerwagenTranslationDict.Add(0x0013, 0x0083);
		SteuerwagenTranslationDict.Add(0x0014, 0x0084);
		SteuerwagenTranslationDict.Add(0x0015, 0x0085);
		SteuerwagenTranslationDict.Add(0x0029, 0x0086);
		SteuerwagenTranslationDict.Add(0x002A, 0x0087);
		SteuerwagenTranslationDict.Add(0x0062, 0x0089);
		SteuerwagenTranslationDict.Add(0x0063, 0x008A);
		
		foreach(System.Collections.Generic.KeyValuePair<int, int> v1 in SteuerwagenTranslationDict)
			SteuerwagenTranslationDict_Inverse.Add(v1.Value, v1.Key);
		
		PZBTypes.Add("Indusi I54", Zusi_Datenausgabe.PZBSystem.IndusiI54);
		PZBTypes.Add("Indusi I60", Zusi_Datenausgabe.PZBSystem.IndusiI60);
		PZBTypes.Add("Indusi I60M", Zusi_Datenausgabe.PZBSystem.IndusiI60); //ToDo: Ungenaue Konvertierung
		PZBTypes.Add("Indusi I60R", Zusi_Datenausgabe.PZBSystem.IndusiI60R);
		PZBTypes.Add("Indusi I60DR", Zusi_Datenausgabe.PZBSystem.PZ80R);
		PZBTypes.Add("LZB80/I80 PZB90 V2.0", Zusi_Datenausgabe.PZBSystem.PZB90V16);
		LZBTypes.Add("LZB80/I80 PZB90 V2.0");
		PZBTypes.Add("LZB80/I80", Zusi_Datenausgabe.PZBSystem.LZB80I80);
		LZBTypes.Add("LZB80/I80");
		PZBTypes.Add("PZB90/I60R - V2.0", Zusi_Datenausgabe.PZBSystem.PZB90V16); //ToDo: Ungenaue Konvertierung
		PZBTypes.Add("PZB90/I60R - V1.5", Zusi_Datenausgabe.PZBSystem.PZB90V15);
		PZBTypes.Add("PZB90 V2.0", Zusi_Datenausgabe.PZBSystem.PZB90V16); //ToDo: Ungenaue Konvertierung
		PZBTypes.Add("PZB90 V1.5", Zusi_Datenausgabe.PZBSystem.PZB90V15);
	}
	
	private static void TCPConnection_ErrorReceived(object sender, Zusi_Datenausgabe.ZusiTcpException ex)
	{
		System.Console.WriteLine(ex.ToString());
	}
	private static void On_ErrorReceived(object sender, Zusi_Datenausgabe.ZusiTcpException ex)
	{
		System.Console.WriteLine(ex.ToString());
	}
	private static void On_FloatReceived(object sender, Zusi_Datenausgabe.DataSet<float> data)
	{
		int idNew;
		float v = data.Value;
		int idOld = data.Id;
		
		if (idOld == 0x007C)
			HasZugkraftStw = (v != 0.0f);
		if (idOld == 0x0009)
			HasZugkraftLok = (v != 0.0f);
		if (HasZugkraftStw && !IsSteuerwagenMode)
		{
			System.Console.WriteLine("Schalte in den Steuerwagenmodus.");
			IsSteuerwagenMode = true;
		}
		if (!HasZugkraftStw && HasZugkraftLok && IsSteuerwagenMode)
		{
			System.Console.WriteLine("Schalte in den Zugmodus.");
			IsSteuerwagenMode = false;
		}
		
		bool isZugId = SteuerwagenTranslationDict.ContainsKey(idOld);
		bool isSteuerwagenId = SteuerwagenTranslationDict_Inverse.ContainsKey(idOld);
		if ((IsSteuerwagenMode && isZugId) || (!IsSteuerwagenMode && isSteuerwagenId))
		{
			//System.Console.WriteLine(string.Format("Skip-mode {1:X4} at value {2}. ({0})",
			//client[data.Id], data.Id, v));
			return;
		}
		else if(isSteuerwagenId)
		{
			idOld = SteuerwagenTranslationDict_Inverse[idOld];
			//System.Console.WriteLine(string.Format("Translate {1:X4} to {3:X4} at value {2}. ({0})",
			//client[data.Id], data.Id, v, idOld));
		}
		
		if (SingleIdClientToMaster.TryGetValue(idOld, out idNew))
		{
			if (SingleIsV.Contains(idOld)) //v: m/s => km/h
			{
				v *= 3.6f;
			//System.Console.WriteLine(string.Format("Translate {1:X4} from {3} m/s to {2} km/h. ({0})",
			//client[data.Id], data.Id, v, data.Value));
		}
			if (idOld == 0x0061) //s: km => m
				v *= 1000f;
		
			master.SendSingle(v, idNew);
		}
	}
	private static void On_BoolReceived(object sender, Zusi_Datenausgabe.DataSet<bool> data)
	{
		int idNew;
		int idOld = data.Id;
		
		bool isZugId = SteuerwagenTranslationDict.ContainsKey(idOld);
		bool isSteuerwagenId = SteuerwagenTranslationDict_Inverse.ContainsKey(idOld);
		if ((IsSteuerwagenMode && isZugId) || (!IsSteuerwagenMode && isSteuerwagenId))
		{
			//System.Console.WriteLine(string.Format("Skip-mode {1:X4} at value {2}. ({0})",
			//client[data.Id], data.Id, v));
			return;
		}
		else if(isSteuerwagenId)
		{
			idOld = SteuerwagenTranslationDict_Inverse[idOld];
			//System.Console.WriteLine(string.Format("Translate {1:X4} to {3:X4} at value {2}. ({0})",
			//client[data.Id], data.Id, v, idOld));
		}
		
		if (BoolIdClientToMaster.TryGetValue(idOld, out idNew))
		{
			master.SendBoolAsSingle(data.Value, idNew);
		}
	}
	private static void On_DateTimeReceived(object sender, Zusi_Datenausgabe.DataSet<System.DateTime> data)
	{
		master.SendDateTime(data.Value, 2610);
	}
	private static void On_ZugsicherungReceived(object sender, Zusi_Datenausgabe.DataSet<Zusi_Datenausgabe.Zugsicherung> data)
	{
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_0500Hz, 2581);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_1000Hz, 2580);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_2000Hz, 2582);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_U, 2583);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_M, 2584);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_O, 2585);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_H, 2586);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_G, 2587);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_E40, 2588);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_EL, 2589);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_Ende, 2590);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_V40, 2591);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_B, 2592);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_S, 2593);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_Ue, 2594);
		master.SendBoolAsSingle(data.Value.StateIndusi.LM_PruefStoer, 2595);
		master.SendSingle(data.Value.StateIndusi.vZiel * 3.6f, 2573);
		master.SendSingle(data.Value.StateIndusi.vSoll * 3.6f, 2574); //ToDo: LZB-Soll != LZB/AFB-Soll
		master.SendSingle(data.Value.StateIndusi.Zielweg, 2635); //Zielweg 2575 macht keinen Sinn...
		
		if (data.Value.StateIndusi.LM_Ue && LZBTypes.Contains(data.Value.ZugsicherungName))
			master.SendPZBAsInt(Zusi_Datenausgabe.PZBSystem.LZB80I80 , 2649);
		else
		{
			Zusi_Datenausgabe.PZBSystem sys;
			if (PZBTypes.TryGetValue(data.Value.ZugsicherungName, out sys))
				master.SendPZBAsInt(sys, 2649);
			else
				System.Console.WriteLine("Unknown PZB format: " + data.Value.ZugsicherungName);
		}
		
	}
	private static void On_SifaReceived(object sender, Zusi_Datenausgabe.DataSet<Zusi_Datenausgabe.Sifa> data)
	{
		master.SendBoolAsSingle(data.Value.OpticalReminderOn, 2596);
	}
	private static void On_NotbremssystemReceived(object sender, Zusi_Datenausgabe.DataSet<Zusi_Datenausgabe.Notbremssystem> data)
	{
		master.SendBoolAsSingle(data.Value.Notbremsung, 2606);
	}
	private static void On_DoorSystemReceived(object sender, Zusi_Datenausgabe.DataSet<Zusi_Datenausgabe.DoorSystem> data)
	{
		master.SendDoorsAsInt(data.Value.Zusi2State, 2646);
		master.SendBoolAsSingle(data.Value.Schalter_UnlockDoorsLeft || data.Value.Schalter_UnlockDoorsRight, 2607);
		master.SendBoolAsSingle(data.Value.Schalter_UnlockDoorsLeft || data.Value.Schalter_UnlockDoorsRight, 2627);
		master.SendBoolAsInt(data.Value.DoorSystemName != "", 2648);
		//System.Console.WriteLine(data.Value.DoorSystemName);
	}
}
