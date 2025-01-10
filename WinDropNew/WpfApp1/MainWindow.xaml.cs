using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Net;
using System.Net.Sockets;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WpfApp1;

public class ChatMessage
{
    public string Message { get; init; }
    public Brush StatusColor { get; set; }
    public string StatusText { get; set; }
    public ImageSource Image { get; init; }
    public string ImagePath { get; init; }
}

public partial class MainWindow
{
    private List<string> _filesToSend = new();

    private ObservableCollection<string> AvailableDevices { get; set; }
    private DispatcherTimer _scanTimer;
    private readonly Guid _serviceUuid = BluetoothService.SerialPort;

    private BluetoothListener _listener;
    private Dictionary<string, BluetoothClient> _connectedClients = new();

    public ObservableCollection<string> ConnectedDevices { get; }
    public ObservableCollection<ChatMessage> ChatMessages { get; }

    public MainWindow()
    {
        InitializeComponent();
        AvailableDevices = new ObservableCollection<string>();
        ConnectedDevices = new ObservableCollection<string>();
        ChatMessages = new ObservableCollection<ChatMessage>();

        AvailableDevicesListBox.ItemsSource = AvailableDevices;
        ConnectedDevicesListBox.ItemsSource = ConnectedDevices;
        ChatListBox.ItemsSource = ChatMessages;

        _scanTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        _scanTimer.Tick += async (_, _) => await ScanForBluetoothDevicesAsync();
        _scanTimer.Start();

        AvailableDevicesListBox.MouseDoubleClick += AvailableDevicesListBox_MouseDoubleClick;

        CheckConnectedDevices();
        StartBluetoothService();
    }

 
    private void CheckConnectedDevices()
    {
        try
        {
            var devices = new BluetoothClient().PairedDevices;

            foreach (var device in devices)
            {
                string deviceEntry = $"{device.DeviceName} - {device.DeviceAddress}";
                if (!ConnectedDevices.Contains(deviceEntry))
                {
                    ConnectedDevices.Add(deviceEntry);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fehler beim Überprüfen der verbundenen Geräte: {ex.Message}");
        }
    }


    private async void AvailableDevicesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (AvailableDevicesListBox.SelectedItem is not string selectedDevice) return;
            Debug.WriteLine($"Gerät ausgewählt: {selectedDevice}");
            await ConnectToDeviceAsync(selectedDevice);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fehler beim Verbinden mit dem Gerät: {ex.Message}");
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async Task ScanForBluetoothDevicesAsync()
    {
        try
        {
            Debug.WriteLine("Starting Bluetooth device scan...");
            var client = new BluetoothClient();
            var devices = new List<BluetoothDeviceInfo>();

            for (var attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    devices = await Task.Run(() => client.DiscoverDevices().ToList());
                    if (devices.Count > 0)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Scan versuch {attempt + 1} fehlgeschlagen: {ex.Message}");
                    await Task.Delay(1000);
                }
            }

            Debug.WriteLine($"{devices.Count} Geräte gefunden");

            Dispatcher.Invoke(() =>
            {
                AvailableDevices.Clear();
                foreach (BluetoothDeviceInfo device in devices)
                {
                    // "NAME - ADDRESSE"
                    var deviceEntry = $"{device.DeviceName} - {device.DeviceAddress}";
                    Debug.WriteLine($"Füge Gerät hinzu: {deviceEntry}");
                    if (!ConnectedDevices.Contains(deviceEntry))
                    {
                        AvailableDevices.Add(deviceEntry);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error scanning for devices: {ex.Message}");
            MessageBox.Show($"Error scanning for Bluetooth devices: {ex.Message}");
        }
    }

    private void StartBluetoothService()
    {
        try
        {
            _listener = new BluetoothListener(_serviceUuid);
            _listener.Start();
            Task.Run(() => AcceptBluetoothClientsAsync(_listener));
            Debug.WriteLine("Bluetooth-Dienst gestartet.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fehler beim Starten des Bluetooth-Dienstes: {ex.Message}");
            MessageBox.Show("Bluetooth-Dienst konnte nicht gestartet werden. Stellen Sie sicher, dass Bluetooth aktiviert ist.");
            System.Windows.Application.Current.Shutdown();
        }
    }

    private void ChatListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ChatListBox.SelectedItem is not ChatMessage selectedMessage) return;
        var imageZoomWindow = new ImageZoomWindow((BitmapImage)selectedMessage.Image);
        imageZoomWindow.Show();
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ChatMessage chatMessage } &&
            !string.IsNullOrEmpty(chatMessage.ImagePath))
        {
            string imagePath = chatMessage.ImagePath;
            Debug.WriteLine($"Versuche, den Pfad zu öffnen: {imagePath}");

            if (File.Exists(imagePath))
            {
                Process.Start("explorer.exe", $"/select, \"{imagePath}\"");
            }
            else
            {
                MessageBox.Show("Bildpfad ist ungültig oder die Datei existiert nicht.");
            }
        }
        else
        {
            MessageBox.Show("Ungültige Daten im Button oder ChatMessage.");
        }
    }

    private async Task AcceptBluetoothClientsAsync(BluetoothListener listener)
    {
        while (true)
        {
            try
            {
                var client = await Task.Run(() => listener.AcceptBluetoothClient());
                Debug.WriteLine("Bluetooth-Client verbunden.");

                var stream = client.GetStream();
                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                var receivedData = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (receivedData.EndsWith("EOF"))
                {
                    receivedData = receivedData.Substring(0, receivedData.Length - 3);
                }
                Debug.WriteLine($"Empfangene Daten: {receivedData}");
              

                Dispatcher.Invoke(() => ChatMessages.Add(new ChatMessage
                    { Message = $"{client.RemoteMachineName}: {receivedData}", StatusColor = Brushes.Green }));

                var uuidData = System.Text.Encoding.UTF8.GetBytes(_serviceUuid.ToString());
                await WriteDataWithRetryAsync(stream, uuidData);
                Debug.WriteLine("UUID an den Client gesendet.");

                // Starte Empfangen von Nachrichten
                _ = Task.Run(() => ReceiveMessagesAsync(client));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Akzeptieren des Bluetooth-Clients: {ex.Message}");
            }
        }
    }


    private void DragDropField_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        _filesToSend.AddRange(files);
        Debug.WriteLine($"Dateien hinzugefügt: {string.Join(", ", files)}");
    }

