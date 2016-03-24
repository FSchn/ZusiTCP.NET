class ServertTest1 : Zusi_Datenausgabe.ZusiTcpServer
{
	public static void Main()
	{
		System.Console.WriteLine("TCP Server");
		var commands = Zusi_Datenausgabe.CommandSet.LoadFromFile("commandset_server.xml");
		Zusi_Datenausgabe.ZusiTcpServer server = new ServertTest1(commands, null);
		var listIds = new System.Collections.Generic.List<int>();
		foreach(var i in commands.CommandByID)
		{
			if ((i.Key == 2680)||(i.Key == 2681))
				continue;
			listIds.Add(i.Value.ID);
		}
		server.ReplaceAnywayRequested(listIds);
		System.Console.WriteLine("Set up Server successfull");
		server.Start(1435);
		while (!server.IsStarted) System.Threading.Thread.Sleep(100);
		System.Console.WriteLine("Starting successfull");
		server.OnError += delegate(object o, Zusi_Datenausgabe.ZusiTcpException ex) {
			//System.Console.WriteLine(ex.InnerException.Message);
			//System.Console.WriteLine(ex.StackTrace);
			System.Console.WriteLine(ex.ToString());
			//System.Console.WriteLine(ex.InnerException.StackTrace);
				};
		int numCon = 0;
		bool masterConnected = false;
		
		bool firstCancel = true;
		
		System.ConsoleCancelEventHandler shutDownServer = null;
		shutDownServer = delegate(object sender, System.ConsoleCancelEventArgs e)
		{
			if (server.IsStarted)
			{
				System.Console.Write("Shutting Down... ");
				server.Stop();
				if (firstCancel)
					e.Cancel = true;
				firstCancel = false;
				System.Console.WriteLine("OK!");
			}
		};
		
		System.Console.CancelKeyPress += shutDownServer;
		
		while (server.IsStarted)
		{
			System.Threading.Thread.Sleep(100);
			if ((numCon != server.Clients.Count)||((server.Master != null) != masterConnected))
				System.Console.WriteLine(string.Format("{0} Clients connected, Master {1}connected", 
					server.Clients.Count, (server.Master == null) ? "not " : ""));
			numCon = server.Clients.Count;
			masterConnected = (server.Master != null);
		}
		System.Console.WriteLine("Server shut down");
	}

	// Auf das Erben kann verzichtet werden, wenn man nicht kurz vor dem bekanntwerden des Clients noch was anfordern muss.
	public ServertTest1(Zusi_Datenausgabe.CommandSet commandsetDocument, System.Threading.SynchronizationContext hostContext)
		: base(commandsetDocument, hostContext)
	{
	}
	
	protected override void BeforeConnectingMaster(string masterId)
	{
		bool converter = masterId.ToLower().Contains("Converter".ToLower());
		bool zusi = masterId.ToLower().Contains("Zusi".ToLower()) && !converter;
		bool loksim = masterId.ToLower().Contains("Loksim".ToLower());
		bool unknown = (!converter && !zusi && !loksim);
		
		if (unknown)
			System.Console.WriteLine("Client Type not specific enaugh. Assume Oberstrom and Druck Zeitbehälter to be implemented.");
		else if (loksim)
			System.Console.WriteLine("Client Type Loksim. Assume Oberstrom to be implemented and Druck Zeitbehälter NOT implemented.");
		else if (zusi)
			System.Console.WriteLine("Client Type Zusi 2. Assume Oberstrom and Druck Zeitbehälter NOT to be implemented.");
		else if (converter)
			System.Console.WriteLine("Client Type Converter to Zusi 3. Assume Oberstrom and Druck Zeitbehälter to be implemented.");
		
		// Die beiden Speziellen IDs 2680 und 2681
		var listIds = new System.Collections.Generic.List<int>(AnywayRequested);
		if (converter || loksim || unknown)
			listIds.Add(2680);
		else
			listIds.Remove(2680);
		if (converter || unknown)
			listIds.Add(2681);
		else
			listIds.Remove(2681);
		
	}
}
