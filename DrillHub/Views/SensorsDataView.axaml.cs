using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace DrillHub.Views
{

    public partial class SensorsDataView : UserControl
    {
        SimpleTcpServer tcpServerConsumer, tcpServerProducer;
        SimpleTcpClient tcpClientConsumer;
        string serverConsumerPortnumber;

        public bool dataReceivingIsRun, clientIsConnected, consumerIsConnected;

        public SensorsDataView()
        {
            InitializeComponent();

            tcpClientConsumer = new SimpleTcpClient(TxtBxServerIp.Text, int.Parse(TxtBxPort.Text));
            tcpClientConsumer.Events.Connected += tcpClientConsumer_ConnectedToServer;
            tcpClientConsumer.Events.DataReceived += tcpClientConsumer_ClientDataReceived;
            tcpClientConsumer.Events.Disconnected += tcpClientConsumer_DisconnectedFromServer;

            tcpServerConsumer = new SimpleTcpServer("127.0.0.1", int.Parse(TxtBxPort.Text));
            tcpServerConsumer.Events.ClientConnected += tcpServerConsumer_ProduceClientConnected;
            tcpServerConsumer.Events.DataReceived += tcpServerConsumer_DataReceived;
            tcpServerConsumer.Events.ClientDisconnected += tcpServerConsumer_ProduceClientDisconnected;

            tcpServerProducer = new SimpleTcpServer("127.0.0.1", int.Parse(TxtBxServerPort.Text));
            tcpServerProducer.Events.ClientConnected += tcpServerProducer_ClientConnected;
            tcpServerProducer.Events.ClientDisconnected += tcpServerProducer_ClientDisconnected;
        }

        //TCPClient Events
        private void tcpClientConsumer_ConnectedToServer(object? sender, ConnectionEventArgs e)
        {            
            clientIsConnected = true;

            Dispatcher.UIThread.Post(() =>
            {
                btnStop.IsEnabled = false;
                ConnectionProgress.IsVisible = false;
                ConnectionProgress.IsIndeterminate = false;
                StatusIndicator.Value = 100;
                txtBlkStatus.Text = "Источник подключен";
                btnStop.IsEnabled = true;
                txtBlkStatus.Foreground = Brush.Parse("#0070c0");
                serverConsumerPortnumber = TxtBxPort.Text;
            });
        }
        private void tcpClientConsumer_ClientDataReceived(object? sender, DataReceivedEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (LogPane.IsPaneOpen)
                {
                    Dispatcher.UIThread.Post(() => txtBlckLog.Inlines?.Add(new Run($"{ Encoding.UTF8.GetString(e.Data) }\n")));

                    if (txtBlckLog.Inlines?.Count > 100)
                    {
                        txtBlckLog.Inlines?.RemoveAt(0);
                    }
                    scrllVwrLog?.ScrollToEnd();
                }
            });
        }
        private async void tcpClientConsumer_DisconnectedFromServer(object? sender, ConnectionEventArgs e)
        {
            if (dataReceivingIsRun)
            {
                clientIsConnected = false;
                Dispatcher.UIThread.Post(() =>
                {
                    StatusIndicator.Value = 0;
                    ConnectionProgress.IsIndeterminate = true;
                    ConnectionProgress.IsVisible = true;
                    txtBlkStatus.Text = "Попытка восстановть подключение";
                    txtBlkStatus.Foreground = Brush.Parse("#5a666d");
                });

                while (dataReceivingIsRun && !clientIsConnected)
                {
                    try
                    {
                        await Task.Run(async () =>
                        {
                            tcpClientConsumer.ConnectWithRetries(4000);
                            clientIsConnected = true;
                        });
                    }
                    catch
                    {

                    }
                }
            }

            if (!dataReceivingIsRun)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    WITSDataVidget.IsEnabled = true;
                    btnStart.IsEnabled = true;
                    btnStop.IsEnabled = false;

                    StatusIndicator.Value = 0;
                    txtBlkStatus.Text = "Источник не подключен";
                    txtBlkStatus.Foreground = Brush.Parse("#5a666d");
                });
            }
        }

        //TCPServer Events
        private void tcpServerConsumer_ProduceClientConnected(object? sender, ConnectionEventArgs e)
        {

            Dispatcher.UIThread.Post(() =>
            {
                ConnectionProgress.IsVisible = false;
                ConnectionProgress.IsIndeterminate = false;
                StatusIndicator.Value = 100;
                txtBlkStatus.Text = "Источник подключен";
                txtBlkStatus.Foreground = Brush.Parse("#0070c0");
            });

        }
        private void tcpServerConsumer_DataReceived(object? sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (LogPane.IsPaneOpen)
                {
                    Dispatcher.UIThread.Post(() => txtBlckLog.Inlines?.Add(new Run($"{Encoding.UTF8.GetString(e.Data)}\n")));

                    if (txtBlckLog.Inlines?.Count > 100)
                    {
                        txtBlckLog.Inlines?.RemoveAt(0);
                    }
                    scrllVwrLog?.ScrollToEnd();
                }
            });
        }
        private void tcpServerConsumer_ProduceClientDisconnected(object? sender, ConnectionEventArgs e)
        {
            if (dataReceivingIsRun)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtBlkStatus.Text = "Ожидание подключения источника";
                    ConnectionProgress.IsIndeterminate = true;
                    ConnectionProgress.IsVisible = true;
                });
            }

            if (!dataReceivingIsRun)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    WITSDataVidget.IsEnabled = true;
                    btnStart.IsEnabled = true;
                    btnStop.IsEnabled = false;

                    StatusIndicator.Value = 0;
                    txtBlkStatus.Text = "Источник не подключен";
                    txtBlkStatus.Foreground = Brush.Parse("#5a666d");
                });
            }
        }

        //TCPServerProducer
        private void tcpServerProducer_ClientConnected(object? sender, ConnectionEventArgs e)
        {
            consumerIsConnected = true;
        }
        private void tcpServerProducer_ClientDisconnected(object? sender, ConnectionEventArgs e)
        {

        }

        private async void btnStart_OnClick(object? sender, RoutedEventArgs args)
        {
            dataReceivingIsRun = true;

            WITSDataVidget.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;

            if (IsServer.IsChecked == true)
            {
                btnStop.IsEnabled = false;
                ConnectionProgress.IsIndeterminate = true;
                ConnectionProgress.IsVisible = true;
                txtBlkStatus.Text = "Поиск источника";

                try
                {
                    await Task.Run(async () =>
                    {
                        tcpClientConsumer.ConnectWithRetries(4000);
                    });
                }
                catch { }


                if (!tcpClientConsumer.IsConnected)
                {
                    ConnectionProgress.IsVisible = false;
                    ConnectionProgress.IsIndeterminate = false;

                    WITSDataVidget.IsEnabled = true;
                    btnStart.IsEnabled = true;
                    btnStop.IsEnabled = false;

                    StatusIndicator.Value = 0;
                    txtBlkStatus.Text = "Источник не подключен";
                    txtBlkStatus.Foreground = Brush.Parse("#5a666d");
                }
            }
            if (IsClient.IsChecked == true)
            {
                txtBlkStatus.Text = "Ожидание подключения источника";
                ConnectionProgress.IsIndeterminate = true;
                ConnectionProgress.IsVisible = true;
                tcpServerConsumer.Start();
            }

            SocketServerWITSSender();
        }

        private void btnStop_OnClick(object? sender, RoutedEventArgs args)
        {
            dataReceivingIsRun = false;

            if (IsClient.IsChecked == true)
            {
                tcpServerConsumer.Stop();
                tcpServerConsumer.Dispose();
            }

            if (IsServer.IsChecked == true)
            {
                tcpClientConsumer.Disconnect();
                tcpClientConsumer.Dispose();
            }

            ConnectionProgress.IsIndeterminate = false;
            ConnectionProgress.IsVisible = false;
            WITSDataVidget.IsEnabled = true;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            txtBlkStatus.Text = "Источник не подключен";
            txtBlkStatus.Foreground = Brush.Parse("#5a666d");

            if (tcpServerProducer.IsListening)
            {
                //tcpServerProducer.Stop();
            }
        }


        private void TglSwchServer_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (TglSwchServer.IsChecked == true)
            {
                TxtBxServerPort.IsEnabled = true;
            }

            if (TglSwchServer.IsChecked == false)
            {
                TxtBxServerPort.IsEnabled = false;
            }

        }

        private void IsServer_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (IsServer?.IsChecked == true)
            {
                TxtBxServerIp.IsEnabled = true;
                TxtBxPort.IsEnabled = true;
            }
            if (IsClient?.IsChecked == true)
            {
                TxtBxServerIp.IsEnabled = false;
                TxtBxPort.IsEnabled = true;
            }
        }

        public async void ShowInputDialogAsync(object? sender, RoutedEventArgs args)
        {
            var dialog = new ContentDialog()
            {
                Title = "Выберите тип источника данных",
                PrimaryButtonText = "Ok",
                CloseButtonText = "Отмена"
            };

            dialog.IsPrimaryButtonEnabled = false;
            
            dialog.Content = new SensorDataTransferTypeDialog()
            {

            };

            var result = await dialog.ShowAsync();
        }

        Queue<ArraySegment<byte>> msgToSendQueu = new Queue<ArraySegment<byte>>();

        bool SocketConnected(Socket s)
        {
            bool notPoll = s.Poll(1000, SelectMode.SelectRead);
            bool notAvailable = (s.Available == 0);
            if (notPoll && notAvailable)
                return false;
            else
                return true;
        }

        private void btnShowLog_OnClick(object? sender, RoutedEventArgs args)
        {
            LogPane.IsPaneOpen = !LogPane.IsPaneOpen;
        }

        private Socket serverConsumer, clientConsumer, serverProducer;
        
        private byte[] buffer = new byte[1024];

        public async Task SocketServerWITSSender()
        {
            IPEndPoint witsServer = new IPEndPoint(IPAddress.Any, int.Parse("10003"));

            Socket serverSender = new(
                witsServer.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
                );

            serverSender.Bind(witsServer);
            serverSender.Listen(10);

            Task ResenderConnect = Task.Factory.StartNew(async () =>
            {
                var handler = await serverSender.AcceptAsync();

                while (SocketConnected(handler))
                {
                    if (msgToSendQueu.Count > 0)
                    {
                        try
                        {
                            handler.SendAsync(msgToSendQueu.Dequeue());
                        }
                        catch{ }
                    }
                }
            });

            ResenderConnect.Start();
        }

        private void MenuFlyoutItem_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            
        }

        private void ipValidation(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            IPAddress address;
            if (IPAddress.TryParse(TxtBxServerIp.Text, out address))
            {
                btnStart.IsEnabled = true;
                TxtBxServerIp.BorderBrush = Brush.Parse("#343434");
            }
            else
            {
                btnStart.IsEnabled = false;
                TxtBxServerIp.BorderBrush = Brush.Parse("#ff0000");
            }
        }

        private void portValidation(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            int port;
            if (int.TryParse(TxtBxPort.Text, out port))
            {
                btnStart.IsEnabled = true;
                TxtBxPort.BorderBrush = Brush.Parse("#343434");
            }
            else
            {
                btnStart.IsEnabled = false;
                TxtBxPort.BorderBrush = Brush.Parse("#ff0000");
            }

        }

        private void serverPortValidation(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            int port;
            if (int.TryParse(TxtBxServerPort.Text, out port))
            {
                btnStart.IsEnabled = true;
                TxtBxServerPort.BorderBrush = Brush.Parse("#343434");
            }
            else
            {
                btnStart.IsEnabled = false;
                TxtBxServerPort.BorderBrush = Brush.Parse("#ff0000");
            }

        }
    }
}
