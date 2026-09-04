using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FeasaLedAnalyser;

namespace FeasaWpfDemo;

public partial class MainWindow : Window
{
    private FeasaClient? _client;
    private readonly ObservableCollection<string> _readings = new();

    public MainWindow()
    {
        InitializeComponent();
        ReadingsListBox.ItemsSource = _readings;
    }

    // ───────────────────────── Connect / disconnect ─────────────────────────

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_client != null)
        {
            DisconnectClient();
            return;
        }

        string port = string.IsNullOrWhiteSpace(PortTextBox.Text) ? "auto" : PortTextBox.Text.Trim();

        if (!int.TryParse(BaudTextBox.Text.Trim(), out int baud))
        {
            AppendLog("Invalid baud rate — enter a number, e.g. 57600.");
            return;
        }

        ConnectButton.IsEnabled = false;
        AppendLog($"Connecting on '{port}' at {baud} baud...");

        var client = new FeasaClient(port, baud);
        client.OnLog += AppendLog;

        bool connected;
        try
        {
            connected = await client.ConnectAsync();
        }
        catch (Exception ex)
        {
            AppendLog($"Connection threw an exception: {ex.Message}");
            client.OnLog -= AppendLog;
            ConnectButton.IsEnabled = true;
            return;
        }

        ConnectButton.IsEnabled = true;

        if (connected)
        {
            _client = client;
            SetConnectedUi(true);
            AppendLog("Connected.");
        }
        else
        {
            client.OnLog -= AppendLog;
            AppendLog("Failed to connect — check the port and that the analyser is powered on.");
        }
    }

    private void DisconnectClient()
    {
        if (_client == null) return;

        _client.OnLog -= AppendLog;
        _client.Disconnect();
        _client = null;

        SetConnectedUi(false);
        AppendLog("Disconnected.");
    }

    private void SetConnectedUi(bool connected)
    {
        ConnectButton.Content = connected ? "Disconnect" : "Connect";
        StatusText.Text = connected ? "Connected" : "Disconnected";
        StatusDot.Fill = connected ? new SolidColorBrush(Color.FromRgb(0x2E, 0x8B, 0x57))
                                    : new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B));
        CaptureButton.IsEnabled = connected;
        ReadButton.IsEnabled = connected;
    }

    // ───────────────────────── Capture ─────────────────────────

    private async void CaptureButton_Click(object sender, RoutedEventArgs e)
    {
        if (_client == null) return;

        CaptureButton.IsEnabled = false;
        AppendLog("Capturing...");
        try
        {
            await _client.CaptureAsync();
            AppendLog("Capture complete.");
        }
        catch (Exception ex)
        {
            AppendLog($"Capture failed: {ex.Message}");
        }
        finally
        {
            CaptureButton.IsEnabled = true;
        }
    }

    // ───────────────────────── Read measurements ─────────────────────────

    private async void ReadButton_Click(object sender, RoutedEventArgs e)
    {
        if (_client == null) return;

        ReadButton.IsEnabled = false;
        string selection = (MeasurementCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All measurements";

        try
        {
            switch (selection)
            {
                case "All measurements":
                    // Exercises every measurement method the library exposes, one after another.
                    await ReadAndAppend("RGBI", _client.GetRgbiAsync());
                    await ReadAndAppend("HSI", _client.GetHsiAsync());
                    await ReadAndAppend("xy", _client.GetXyAsync());
                    await ReadAndAppend("xyi", _client.GetXyiAsync());
                    await ReadAndAppend("CCT", _client.GetCctAsync());
                    await ReadAndAppend("UV", _client.GetUvAsync());
                    await ReadAndAppend("CIE XYZ", _client.GetCieXyzAsync());
                    await ReadAndAppend("Wavelength + Intensity", _client.GetWiAsync());
                    await ReadAndAppend("WSI", _client.GetWsiAsync());
                    await ReadAndAppend("Signal level", _client.GetSignalLevelAsync());
                    await ReadAndAppend("Wavelength", _client.GetWavelengthAsync());
                    await ReadAndAppend("Intensity", _client.GetIntensityAsync());
                    await ReadAndAppend("Absolute intensity", _client.GetAbsIntAsync());
                    break;

                case "RGBI": await ReadAndAppend("RGBI", _client.GetRgbiAsync()); break;
                case "HSI": await ReadAndAppend("HSI", _client.GetHsiAsync()); break;
                case "xy": await ReadAndAppend("xy", _client.GetXyAsync()); break;
                case "xyi": await ReadAndAppend("xyi", _client.GetXyiAsync()); break;
                case "CCT": await ReadAndAppend("CCT", _client.GetCctAsync()); break;
                case "UV": await ReadAndAppend("UV", _client.GetUvAsync()); break;
                case "CIE XYZ": await ReadAndAppend("CIE XYZ", _client.GetCieXyzAsync()); break;
                case "Wavelength + Intensity": await ReadAndAppend("Wavelength + Intensity", _client.GetWiAsync()); break;
                case "WSI": await ReadAndAppend("WSI", _client.GetWsiAsync()); break;
                case "Signal level": await ReadAndAppend("Signal level", _client.GetSignalLevelAsync()); break;
                case "Wavelength": await ReadAndAppend("Wavelength", _client.GetWavelengthAsync()); break;
                case "Intensity": await ReadAndAppend("Intensity", _client.GetIntensityAsync()); break;
                case "Absolute intensity": await ReadAndAppend("Absolute intensity", _client.GetAbsIntAsync()); break;
            }
        }
        catch (Exception ex)
        {
            AppendLog($"Read failed: {ex.Message}");
        }
        finally
        {
            ReadButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// Awaits any of the library's Get*Async() calls and appends each reading to the
    /// results list under a header. Uses each reading's own ToString() rather than
    /// reading individual fields, so this works the same way for every measurement
    /// type without needing to know its specific properties.
    /// </summary>
    private async Task ReadAndAppend<T>(string label, Task<List<T>> readTask)
    {
        List<T> results = await readTask;

        _readings.Add($"── {label} ──");

        if (results.Count == 0)
        {
            _readings.Add("(no readings returned)");
        }
        else
        {
            foreach (T reading in results)
                _readings.Add(reading?.ToString() ?? "(null)");
        }

        if (ReadingsListBox.Items.Count > 0)
            ReadingsListBox.ScrollIntoView(ReadingsListBox.Items[ReadingsListBox.Items.Count - 1]);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e) => _readings.Clear();

    // ───────────────────────── Logging ─────────────────────────

    private void AppendLog(string message)
    {
        void Append()
        {
            LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            LogTextBox.ScrollToEnd();
        }

        // FeasaClient may raise OnLog from a background thread; WPF UI elements can
        // only be touched from the UI thread, so marshal over if we're not on it.
        if (Dispatcher.CheckAccess())
            Append();
        else
            Dispatcher.Invoke(Append);
    }

    // ───────────────────────── Cleanup ─────────────────────────

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        DisconnectClient();
    }
}