    private async void SendFilesButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "All Files (*.*)|*.*",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() != true) return;
        foreach (var filePath in openFileDialog.FileNames)
        {
            var chatMessage = new ChatMessage
            {
                Message = $"(Ich) {Path.GetFileName(filePath)}",
                StatusColor = Brushes.Yellow,
                StatusText = "Wird gesendet"
            };
            AddChatMessageWithAnimation(chatMessage);

            var sendTasks = ConnectedDevices.Select(device => SendFileToDeviceAsync(device, filePath))
                .ToArray();
            var results = await Task.WhenAll(sendTasks);

            bool allSuccess = results.All(success => success);
            chatMessage.StatusColor = allSuccess ? Brushes.Green : Brushes.Red;

            ChatMessages[ChatMessages.IndexOf(chatMessage)] = new ChatMessage
            {
                Message = chatMessage.Message,
                StatusColor = chatMessage.StatusColor
            };
        }
    }

    private async Task<bool> SendFileToDeviceAsync(string device, string filePath)
    {
        try
        {
            string[] deviceParts = device.Split([" - "], StringSplitOptions.None);
            string addressStr = deviceParts[1].Replace(":", "").ToUpper();
            var deviceAddress = BluetoothAddress.Parse(addressStr);

            if (!_connectedClients.TryGetValue(device, out var client) || !client.Connected)
            {
                client = new BluetoothClient();
                var endpoint = new BluetoothEndPoint(deviceAddress, BluetoothService.SerialPort);

                for (var attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        await Task.Run(() => client.Connect(endpoint));
                        if (!client.Connected) continue;
                        _connectedClients[device] = client;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Verbindungsversuch {attempt + 1} fehlgeschlagen: {ex.Message}");
                        await Task.Delay(500);
                    }
                }
            }

            if (client.Connected)
            {
                var stream = client.GetStream();
                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var fileName = Path.GetFileName(filePath);
                var identifier = System.Text.Encoding.UTF8.GetBytes($"WINDROP_FILE:{fileName}:");
                var dataWithIdentifier = new byte[identifier.Length + fileBytes.Length];
                Buffer.BlockCopy(identifier, 0, dataWithIdentifier, 0, identifier.Length);
                Buffer.BlockCopy(fileBytes, 0, dataWithIdentifier, identifier.Length, fileBytes.Length);

                await stream.WriteAsync(dataWithIdentifier, 0, dataWithIdentifier.Length);
                Debug.WriteLine($"Datei an {device} gesendet: {filePath}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fehler beim Senden der Datei an {device}: {ex.Message}");
        }
        return false;
    }


    private static string GetWinDropReceivedFolderPath()
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var winDropReceivedPath = Path.Combine(desktopPath, "WinDropReceived");

        if (!Directory.Exists(winDropReceivedPath))
        {
            Directory.CreateDirectory(winDropReceivedPath);
        }
        return winDropReceivedPath;
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is ChatMessage chatMessage &&
            !string.IsNullOrEmpty(chatMessage.ImagePath))
        {
            string filePath = chatMessage.ImagePath;
            Debug.WriteLine($"Versuche, den Pfad zu öffnen: {filePath}");

            if (File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show("Dateipfad ist ungültig oder die Datei existiert nicht.");
            }
        }
        else
        {
            MessageBox.Show("Ungültige Daten im Button oder ChatMessage.");
        }
    }

    private void DragDropField_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private void AddChatMessageWithAnimation(ChatMessage chatMessage)
    {
        ChatMessages.Add(chatMessage);
        ChatListBox.UpdateLayout();
        var listBoxItem = (ListBoxItem)ChatListBox.ItemContainerGenerator.ContainerFromItem(chatMessage);
        if (listBoxItem == null) return;
        listBoxItem.RenderTransform = new TranslateTransform();
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        var border = VisualTreeHelper.GetChild(ChatListBox, 0) as Border;
        if (border == null) return;
        var scrollViewer = border.Child as ScrollViewer;
        scrollViewer?.ScrollToEnd();
    }

    

    private async Task ReceiveMessagesAsync(BluetoothClient client)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];
            var memoryStream = new MemoryStream();
            bool isReceivingImage = false, isReceivingFile = false;
            string fileName = null;
            
            Debug.WriteLine("Starte Empfang von Nachrichten...");

            while (client.Connected)
            {
                Debug.WriteLine("Warten auf Daten vom Stream...");
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    Debug.WriteLine($"Bytes gelesen: {bytesRead}");
                    memoryStream.Write(buffer, 0, bytesRead);

                    if (!isReceivingImage && !isReceivingFile && IsFileData(memoryStream.ToArray()))
                    {
                        Debug.WriteLine("Dateisignatur erkannt. Behandle als Datei.");
                        isReceivingFile = true;

                        string data = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                        string[] parts = data.Split(new[] { ':' }, 3);

                        if (parts.Length >= 2 && parts[0] == "WINDROP_FILE")
                        {
                            fileName = parts[1];
                            byte[] fileData = memoryStream.ToArray()[(data.IndexOf(parts[2], StringComparison.Ordinal) + parts[2].Length)..];
                            memoryStream.SetLength(0);
                            memoryStream.Write(fileData, 0, fileData.Length);
                        }
                    }
                    else if (isReceivingFile && fileName != null)
                    {
                        Debug.WriteLine("Empfange Datei...");
                        var filePath = Path.Combine(GetWinDropReceivedFolderPath(), fileName);

                        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            memoryStream.Position = 0;
                            await memoryStream.CopyToAsync(fileStream);
                        }

                        var name = fileName;
                        Dispatcher.Invoke(() =>
                        {
                            AddChatMessageWithAnimation(new ChatMessage
                            {
                                Message = $"{client.RemoteMachineName} hat {name} geschickt",
                                StatusColor = Brushes.Green,
                                ImagePath = filePath
                            });
                        });

                        memoryStream.SetLength(0);
                        isReceivingFile = false;
                        fileName = null;
                    }
                    else if (!isReceivingImage && !isReceivingFile)
                    {
                        if (IsImageData(memoryStream.ToArray()))
                        {
                            Debug.WriteLine("Bildsignatur erkannt.");
                            isReceivingImage = true;
                        }
                        else
                        {
                            Debug.WriteLine("Behandle als Textnachricht.");
                            string message = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                            Debug.WriteLine($"Textnachricht: {message}");
                            if (message.EndsWith("EOF"))
                            {
                                message = message.Substring(0, message.Length - 3);
                            }
                            Debug.WriteLine($"Korrigierte Textnachricht : {message}");
                            Dispatcher.Invoke(() =>
                            {
                                AddChatMessageWithAnimation(new ChatMessage
                                {
                                    Message = $"{client.RemoteMachineName}: {message}",
                                    StatusColor = Brushes.Green
                                });
                            });

                            memoryStream.SetLength(0);
                        }
                    }

                    if (!isReceivingImage || !IsEndOfImageData(memoryStream.ToArray())) continue;
                    {
                        Debug.WriteLine("Bild vollständig empfangen. Verarbeite Bild...");
                        Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                var image = new BitmapImage();
                                using (var imageStream = new MemoryStream(memoryStream.ToArray()))
                                {
                                    image.BeginInit();
                                    image.CacheOption = BitmapCacheOption.OnLoad;
                                    image.StreamSource = imageStream;
                                    image.EndInit();
                                }

                                string filePath = Path.Combine(GetWinDropReceivedFolderPath(),
                                    $"{Guid.NewGuid()}.png");
                                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                                {
                                    memoryStream.WriteTo(fileStream);
                                }
                                
                                AddChatMessageWithAnimation(new ChatMessage
                                {
                                    Message = $"{client.RemoteMachineName} hat ein Bild geschickt"
                                });

                                AddChatMessageWithAnimation(new ChatMessage
                                {
                                     Image = image,
                                    StatusColor = Brushes.Green,
                                    ImagePath = filePath
                                });

                                Debug.WriteLine(
                                    $"Bild erfolgreich in den Chat eingefügt und unter {filePath} gespeichert.");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Fehler beim Verarbeiten des Bildes: {ex.Message}");
                            }
                        });

                        memoryStream.SetLength(0);
                        isReceivingImage = false;
                    }
                }
                else
                {
                    Debug.WriteLine("Keine Daten mehr verfügbar, Client könnte getrennt sein.");
                    var uuidData = System.Text.Encoding.UTF8.GetBytes(_serviceUuid.ToString());
                    await WriteDataWithRetryAsync(stream, uuidData);
                    Debug.WriteLine("UUID an den Client gesendet um verbindung wiederherzustellen.");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fehler beim Empfangen der Nachricht: {ex.Message}");
        }
        finally
        {
            Debug.WriteLine("Nachrichtenempfang beendet.");
        }
    }

    private static bool IsFileData(byte[] data)
    {
        if (data.Length < 16)
        {
            Debug.WriteLine("Zu wenige Daten, um Dateisignatur zu prüfen.");
            return false;
        }

        var prefix = "WINDROP_FILE:";
        var prefixBytes = System.Text.Encoding.UTF8.GetBytes(prefix);
        if (prefixBytes.Where((t, i) => data[i] != t).Any())
        {
            return false;
        }

        Debug.WriteLine("Dateisignatur erkannt.");
        return true;
    }

    private bool IsEndOfImageData(byte[] data)
    {
        switch (data.Length)
        {
            case > 2 when data[^2] == 0xFF && data[^1] == 0xD9:
                Debug.WriteLine("JPG-Dateiende erkannt.");
                return true;
            case > 8 when data[^8] == 0x49 && data[^7] == 0x45 &&
                          data[^6] == 0x4E && data[^5] == 0x44 &&
                          data[^4] == 0xAE && data[^3] == 0x42 &&
                          data[^2] == 0x60 && data[^1] == 0x82:
                Debug.WriteLine("PNG-Dateiende erkannt.");
                return true;
            default:
                return false;
        }
    }

    private static bool IsImageData(byte[] data)
    {
        if (data.Length < 8)
        {
            Debug.WriteLine("Zu wenige Daten, um Bildsignatur zu prüfen.");
            return false;
        }

        if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
            data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
        {
            Debug.WriteLine("PNG-Dateisignatur erkannt.");
            return true;
        }

        if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            Debug.WriteLine("JPG-Dateisignatur erkannt.");
            return true;
        }

        Debug.WriteLine("Keine bekannte Bildsignatur erkannt.");
        return false;
    }


    private static async Task WriteDataWithRetryAsync(NetworkStream stream, byte[] data, int retryCount = 3)
    {
        for (var i = 0; i < retryCount; i++)
        {
            try
            {
                await stream.WriteAsync(data, 0, data.Length);
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Senden der Daten, Versuch {i + 1} von {retryCount}: {ex.Message}");
                if (i == retryCount - 1)
                {
                    throw;
                }

                await Task.Delay(1000);
            }
        }
    }

    private async void SelectAndSendImage_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg"
        };

        if (openFileDialog.ShowDialog() != true) return;
        var filePath = openFileDialog.FileName;
        var image = new BitmapImage(new Uri(filePath));

        var chatMessage = new ChatMessage
        {
            Image = image,
            StatusColor = Brushes.Yellow,
            StatusText = "Wird gesendet"
        };
        AddChatMessageWithAnimation(chatMessage);

        var sendTasks = ConnectedDevices.Select(device => SendImageToDeviceAsync(device, filePath)).ToArray();
        var results = await Task.WhenAll(sendTasks);

        bool allSuccess = results.All(success => success);
        chatMessage.StatusColor = allSuccess ? Brushes.Green : Brushes.Red;

        ChatMessages[ChatMessages.IndexOf(chatMessage)] = new ChatMessage
        {
            Image = chatMessage.Image,
            StatusColor = chatMessage.StatusColor
        };
    }

    private async Task<bool> SendImageToDeviceAsync(string device, string filePath)
    {
        try
        {
            var deviceParts = device.Split([" - "], StringSplitOptions.None);
            var addressStr = deviceParts[1].Replace(":", "").ToUpper();
            var deviceAddress = BluetoothAddress.Parse(addressStr);

            if (!_connectedClients.TryGetValue(device, out var client) || !client.Connected)
            {
                client = new BluetoothClient();
                var endpoint = new BluetoothEndPoint(deviceAddress, BluetoothService.SerialPort);

                for (var attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        await Task.Run(() => client.Connect(endpoint));
                        if (!client.Connected) continue;
                        _connectedClients[device] = client;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Verbindungsversuch {attempt + 1} fehlgeschlagen: {ex.Message}");
                        await Task.Delay(500);
                    }
                }
            }

            if (client.Connected)
            {
                var stream = client.GetStream();
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                await stream.WriteAsync(fileBytes, 0, fileBytes.Length);
                Debug.WriteLine($"Bild an {device} gesendet: {filePath}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fehler beim Senden des Bildes an {device}: {ex.Message}");
        }

        return false;
    }


    private async Task ConnectToDeviceAsync(string selectedDevice)
    {
        try
        {
            var deviceParts = selectedDevice.Split(new[] { " - " }, StringSplitOptions.None);
            var deviceName = deviceParts[0];
            var addressStr = deviceParts[1].Replace(":", "").ToUpper();

            using var discoveryClient = new BluetoothClient();
            var devices = await Task.Run(() => discoveryClient.DiscoverDevices(5));
            var device = devices.FirstOrDefault(d => d.DeviceAddress.ToString() == addressStr);

            if (device == null)
            {
                throw new Exception("Device not in range. Please ensure it's powered on and discoverable.");
            }

            device.Refresh();
            device.SetServiceState(BluetoothService.SerialPort, true);
            await Task.Delay(1000);

            var services = await device.GetRfcommServicesAsync();
            if (!services.Contains(BluetoothService.SerialPort))
            {
                throw new Exception("Device does not support Serial Port Profile.");
            }

            using var client = new BluetoothClient();
            var endpoint = new BluetoothEndPoint(device.DeviceAddress, BluetoothService.SerialPort);

            await Task.Run(() =>
            {
                client.Connect(endpoint);
                if (client.Connected)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ConnectedDevices.Add(selectedDevice);
                        AvailableDevices.Remove(selectedDevice);
                    });
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Connection error: {ex.Message}");
            MessageBox.Show($"Failed to connect: {ex.Message}");
        }
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ChatInputTextBox.Text)) return;
        var message = ChatInputTextBox.Text;
        var chatMessage = new ChatMessage
        {
            Message = $"(Ich): {message}",
            StatusColor = Brushes.Yellow,
            StatusText = "Wird gesendet"
        };
        AddChatMessageWithAnimation(chatMessage);
        ChatInputTextBox.Clear();

        var sendTasks = ConnectedDevices.Select(device => SendMessageToDeviceAsync(device, message)).ToArray();
        var results = await Task.WhenAll(sendTasks);

        var allSuccess = results.All(success => success);
        chatMessage.StatusColor = allSuccess ? Brushes.Green : Brushes.Red;

        ChatMessages[ChatMessages.IndexOf(chatMessage)] = new ChatMessage
        {
            Message = chatMessage.Message,
            StatusColor = chatMessage.StatusColor
        };
    }


    private async Task<bool> SendMessageToDeviceAsync(string device, string message)
    {
        try
        {
            var deviceParts = device.Split(new[] { " - " }, StringSplitOptions.None);
            var addressStr = deviceParts[1].Replace(":", "").ToUpper();
            var deviceAddress = BluetoothAddress.Parse(addressStr);

            if (!_connectedClients.TryGetValue(device, out var client) || !client.Connected)
            {
                client = new BluetoothClient();
                var endpoint = new BluetoothEndPoint(deviceAddress, BluetoothService.SerialPort);

                for (int attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        await Task.Run(() => client.Connect(endpoint));
                        if (!client.Connected) continue;
                        _connectedClients[device] = client;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Verbindungsversuch {attempt + 1} fehlgeschlagen: {ex.Message}");
                        await Task.Delay(500);
                    }
                }
            }

            if (client.Connected)
            {
                var stream = client.GetStream();
                var messageWithEOF = message + "EOF";
                var messageBytes = System.Text.Encoding.UTF8.GetBytes(messageWithEOF);
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                Debug.WriteLine($"Nachricht an {device} gesendet: {messageWithEOF}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fehler beim Senden der Nachricht an {device}: {ex.Message}");
        }

        return false;
    }
}