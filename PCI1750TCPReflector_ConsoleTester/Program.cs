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
            String IP = "10.89.0.127";
            Int32 port = 1700;
            int message_int = 0;
            string message = "";
            bool manualinput = true;

            

            TCPDigitalIOTranslator translator = new TCPDigitalIOTranslator();
            StaticDIO staticDIO = new StaticDIO();

            while (true)
            {
                //staticDIO.RunStaticDI();
                //staticDIO.RunStaticDO();

                //Console.WriteLine("ProjectNum: {0}, RobotNum: {1}, Command: {2}.", staticDIO.ProjectNum, staticDIO.RobotNum, staticDIO.Command);
                //Console.WriteLine("Execute: {0}, StatusAck: {1}.", staticDIO.Execute, staticDIO.StatusAck);

                //Console.WriteLine("Project TCP command: {0}.", translator.test_getProjectTCPCommand(staticDIO.ProjectNum, staticDIO.Command));
                //Console.WriteLine("Robot TCP command: {0}.", translator.test_getRobotTCPCommand(staticDIO.ProjectNum, staticDIO.RobotNum, staticDIO.Command));
                byte[] testResponse = { 2, 103, 0, 7};
                byte[] responsebyte = { 0, 0, 0, 0 };
                //Console.WriteLine("Test Status: {0}.", translator.getDOfromTCPResponse(testResponse));

                if (manualinput)
                {
                    Console.WriteLine("");
                    Console.Write("Value to send: ");
                    message_int = Convert.ToInt32(Console.ReadLine());
                }
                else
                {
                    Console.WriteLine("TCP command: {0}.", translator.getTCPCommand(staticDIO.ProjectNum, staticDIO.RobotNum, staticDIO.Command));
                    message_int = translator.getTCPCommand(staticDIO.ProjectNum, staticDIO.RobotNum, staticDIO.Command);
                }
                
                if (checkMessageValidity(message_int))
                {
                    Console.WriteLine("Message is valid.");
                    if(staticDIO.Execute == 1 || manualinput)
                    {
                        message = message_int.ToString("X");
                        completeHexMessage(ref message);

                        Console.WriteLine("Will sent: {0}", message);

                        responsebyte = ConnectAndSend(IP, port, message, message_int);
                        Console.WriteLine("TCP Status: {0}.", translator.getDOfromTCPResponse(responsebyte));
                    }
                    
                }
                else
                {
                    Console.WriteLine("Message is invalid.");
                }

                
                Thread.Sleep(100);

            }
        }
        static bool checkMessageValidity(int message)
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
        static void completeHexMessage(ref String hex)
        {
            if (hex.Length % 2 != 0)
            {
                hex = "0" + hex;
            }
        }
        static byte[] ConnectAndSend(string IP, Int32 port, String message, int message_int)
        {
            // Buffer to store the response bytes.
            Byte[] data_read = new Byte[4];
            try
            {
                TcpClient client = new TcpClient(IP, port);
                NetworkStream stream = client.GetStream();

                Byte[] data = { 0, 0 };
                Byte[] data_message = StringToByteArray(message);
                cleanUpDataForSending(ref data_message, ref data);

                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                while (client.Available == 0) { };

                // Receive the TcpServer.response.

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
            return data_read;

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
