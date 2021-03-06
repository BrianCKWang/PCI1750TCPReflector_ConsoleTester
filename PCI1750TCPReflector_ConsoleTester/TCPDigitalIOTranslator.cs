﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI1750TCPReflector_ConsoleTester
{
    class TCPDigitalIOTranslator
    {
        private bool _showVerboseMessage;
        private static Dictionary<byte, byte> Dic_TCPtoDIO_projectResponse = new Dictionary<byte, byte>();
        private static Dictionary<byte, string> Dic_TCPtoString_projectResponse = new Dictionary<byte, string>();
        private static Dictionary<byte, byte> Dic_TCPtoDIO_robotResponse = new Dictionary<byte, byte>();
        private static Dictionary<byte, string> Dic_TCPtoString_robotResponse = new Dictionary<byte, string>();
        private static Dictionary<byte, byte> Dic_DIOtoTCP_command = new Dictionary<byte, byte>();
        private static Dictionary<byte, byte> Dic_DIOtoTCP_projectNumber = new Dictionary<byte, byte>();
        private static Dictionary<byte, byte> Dic_TCPtoDIO_projectNumber = new Dictionary<byte, byte>();
        private static Dictionary<byte, byte> Dic_RobotCommand = new Dictionary<byte, byte>();
        public enum DIOCommand {
            Empty = 0,
            Project_Open = 1,
            Project_Close = 2,
            Project_Start = 3,
            Project_Stop = 4,
            Project_GetStatus = 5,
            Robot_Start = 6,
            Robot_Pause = 7,
            Robot_Stop = 8,
            Robot_GetStatus = 9,
            Robot_ResetEmergencyStop = 10
        }
        public enum DIOResponse
        {
            Empty = 0,
            Project_Open = 1,
            Project_Closed = 2,
            Project_Running = 3,
            Project_Stopped = 4,
            Project_Modified = 5,
            Project_ConfigurationError = 6,
            Project_InvalidLicense = 7,
            Project_Error = 8,
            Robot_Running = 17,
            Robot_Paused = 18,
            Robot_Stopped = 19,
            Robot_Shutdown = 20,
            Robot_EmergencyStop = 21,
            Robot_Error = 22,
            Robot_ManualMode = 23
        }
        public enum RobotNum
        {
            R1 = 0,
            R2 = 1,
            R3 = 2,
            R4 = 3
        }
        public TCPDigitalIOTranslator(bool showVerboseMessaeg)
        {
            _showVerboseMessage = showVerboseMessaeg;
            populateDictionary();
        }
        public short getDOfromTCPResponse(byte[] response)
        {
            short commandSent = 0;
            byte responseCode = 0;
            byte PM_Command = 0;
            short fullMessage = 0;

            if (response.Length >= 4)
            {
                commandSent = (short)(response[1] | response[0] << 8);
                Console.WriteLine("commandSent: {0}.", commandSent);
                responseCode = response[3];
                Console.WriteLine("responseCode: {0}.", responseCode);
                if (isProjectCommand(commandSent))
                {
                    try
                    {
                        Dic_TCPtoDIO_projectResponse.TryGetValue(responseCode, out PM_Command);
                        Console.WriteLine("extractProjectNum(commandSent): {0}.", extractProjectNum(commandSent));
                        Console.WriteLine("extractRobotNum(commandSent): {0}.", extractRobotNum(commandSent));
                        Console.WriteLine("ProjectCommandSent PM_Command: {0}.", PM_Command);
                        
                        fullMessage = (short)(extractProjectNum(commandSent) | 1 << 3 | PM_Command << 5);

                    }
                    catch (KeyNotFoundException)
                    {
                        Console.WriteLine("Key = {0} is not found.", commandSent);
                    }

                }
                else
                {
                    try
                    {
                        Dic_TCPtoDIO_robotResponse.TryGetValue(responseCode, out PM_Command);
                        Console.WriteLine("RobotCommandSent PM_Command: {0}.", PM_Command);
                        fullMessage = (short)(1 | extractRobotNum(commandSent) << 3 | PM_Command << 5);
                    }
                    catch (KeyNotFoundException)
                    {
                        Console.WriteLine("Key = {0} is not found.", commandSent);
                    }
                }
            }
            return fullMessage;

        }
        public string getStatusStringfromTCPResponse(byte[] response)
        {
            short commandSent = 0;
            byte responseCode = 0;
            string PM_Response = "";

            if (response.Length >= 4)
            {
                commandSent = (short)(response[1] | response[0] << 8);
                Console.WriteLine("commandSent: {0}.", commandSent);
                responseCode = response[3];
                Console.WriteLine("responseCode: {0}.", responseCode);
                if (isProjectCommand(commandSent))
                {
                    try
                    {
                        Dic_TCPtoString_projectResponse.TryGetValue(responseCode, out PM_Response);
                    }
                    catch (KeyNotFoundException)
                    {
                        Console.WriteLine("Key = {0} is not found.", commandSent);
                    }
                }
                else
                {
                    try
                    {
                        Dic_TCPtoString_robotResponse.TryGetValue(responseCode, out PM_Response);
                    }
                    catch (KeyNotFoundException)
                    {
                        Console.WriteLine("Key = {0} is not found.", commandSent);
                    }
                }
            }
            return PM_Response;
        }
        private bool isProjectCommand(short commandSent)
        {
            commandSent /= 10;
            commandSent %= 10;
            if (commandSent <= 1)
            {
                return true;
            }
            else
            { 
               return false;
            }
        }
        private byte extractProjectNum(short commandSent)
        {
            return (byte)(commandSent /= 100);
        }
        private byte extractRobotNum(short commandSent)
        {
            commandSent /= 10;
            commandSent %= 10;
            return (byte)(commandSent);
        }
        public int getTCPCommand(byte projectNum, byte robotNum, byte command)
        {
            switch (command)
            {
                case (byte)DIOCommand.Project_Open:
                case (byte)DIOCommand.Project_Close:
                case (byte)DIOCommand.Project_Start:
                case (byte)DIOCommand.Project_Stop:
                case (byte)DIOCommand.Project_GetStatus:
                    return getProjectTCPCommand(projectNum, command);
                case (byte)DIOCommand.Robot_Start:
                case (byte)DIOCommand.Robot_Pause:
                case (byte)DIOCommand.Robot_Stop:
                case (byte)DIOCommand.Robot_GetStatus:
                case (byte)DIOCommand.Robot_ResetEmergencyStop:
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
                Dic_DIOtoTCP_projectNumber.TryGetValue(projectNum, out PM_projectNum);
                if (_showVerboseMessage)
                {
                    Console.WriteLine("projectNum: {0}", projectNum);
                    Console.WriteLine("PM_projectNum: {0}", PM_projectNum);
                }
                    
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key = {0} is not found.", projectNum);
            }
            try
            {
                Dic_DIOtoTCP_command.TryGetValue(command, out PM_command);
                if (_showVerboseMessage)
                {
                    Console.WriteLine("command: {0}", command);
                    Console.WriteLine("PM_command: {0}", PM_command);
                }
                    
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
            Console.WriteLine("======================= PM_robotNum: {0}", PM_robotNum);
            try
            {
                Dic_DIOtoTCP_projectNumber.TryGetValue(projectNum, out PM_projectNum);

                if (_showVerboseMessage)
                    Console.WriteLine("PM_projectNum: {0}", PM_projectNum);

            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key = {0} is not found.", projectNum);
            }
            try
            {
                Dic_RobotCommand.TryGetValue(command, out PM_command);

                if (_showVerboseMessage)
                    Console.WriteLine("PM_command: {0}", PM_command);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key = {0} is not found.", command);
            }
            return Convert.ToInt32(PM_projectNum * 100 + PM_robotNum * 10 + PM_command);
        }
        private byte getRobotDigit(byte robotNum)
        {
            
            int robotCode = robotNum;
            robotCode += 1;
            return (byte)robotCode;
        }
        public static void populateDictionary()
        {
            try
            {
                //Project status
                Dic_TCPtoDIO_projectResponse.Add(0x06, 0b00001);
                Dic_TCPtoDIO_projectResponse.Add(0x07, 0b00010);
                Dic_TCPtoDIO_projectResponse.Add(0x01, 0b00011);
                Dic_TCPtoDIO_projectResponse.Add(0x02, 0b00100);
                Dic_TCPtoDIO_projectResponse.Add(0x08, 0b00101);
                Dic_TCPtoDIO_projectResponse.Add(0x03, 0b00110);
                Dic_TCPtoDIO_projectResponse.Add(0x04, 0b00111);
                Dic_TCPtoDIO_projectResponse.Add(0x05, 0b01000);

                Dic_TCPtoString_projectResponse.Add(0x01, "Running");
                Dic_TCPtoString_projectResponse.Add(0x02, "Stopped");
                Dic_TCPtoString_projectResponse.Add(0x03, "Configuratino Error");
                Dic_TCPtoString_projectResponse.Add(0x04, "No License");
                Dic_TCPtoString_projectResponse.Add(0x05, "Error");
                Dic_TCPtoString_projectResponse.Add(0x06, "Open");
                Dic_TCPtoString_projectResponse.Add(0x07, "Closed");
                Dic_TCPtoString_projectResponse.Add(0x08, "Modified");

                //Robot status
                Dic_TCPtoDIO_robotResponse.Add(0x01, 0b10001);
                Dic_TCPtoDIO_robotResponse.Add(0x03, 0b10010);
                Dic_TCPtoDIO_robotResponse.Add(0x02, 0b10011);
                Dic_TCPtoDIO_robotResponse.Add(0x04, 0b10100);
                Dic_TCPtoDIO_robotResponse.Add(0x05, 0b10101);
                Dic_TCPtoDIO_robotResponse.Add(0x06, 0b10110);
                Dic_TCPtoDIO_robotResponse.Add(0x07, 0b10111);

                Dic_TCPtoString_robotResponse.Add(0x01, "Running");
                Dic_TCPtoString_robotResponse.Add(0x02, "Stopped");
                Dic_TCPtoString_robotResponse.Add(0x03, "Paused");
                Dic_TCPtoString_robotResponse.Add(0x04, "Shutdown");
                Dic_TCPtoString_robotResponse.Add(0x05, "Emergency Stopped");
                Dic_TCPtoString_robotResponse.Add(0x06, "Error");
                Dic_TCPtoString_robotResponse.Add(0x07, "Manual Mode");

                //
                Dic_DIOtoTCP_command.Add(1, 0);
                Dic_DIOtoTCP_command.Add(2, 1);
                Dic_DIOtoTCP_command.Add(3, 2);
                Dic_DIOtoTCP_command.Add(4, 3);
                Dic_DIOtoTCP_command.Add(5, 4);

                //Project number
                Dic_DIOtoTCP_projectNumber.Add(1, 1);    //HMI project 1 is PM project 6
                Dic_DIOtoTCP_projectNumber.Add(2, 2);    //HMI project 2 is PM project 2

                Dic_TCPtoDIO_projectNumber.Add(1, 1);    //PM project 6 is HMI project 1 
                Dic_TCPtoDIO_projectNumber.Add(2, 2);    //PM project 2 is HMI project 2 

                //RobotCommand
                Dic_RobotCommand.Add(5, 5); //Start
                Dic_RobotCommand.Add(6, 6); //Pause
                Dic_RobotCommand.Add(7, 7); //Stop
                Dic_RobotCommand.Add(8, 8); //Get Status
                Dic_RobotCommand.Add(9, 9);//Reset emergency stop
            }
            catch (ArgumentException)
            {

            }
        }

    }
}
