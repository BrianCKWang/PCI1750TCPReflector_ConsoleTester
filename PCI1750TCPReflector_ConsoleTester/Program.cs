using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;


namespace PCI1750TCPReflector_ConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            String IP = "127.0.0.1";
            Int32 port = 1700;
            int message_int = 0;
            
            bool manualInput = false;
            bool showVerboseMessage = false;

            TCPDigitalIOTranslator translator = new TCPDigitalIOTranslator(showVerboseMessage);
            StaticDIO staticDIO = new StaticDIO(showVerboseMessage);
            TcpClient client = new TcpClient(IP, port);
            

            while (true)
            {
                //staticDIO.RunStaticDI();
                //staticDIO.RunStaticDO();

                //Console.WriteLine("ProjectNum: {0}, RobotNum: {1}, Command: {2}.", staticDIO.ProjectNum, staticDIO.RobotNum, staticDIO.Command);
                //Console.WriteLine("Execute: {0}, StatusAck: {1}.", staticDIO.Execute, staticDIO.StatusAck);

                //Console.WriteLine("Project TCP command: {0}.", translator.test_getProjectTCPCommand(staticDIO.ProjectNum, staticDIO.Command));
                //Console.WriteLine("Robot TCP command: {0}.", translator.test_getRobotTCPCommand(staticDIO.ProjectNum, staticDIO.RobotNum, staticDIO.Command));

                //byte[] testResponse = { 2, 103, 0, 7};
                //Console.WriteLine("Test Status: {0}.", translator.getDOfromTCPResponse(testResponse));

                if (manualInput)
                {
                    Console.WriteLine("");
                    Console.Write("Value to send: ");
                    message_int = Convert.ToInt32(Console.ReadLine());
                }
                else
                {
                    staticDIO.UpdateStaticDI();
                    Console.WriteLine("TCP command: {0}. ProjectNum: {1}. RobotNum: {2}. Command: {3}", translator.getTCPCommand(staticDIO.ProjectNum, staticDIO.RobotNum, staticDIO.Command), staticDIO.ProjectNum, staticDIO.RobotNum, staticDIO.Command);
                    message_int = translator.getTCPCommand(staticDIO.ProjectNum, staticDIO.RobotNum, staticDIO.Command);
                }
                
                if (CheckMessageValidity(message_int))
                {
                    Console.WriteLine("Message is valid.");
                    if(staticDIO.Execute == 1 || manualInput)
                    {
                        SendTCPAndUpdateDO(IP, port, message_int, showVerboseMessage, ref client);
                    }
                    
                }
                else
                {
                    Console.WriteLine("Message is invalid.");
                }

                UpdateAllStatus(IP, port, showVerboseMessage, ref client);
                Thread.Sleep(100);

            }
        }
        static bool CheckMessageValidity(int message)
        {
            bool isGood = true;


            for(int i = 0; i < 3; ++i)
            {
                if (message % 10 == 0)
                {
                    isGood = false;
                }
                message /= 10;
            }

            
            return isGood;
        }
        static void CompleteHexMessage(ref String hex)
        {
            if (hex.Length % 2 != 0)
            {
                hex = "0" + hex;
            }
        }
        static byte[] ConnectAndSend(string IP, Int32 port, int message_int, ref TcpClient client)
        {
            // Buffer to store the response bytes.
            Byte[] data_read = new Byte[4];
            string message = message_int.ToString("X");
            message = message_int.ToString("X");
            CompleteHexMessage(ref message);
            Console.WriteLine("Will sent: {0}", message);
            try
            {
                
                NetworkStream stream = client.GetStream();
                stream.ReadTimeout = 2000;
                stream.WriteTimeout = 2000;

                Byte[] data = { 0, 0 };
                Byte[] data_message = StringToByteArray(message);
                CleanUpDataForSending(ref data_message, ref data);

                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                while (client.Available == 0) { };

                // Receive the TcpServer.response.

                // Read the first batch of the TcpServer response bytes.
                Console.WriteLine("stream.ReadTimeout: {0}", stream.ReadTimeout);
                Int32 bytes = stream.Read(data_read, 0, data_read.Length);

                Console.WriteLine("project: {0}, {1}, {2}, {3}", data_read[0].ToString(), data_read[1].ToString(), data_read[2].ToString(), data_read[3].ToString());
                
                // Close everything.
                //stream.Close();
                //client.Close();
                
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            return data_read;

        }
        static void UpdateAllStatus(string IP, Int32 port, bool showVerboseMessage, ref TcpClient client)
        {
            SendTCPAndUpdateDO(IP, port, 215, showVerboseMessage, ref client);
            SendTCPAndUpdateDO(IP, port, 224, showVerboseMessage, ref client);
            SendTCPAndUpdateDO(IP, port, 234, showVerboseMessage, ref client);
            SendTCPAndUpdateDO(IP, port, 244, showVerboseMessage, ref client);
            SendTCPAndUpdateDO(IP, port, 254, showVerboseMessage, ref client);

            SendTCPAndUpdateDO(IP, port, 615, showVerboseMessage, ref client);
            SendTCPAndUpdateDO(IP, port, 624, showVerboseMessage, ref client);
            SendTCPAndUpdateDO(IP, port, 634, showVerboseMessage, ref client);
            SendTCPAndUpdateDO(IP, port, 644, showVerboseMessage, ref client);
            SendTCPAndUpdateDO(IP, port, 654, showVerboseMessage, ref client);

        }
        static void SendTCPAndUpdateDO(string IP, Int32 port, int message_int, bool showVerboseMessage, ref TcpClient client)
        {
            TCPDigitalIOTranslator translator = new TCPDigitalIOTranslator(showVerboseMessage);
            StaticDIO staticDIO = new StaticDIO(showVerboseMessage);
            byte[] responsebyte = { 0, 0, 0, 0 };

            responsebyte = ConnectAndSend(IP, port, message_int, ref client);
            Console.WriteLine("TCP Status: {0:X}.", translator.getDOfromTCPResponse(responsebyte));

            staticDIO.UpdateStaticDO(translator.getDOfromTCPResponse(responsebyte));
        }
        static void CleanUpDataForSending(ref Byte[] data_in, ref Byte[] data_out)
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
