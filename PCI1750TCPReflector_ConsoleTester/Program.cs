using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;



namespace PCI1750TCPReflector_ConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            String IP = "10.89.0.127";
            Int32 port = 1700;
            int message_int = 0;
            string message = "";

            TCPDigitalIOTranslator translator = new TCPDigitalIOTranslator();
            StaticDIO staticDIO = new StaticDIO();
            while (true)
            {
                Console.Write("Value to send: ");
                message_int = Convert.ToInt32(Console.ReadLine());
                message = message_int.ToString("X");
                completeHexMessage(ref message);

                Console.WriteLine("Will sent: {0}", message);

                ConnectAndSend(IP, port, message, message_int);

                
                Console.WriteLine("Project TCP command: {0}.", translator.test_getProjectTCPCommand(1, 1));
                Console.WriteLine("Robot TCP command: {0}.", translator.test_getRobotTCPCommand(1, 2, 9));

                staticDIO.RunStaticDI();
                Console.WriteLine("ProjectNum: {0}, RobotNum: {1}, Command: {2}.", staticDIO.ProjectNum, staticDIO.RobotNum, staticDIO.Command);
                Console.WriteLine("Execute: {0}, StatusAck: {1}.", staticDIO.Execute, staticDIO.StatusAck);
                //StaticDIO.StaticDI();
                //StaticDIO.StaticDO();

            }
        }

        static void completeHexMessage(ref String hex)
        {
            if (hex.Length % 2 != 0)
            {
                hex = "0" + hex;
            }
        }
        static void ConnectAndSend(String server, Int32 port, String message, int message_Dec)
        {
            try
            {
                TcpClient client = new TcpClient(server, port);

                Byte[] data = { 0, 0 };
                Byte[] data_message = StringToByteArray(message);
                cleanUpDataForSending(ref data_message, ref data);
                
                NetworkStream stream = client.GetStream();

                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                while (client.Available == 0) { };

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                Byte[] data_read = new Byte[4];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data_read, 0, data_read.Length);

                Console.WriteLine("project: {0}, {1}, {2}, {3}", data_read[0].ToString(), data_read[1].ToString(), data_read[2].ToString(), data_read[3].ToString());


                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

        }

        static void cleanUpDataForSending(ref Byte[] data_in, ref Byte[] data_out)
        {
            if(data_in.Length == 1)
            {
                data_out[0] = 0;
                data_out[1] = data_in[0];
            }
            else
            {
                data_out[0] = data_in[0];
                data_out[1] = data_in[1];
            }
        }
        static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        
    }
}
