using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI1750TCPReflector_ConsoleTester
{
    class TCPDigitalIOTranslator
    {
        private static Dictionary<byte, byte> Dic_TCPtoDIO_response = new Dictionary<byte, byte>();
        private static Dictionary<byte, byte> Dic_DIOtoTCP_command = new Dictionary<byte, byte>();
        private static Dictionary<byte, byte> Dic_projectNumber = new Dictionary<byte, byte>();
        private static Dictionary<byte, byte> Dic_TCPCommand = new Dictionary<byte, byte>();
        private static Dictionary<byte, byte> Dic_RobotCommand = new Dictionary<byte, byte>();
        
        public TCPDigitalIOTranslator()
        {
            populateDictionary();
        }

        public int getTCPCommand(byte projectNum, byte robotNum, byte command)
        {
            switch (command)
            {
                //Project Command
                case 1: //Open
                case 2: //Close
                case 3: //Start
                case 4: //Stop
                case 5: //Get Status
                    return getProjectTCPCommand(projectNum, command);
                //Robot Command
                case 6: //Start
                case 7: //Pause
                case 8: //Stop
                case 9: //Get Status
                case 10: //Reset Emergency Stop
                    return getRobotTCPCommand(projectNum, robotNum, command);

            }
            return 0;
        }
        public int test_getProjectTCPCommand(byte projectNum, byte command)
        {
            Console.WriteLine("Numbers in test_getProjectTCPCommand: {0}, {1}", projectNum, command);
            int temp = getProjectTCPCommand(projectNum, command);
            Console.WriteLine("Number out test_getProjectTCPCommand: {0}", temp);
            return temp;
        }
        private int getProjectTCPCommand(byte projectNum, byte command)
        {
            byte PM_projectNum = 0;
            byte PM_command = 0;

            try
            {
                Dic_projectNumber.TryGetValue(projectNum, out PM_projectNum);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key = {0} is not found.", projectNum);
            }
            try
            {
                Dic_DIOtoTCP_command.TryGetValue(command, out PM_command);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key = {0} is not found.", command);
            }

            return Convert.ToInt32(PM_projectNum) * 100 + Convert.ToInt32(PM_command);
        }
        public int test_getRobotTCPCommand(byte projectNum, byte robotNum, byte command)
        {
            return getRobotTCPCommand(projectNum, robotNum, command);
        }
        private int getRobotTCPCommand(byte projectNum, byte robotNum, byte command)
        {
            byte PM_projectNum = 0;
            byte PM_robotNum = getRobotDigit(robotNum);
            byte PM_command = 0;
            try
            {
                Dic_projectNumber.TryGetValue(projectNum, out PM_projectNum);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key = {0} is not found.", projectNum);
            }
            try
            {
                Dic_RobotCommand.TryGetValue(command, out PM_command);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key = {0} is not found.", command);
            }
            return Convert.ToInt32(PM_projectNum*100 + PM_robotNum*10 + PM_command);
        }
        private byte getRobotDigit(byte robotNum)
        {
            return ++robotNum;
        }
        public static void populateDictionary()
        {
            try
            {


                //Project status
                //DIOtoTCP_response.Add(0b00001, 0x06);
                //DIOtoTCP_response.Add(0b00010, 0x07);
                //DIOtoTCP_response.Add(0b00011, 0x01);
                //DIOtoTCP_response.Add(0b00100, 0x02);
                //DIOtoTCP_response.Add(0b00101, 0x08);
                //DIOtoTCP_response.Add(0b00110, 0x03);
                //DIOtoTCP_response.Add(0b00111, 0x04);
                //DIOtoTCP_response.Add(0b01000, 0x05);

                ////Robot status
                //DIOtoTCP_response.Add(0b10001, 0x01);
                //DIOtoTCP_response.Add(0b10010, 0x03);
                //DIOtoTCP_response.Add(0b10011, 0x02);
                //DIOtoTCP_response.Add(0b10100, 0x04);
                //DIOtoTCP_response.Add(0b10101, 0x05);
                //DIOtoTCP_response.Add(0b10110, 0x06);
                //DIOtoTCP_response.Add(0b10111, 0x07);

                ////Project status
                //TCPtoDIO_response.Add(0x06, 0b00001);
                //TCPtoDIO_response.Add(0x07, 0b00010);
                //TCPtoDIO_response.Add(0x01, 0b00011);
                //TCPtoDIO_response.Add(0x02, 0b00100);
                //TCPtoDIO_response.Add(0x08, 0b00101);
                //TCPtoDIO_response.Add(0x03, 0b00110);
                //TCPtoDIO_response.Add(0x04, 0b00111);
                //TCPtoDIO_response.Add(0x05, 0b01000);

                ////Robot status
                //TCPtoDIO_response.Add(0x01, 0b10001);
                //TCPtoDIO_response.Add(0x03, 0b10010);
                //TCPtoDIO_response.Add(0x02, 0b10011);
                //TCPtoDIO_response.Add(0x04, 0b10100);
                //TCPtoDIO_response.Add(0x05, 0b10101);
                //TCPtoDIO_response.Add(0x06, 0b10110);
                //TCPtoDIO_response.Add(0x07, 0b10111);

                //
                Dic_DIOtoTCP_command.Add(1, 11);
                Dic_DIOtoTCP_command.Add(2, 12);
                Dic_DIOtoTCP_command.Add(3, 13);
                Dic_DIOtoTCP_command.Add(4, 14);
                Dic_DIOtoTCP_command.Add(5, 15);

                //Project number
                Dic_projectNumber.Add(1, 6);    //HMI project 1 is project 6
                Dic_projectNumber.Add(2, 2);    //HMI project 2 is project 2

                //RobotCommand
                Dic_RobotCommand.Add(6, 1); //Start
                Dic_RobotCommand.Add(7, 2); //Pause
                Dic_RobotCommand.Add(8, 3); //Stop
                Dic_RobotCommand.Add(9, 4); //Get Status
                Dic_RobotCommand.Add(10, 6);//Reset emergency stop
            }
            catch (ArgumentException)
            {

            }
        }

    }
}
