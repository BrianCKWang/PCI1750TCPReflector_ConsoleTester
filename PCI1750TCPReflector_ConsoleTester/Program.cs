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
            Int32 port = 2500;
            TcpClient client = new TcpClient(IP, port);

            int message_int = 0;
            
            bool manualInput = false;
            bool showVerboseMessage = false;

            TCPDigitalIOTranslator translator = new TCPDigitalIOTranslator(showVerboseMessage);
            StaticDIO staticDIO = new StaticDIO(showVerboseMessage);

            //byte[] message_test = {}
            
            Console.WriteLine("{0}", translator.getTCPCommand(2, 0, 1));
            byte[] response_test = { 0, 202, 0, 6 };
            string yourByteString = Convert.ToString(translator.getDOfromTCPResponse(response_test), 2).PadLeft(16, '0');

            Console.WriteLine("{0:X}", translator.getDOfromTCPResponse(response_test));
            Console.WriteLine(yourByteString);
            Thread.Sleep(2000000);

            while (true)
            {
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
                        SendTCPAndUpdateDO(client, message_int, showVerboseMessage);
                    }
                }
                else
                {
                    Console.WriteLine("Message is invalid.");
                }

                UpdateAllStatus(client, showVerboseMessage);
                Thread.Sleep(200);
                Console.WriteLine("");
            }
        }
        public static bool CheckMessageValidity(int message)
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
        public static void CompleteHexMessage(ref String hex)
        {
            if (hex.Length % 2 != 0)
            {
                hex = "0" + hex;
            }
        }
        static byte[] ConnectAndSend(TcpClient client, int message_int)
        {
            
            // Buffer to store the response bytes.
            Byte[] data_read = new Byte[4];
            string message = message_int.ToString("X");
            //message = message_int.ToString("X");
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
                //Console.WriteLine("stream.ReadTimeout: {0}", stream.ReadTimeout);
                Int32 bytes = stream.Read(data_read, 0, data_read.Length);

                Console.WriteLine("project: {0}, {1}, {2}, {3}", data_read[0].ToString(), data_read[1].ToString(), data_read[2].ToString(), data_read[3].ToString());
                
                // Close everything.
                stream.Close();
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
        static void UpdateAllStatus(TcpClient client, bool showVerboseMessage)
        {
            TCPDigitalIOTranslator translator = new TCPDigitalIOTranslator(showVerboseMessage);
            StaticDIO staticDIO = new StaticDIO(showVerboseMessage);
            int command = 0;
            Console.WriteLine("*** Update All Status ***");
            staticDIO.UpdateStaticDI();
            command = translator.getTCPCommand(staticDIO.ProjectNum, staticDIO.RobotNum, (byte)TCPDigitalIOTranslator.DIOCommand.Project_GetStatus);
            Console.WriteLine("*** command: {0}", command);
            SendTCPAndUpdateDO(client, command, showVerboseMessage);

            command = translator.getTCPCommand(staticDIO.ProjectNum, (byte)TCPDigitalIOTranslator.RobotNum.R1, (byte)TCPDigitalIOTranslator.DIOCommand.Robot_GetStatus);
            Console.WriteLine("*** command: {0}", command);
            SendTCPAndUpdateDO(client, command, showVerboseMessage);

            command = translator.getTCPCommand(staticDIO.ProjectNum, (byte)TCPDigitalIOTranslator.RobotNum.R2, (byte)TCPDigitalIOTranslator.DIOCommand.Robot_GetStatus);
            Console.WriteLine("*** command: {0}", command);
            SendTCPAndUpdateDO(client, command, showVerboseMessage);

            command = translator.getTCPCommand(staticDIO.ProjectNum, (byte)TCPDigitalIOTranslator.RobotNum.R3, (byte)TCPDigitalIOTranslator.DIOCommand.Robot_GetStatus);
            Console.WriteLine("*** command: {0}", command);
            SendTCPAndUpdateDO(client, command, showVerboseMessage);

            command = translator.getTCPCommand(staticDIO.ProjectNum, (byte)TCPDigitalIOTranslator.RobotNum.R4, (byte)TCPDigitalIOTranslator.DIOCommand.Robot_GetStatus);
            Console.WriteLine("*** command: {0}", command);
            SendTCPAndUpdateDO(client, command, showVerboseMessage);

        }
        static void SendTCPAndUpdateDO(TcpClient client, int message_int, bool showVerboseMessage)
        {
            TCPDigitalIOTranslator translator = new TCPDigitalIOTranslator(showVerboseMessage);
            StaticDIO staticDIO = new StaticDIO(showVerboseMessage);
            byte[] responsebyte = { 0, 0, 0, 0 };

            responsebyte = ConnectAndSend(client, message_int);
            Console.WriteLine("TCP Response: {0:X}.", translator.getDOfromTCPResponse(responsebyte));

            staticDIO.UpdateStaticDO(translator.getDOfromTCPResponse(responsebyte));
        }
       
        public static void CleanUpDataForSending(ref Byte[] data_in, ref Byte[] data_out)
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
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        
    }
}
