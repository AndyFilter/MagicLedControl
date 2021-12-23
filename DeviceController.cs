using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Media;
using System.Net;

namespace MagicLedControl
{
    public class DeviceController
    {
        public TcpClient client = new TcpClient();
        public NetworkStream stream;
        string Address = "";
        int Port = 5577;

        public async Task Connect(String server, String message)
        {
            try
            {
                //Initialize the connection with the device;
                Address = server;
                await client.ConnectAsync(server, Port);
                stream = client.GetStream();

            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(("ArgumentNullException: {0}", e));
                Trace.WriteLine(("ArgumentNullException: {0}", e.Message));
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                Trace.WriteLine("SocketException: {0}", e.Message);
            }

            //Console.WriteLine("\n Press Enter to continue...");
            //Console.Read();
        }

        public void Send(string message)
        {
            if(client.Connected && client != null && stream != null)
            {
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                byte[] dataWithChecksum = new byte[data.Length + 1];
                data.CopyTo(dataWithChecksum, 0);
                dataWithChecksum[dataWithChecksum.Length - 1] = MagicUtils.CalculateChecksum(data); //Always last byte of the message is a checksum
                stream.Write(dataWithChecksum, 0, dataWithChecksum.Length);
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
                return true;
            }
            else
                return false;
        }

        public async Task<System.Threading.CancellationToken> SetPowerState(bool isOn)
        {
            if (isOn)
            {
                await Send(Commands.TURN_ON);
            }
            else
            {
                await Send(Commands.TURN_OFF);
            }

            var data = new byte[4];
            var CT = new System.Threading.CancellationToken();

            await stream.ReadAsync(data, CT);
            Trace.WriteLine("Received: " + BitConverter.ToString(data) + " after changing the power state");
            return CT;
        }

        public async void SetColor(Color color)
        {
            byte[] message = Commands.SET_COLOR;
            message[1] = color.R;
            message[2] = color.G;
            message[3] = color.B;

            await Send(message);
            //await Task.Delay(200);
            //await Send(message);
        }

        async public Task<MagicStructs.Controller> RequestControllerData()
        {
            if (client == null || !client.Connected)
            {
                try
                {
                    await client.ConnectAsync(Address, Port);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    return null;
                }
                if(client != null && client.Connected)
                    stream = client.GetStream();
                else
                    return null;
            }

            var res = await Send(Commands.REQUEST_DATA);

            if(!res)
                return null;

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
            controller.functionMode = data[3];

            //string responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Trace.WriteLine(("Received: {0}", BitConverter.ToString(data)));
            return controller;
        }

        public void SetCustomFunction(MagicStructs.Function customFunc)
        {
            var byteColors = new byte[16];
            foreach(Color color in customFunc.colors)
            {

            }
        }
    }
}
