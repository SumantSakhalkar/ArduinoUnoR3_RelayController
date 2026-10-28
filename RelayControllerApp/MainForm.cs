using System.IO.Ports;

namespace RelayControllerApp;

public sealed class MainForm : Form
{
    private const int BaudRate = 9600;

    // Relay 1-4, in the same order as RELAY_PINS in RelayController.ino
    private static readonly string[] RelayNames = ["FLASH_MODE", "WATCHDOG", "JTAG_BOOT", "SPARE_4"];

    private readonly ComboBox _portBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
    private readonly Button _refreshButton = new() { Text = "Refresh", AutoSize = true };
    private readonly Button _connectButton = new() { Text = "Connect", AutoSize = true };
    private readonly Label[] _stateLabels = new Label[RelayNames.Length];
    private readonly ToolStripStatusLabel _statusText = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft, Text = "Not connected" };
    private readonly System.Windows.Forms.Timer _statusTimer = new() { Interval = 2500 };

    private SerialPort? _port;
    private string _rx = "";

    public MainForm()
    {
        Text = $"Arduino Relay Controller v{Application.ProductVersion}";
        Font = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 250);

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = false, Padding = new Padding(10) };
        top.Controls.Add(new Label { Text = "Port:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        top.Controls.Add(_portBox);
        top.Controls.Add(_refreshButton);
        top.Controls.Add(_connectButton);

        var rows = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(10, 0, 10, 0) };
        for (int i = 0; i < RelayNames.Length; i++)
        {
            int relay = i + 1;
            var nameLabel = new Label { Text = RelayNames[i], Width = 130, Height = 28, TextAlign = ContentAlignment.MiddleLeft };
            _stateLabels[i] = new Label
            {
                Text = "?",
                Width = 60,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.Gray,
                Font = new Font(Font, FontStyle.Bold),
                Margin = new Padding(0, 0, 8, 0)
            };
            var onButton = new Button { Text = "ON", AutoSize = true };
            var offButton = new Button { Text = "OFF", AutoSize = true };
            var toggleButton = new Button { Text = "Toggle", AutoSize = true };
            onButton.Click += (_, _) => Send($"ON {relay}");
            offButton.Click += (_, _) => Send($"OFF {relay}");
            toggleButton.Click += (_, _) => Send($"TOGGLE {relay}");

            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 0, 0, 6) };
            row.Controls.AddRange(new Control[] { nameLabel, _stateLabels[i], onButton, offButton, toggleButton });
            rows.Controls.Add(row);
        }

        var statusBar = new StatusStrip();
        statusBar.Items.Add(_statusText);

        // Docking order: controls added last dock first, so the top panels are added after the rows.
        Controls.Add(statusBar);
        Controls.Add(rows);
        Controls.Add(top);

        _refreshButton.Click += (_, _) => RefreshPorts();
        _connectButton.Click += (_, _) => ToggleConnection();
        _statusTimer.Tick += (_, _) =>
        {
            _statusTimer.Stop();
            Send("STATUS");
        };
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        RefreshPorts();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        Disconnect("Not connected");
        base.OnFormClosing(e);
    }

    private void RefreshPorts()
    {
        var ports = SerialPort.GetPortNames()
            .OrderBy(p => p.Length)
            .ThenBy(p => p, StringComparer.Ordinal)
            .ToArray();
        var current = _portBox.SelectedItem as string;

        _portBox.Items.Clear();
        _portBox.Items.AddRange(ports);
        if (current is not null && ports.Contains(current))
            _portBox.SelectedItem = current;
        else if (ports.Length > 0)
            _portBox.SelectedIndex = 0;
    }

    private void ToggleConnection()
    {
        if (_port is not null)
        {
            Disconnect("Not connected");
            return;
        }

        if (_portBox.SelectedItem is not string portName)
        {
            MessageBox.Show(this, "No serial port selected.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var port = new SerialPort(portName, BaudRate, Parity.None, 8, StopBits.One)
        {
            NewLine = "\n",
            ReadTimeout = 500,
            WriteTimeout = 500
        };
        port.DataReceived += OnDataReceived;

        try
        {
            port.Open();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            port.Dispose();
            MessageBox.Show(this, $"Could not open {portName}:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _port = port;
        _rx = "";
        _connectButton.Text = "Disconnect";
        _portBox.Enabled = false;
        _refreshButton.Enabled = false;
        SetStatus($"Connecting to {portName} (waiting for Uno reset)...");

        // The Uno resets when the port opens; request status once it has booted.
        _statusTimer.Start();
    }

    private void Disconnect(string message)
    {
        _statusTimer.Stop();
        var port = _port;
        _port = null;

        if (port is not null)
        {
            port.DataReceived -= OnDataReceived;
            try
            {
                if (port.IsOpen) port.Close();
            }
            catch (IOException)
            {
                // The device was already removed; nothing left to close.
            }
            port.Dispose();
        }

        foreach (var label in _stateLabels)
            SetState(label, null);

        _connectButton.Text = "Connect";
        _portBox.Enabled = true;
        _refreshButton.Enabled = true;
        SetStatus(message);
    }

    private void Send(string command)
    {
        if (_port is null || !_port.IsOpen) return;

        try
        {
            _port.WriteLine(command);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or TimeoutException)
        {
            Disconnect($"Write failed: {ex.Message}");
        }
    }

    // Runs on a background thread; complete lines are marshalled to the UI thread.
    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (sender is not SerialPort port) return;

        string data;
        try
        {
            data = port.ReadExisting();
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or ObjectDisposedException)
        {
            return;
        }

        _rx += data;
        int newline;
        while ((newline = _rx.IndexOf('\n')) >= 0)
        {
            string line = _rx[..newline].Trim();
            _rx = _rx[(newline + 1)..];
            if (line.Length == 0 || !IsHandleCreated || IsDisposed) continue;
            BeginInvoke((Action)(() => HandleLine(line)));
        }
    }

    private void HandleLine(string line)
    {
        if (_port is null) return;

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        switch (parts[0])
        {
            case "READY" when parts.Length >= 2:
                SetStatus($"Connected - {parts[1]} relay(s)");
                break;

            case "STATE" when parts.Length == 3
                              && int.TryParse(parts[1], out int n)
                              && n >= 1 && n <= _stateLabels.Length:
                bool on = parts[2] == "1";
                SetState(_stateLabels[n - 1], on);
                SetStatus($"Connected - {RelayNames[n - 1]} is {(on ? "ON" : "OFF")}");
                break;

            case "ERR":
                SetStatus($"Arduino: {line}");
                break;
        }
    }

    private static void SetState(Label label, bool? on)
    {
        if (on is null)
        {
            label.Text = "?";
            label.BackColor = Color.Gray;
        }
        else
        {
            label.Text = on.Value ? "ON" : "OFF";
            label.BackColor = on.Value ? Color.ForestGreen : Color.Firebrick;
        }
    }

    private void SetStatus(string text) => _statusText.Text = text;
}
