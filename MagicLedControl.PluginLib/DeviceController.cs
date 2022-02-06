using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MagicLedControl.PluginLib
{
    public class DeviceController
    {
        public TcpClient client = new TcpClient();
        public NetworkStream? stream;
        string Address = "";
        static int Port = 5577;
        public event Notif messageSent;
        public event ColorUpdate colorUpdated;
        public delegate void Notif();
        public delegate void ColorUpdate(Color color);

        public async Task<bool> Connect(string server)
        {
            Disconnect();
            try
            {
                //Initialize the connection with the device;
                Address = server;
                client = new TcpClient();
                await client.ConnectAsync(server, Port);
                stream = client.GetStream();
                return true;

            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(("ArgumentNullException: {0}", e));
                Trace.WriteLine(("ArgumentNullException: {0}", e.Message));
                return false;
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                Trace.WriteLine("SocketException: {0}", e.Message);
                return false;
            }

            //Console.WriteLine("\n Press Enter to continue...");
            //Console.Read();
        }

        public void Disconnect()
        {
            if (client != null && client.Connected)
            {
                try
                {
                    //stream.WriteAsync(new byte[1] {0x0});
                    if (stream != null)
                    {
                        stream.Flush();
                        stream.Close();
                    }
                    client.Close();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
        }

        public static async Task<(bool, string)> PingDevice(string address)//Returns true if it is a LED controller
        {
            return await new Pinger(address).Ping();
        }

        public void Send(string message)
        {
            if (client.Connected && client != null && stream != null)
            {
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                byte[] dataWithChecksum = new byte[data.Length + 1];
                data.CopyTo(dataWithChecksum, 0);
                dataWithChecksum[dataWithChecksum.Length - 1] = MagicUtils.CalculateChecksum(data); //Always last byte of the message is a checksum
                //stream.Write(dataWithChecksum, 0, dataWithChecksum.Length);
                Trace.WriteLine(("Sent: {0}", message));
            }
        }

        public async Task<bool> Send(byte[] message)
        {
            if (client.Connected && client != null && stream != null)
            {
                byte[] dataWithChecksum = new byte[message.Length + 1];
                message.CopyTo(dataWithChecksum, 0);
                dataWithChecksum[dataWithChecksum.Length - 1] = MagicUtils.CalculateChecksum(message); //Always last byte of the message is a checksum

                await stream.WriteAsync(dataWithChecksum, 0, dataWithChecksum.Length);

                Trace.WriteLine(($"Sent: {BitConverter.ToString(dataWithChecksum)}"));
                messageSent?.Invoke();
                return true;
            }
            else
                return false;
        }

        public async /*Task<System.Threading.CancellationToken>*/ Task SetPowerState(bool isOn)
        {
            bool powerResult = false;
            if (isOn)
            {
                powerResult = await Send(Commands.TURN_ON);
            }
            else
            {
                powerResult = await Send(Commands.TURN_OFF);
            }

            if (!powerResult) return;

            if (stream != null)
            {
                stream.ReadTimeout = 500;
                var data = new byte[4];

                await stream.ReadAsync(data);
                Trace.WriteLine("Received: " + BitConverter.ToString(data) + " after changing the power state");
            }
            return;
        }

        public async void SetColor(Color color)
        {
            byte[] message = new byte[7];
            Commands.SET_COLOR.CopyTo(message, 0);
            message[1] = color.R;
            message[2] = color.G;
            message[3] = color.B;
            if(colorUpdated != null)
                colorUpdated.Invoke(color);

            await Send(message);
            //await Task.Delay(200);
            //await Send(message);
        }

        async public Task<MagicStructs.Controller> RequestControllerData()
        {
            try
            {
                if (stream != null)
                    await stream.FlushAsync();
                if (client == null || !client.Connected)
                {
                    return null;
                    //try
                    //{
                    //    await Connect(Address);
                    //}
                    //catch (Exception ex)
                    //{
                    //    Trace.WriteLine(ex);
                    //    return null;
                    //}
                    //if (client != null && client.Connected)
                    //    stream = client.GetStream();
                    //else
                    //    return null;
                }

                var res = await Send(Commands.REQUEST_DATA);

                if (!res)
                    return null;

                await Task.Delay(200);

                var data = new byte[14];
                Int32 bytes = await stream.ReadAsync(data, 0, data.Length);

                var controller = new MagicStructs.Controller();
                Color dataColor;
                dataColor.R = data[6];
                dataColor.G = data[7];
                dataColor.B = data[8];
                controller.currentColor = dataColor;
                controller.powerState = data[2] == 0x23 ? true : false;
                controller.modelVersion = data[1];
                controller.firmwareVersion = data[10];
                controller.functionSpeed = 32 - Convert.ToInt32(data[5]);
                controller.functionMode = data[3]; //0x61 - static color, anything elese can be function, but its random, or at least seems like it is.

                //string responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Trace.WriteLine(("Received: {0}", BitConverter.ToString(data)));
                return controller;
            }
            catch (Exception ex)
            {
                return null;
                Trace.WriteLine(ex.Message);
            }
        }

        public void SetCustomFunction(MagicStructs.Function customFunc)
        {
            var byteColors = new byte[16];
            foreach (Color color in customFunc.colors)
            {
                //TO BE IMPLEMENTED!
            }
        }

        private class Pinger
        {
            public string address;
            private static int UDP_PORT = 48899;
            private UdpClient udpClient;
            private IPEndPoint udpEndPoint;

            public Pinger(string address)
            {
                this.address = address;

                udpClient = new UdpClient(address, UDP_PORT);//This line establishes the connection
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                Trace.WriteLine("Commecting to: " + address);
                udpEndPoint = new IPEndPoint(IPAddress.Parse(address), UDP_PORT);
            }

            public async Task<(bool, string)> Ping()
            {
                try
                {
                    if (!udpClient.Client.Connected) return (false, "");
                    var message = System.Text.Encoding.Default.GetBytes(Commands.DISCOVERY_MESSAGE);

                    udpClient.Send(message, message.Length);
                    udpClient.Client.ReceiveTimeout = 800;//works for me, but with shitty wifi or something might not be so good
                    byte[] data = udpClient.Receive(ref udpEndPoint); //NOT ASYNC
                    string response = System.Text.Encoding.ASCII.GetString(data, 0, data.Length);
                    //Trace.WriteLine("Message received:" + response);

                    udpClient.Close();

                    if (response.Contains("AK001-ZJ2101"))
                    {
                        var macAddress = response.Split(',')[1];
                        Trace.WriteLine($"Mac address: {macAddress}");
                        return (true, macAddress);
                    }

                    return (false, "");
                }
                catch (Exception ex)
                {
                    udpClient.Close();
                    //Trace.WriteLine(ex.Message);
                    return (false, "");
                }
            }
        }
    }
}
