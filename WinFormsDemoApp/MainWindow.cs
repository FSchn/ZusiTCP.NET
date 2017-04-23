using System;
using System.Threading;
using System.Windows.Forms;
using ZusiTcpInterface;
using ZusiTcpInterface.Enums;

namespace WinFormsDemoApp
{
  public partial class MainWindow : Form
  {
    private readonly ConnectionCreator _connectionCreator;
    private ThreadMarshallingZusiDataReceiver _dataReceiver;
    private Connection _connection;
    private string _fileNameBuffer;
    private string _descriptionBuffer;
    private System.Collections.Generic.List<string> _vehiclesBuffer;

    public MainWindow()
    {
      InitializeComponent();

      _connectionCreator = new ConnectionCreator();
      _connectionCreator.NeededData.Request("Geschwindigkeit", "LM Getriebe", "Status Sifa-Leuchtmelder", "Status Zugverband", "Status Sifa-Hupe");
    }

    private void OnGearboxPilotLightReceived(DataChunk<bool> dataChunk)
    {
      lblGearboxIndicator.Text = dataChunk.Payload.ToString();
    }

    private void OnSifaPilotLightReceived(DataChunk<bool> dataChunk)
    {
      lblSifaStatus.Text = dataChunk.Payload.ToString();
    }

    private void OnVelocityReceived(DataChunk<float> dataChunk)
    {
      lblVelocity.Text = String.Format("{0:F1}", dataChunk.Payload * 3.6f);
    }

    private void OnSifaHornReceived(DataChunk<StatusSifaHupe> dataChunk)
    {
      lblSifaHorn.Text = dataChunk.Payload.ToString();
    }
    
    private void OnZugReceived(DataChunk<bool> dataChunk)
    {
      if (dataChunk.Payload)
        _vehiclesBuffer = null;
      else
      {
        _vehiclesBuffer.Add(string.Format("{0} ({1})", _descriptionBuffer, _fileNameBuffer));
        lblVehicles.Text = string.Join(System.Environment.NewLine, _vehiclesBuffer.ToArray());
      }
      _fileNameBuffer = string.Empty;
      _descriptionBuffer = string.Empty;
    }
    private void OnZugFzgReceived(DataChunk<bool> dataChunk)
    {
      if (_vehiclesBuffer == null)
        _vehiclesBuffer = new System.Collections.Generic.List<string>();
      else
        _vehiclesBuffer.Add(string.Format("{0} ({1})", _descriptionBuffer, _fileNameBuffer));
      _fileNameBuffer = string.Empty;
      _descriptionBuffer = string.Empty;
    }
    private void OnZugFzgFileNameReceived(DataChunk<string> dataChunk)
    {
      _fileNameBuffer = dataChunk.Payload;
    }
    private void OnZugFzgDescriptionReceived(DataChunk<string> dataChunk)
    {
      _descriptionBuffer = dataChunk.Payload;
    }

    private void MainWindow_Load(object sender, EventArgs e)
    {
      lblConnecting.Text = "Connecting!";

      _connection = _connectionCreator.CreateConnection();

      _dataReceiver = new ThreadMarshallingZusiDataReceiver(_connectionCreator.Descriptors, _connection.ReceivedDataChunks, SynchronizationContext.Current);
      _dataReceiver.RegisterCallbackFor<bool>("LM Getriebe", OnGearboxPilotLightReceived);
      _dataReceiver.RegisterCallbackFor<bool>("Status Sifa-Leuchtmelder", OnSifaPilotLightReceived);
      _dataReceiver.RegisterCallbackFor<StatusSifaHupe>("Status Sifa-Hupe", OnSifaHornReceived);
      _dataReceiver.RegisterCallbackFor<float>("Geschwindigkeit", OnVelocityReceived);
      
      _dataReceiver.RegisterCallbackFor<bool>("Status Zugverband", OnZugReceived);
      _dataReceiver.RegisterCallbackFor<bool>("Status Zugverband:Fahrzeug", OnZugFzgReceived);
      _dataReceiver.RegisterCallbackFor<string>("Status Zugverband:Fahrzeug:Dateiname", OnZugFzgFileNameReceived);
      _dataReceiver.RegisterCallbackFor<string>("Status Zugverband:Fahrzeug:Beschreibung", OnZugFzgDescriptionReceived);

      lblConnecting.Text = "Connected!";
    }

    private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
    {
      _dataReceiver.Dispose();
      _connection.Dispose();
    }
  }
}