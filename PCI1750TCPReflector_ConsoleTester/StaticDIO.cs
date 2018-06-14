using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automation.BDaq;
using System.Threading;

namespace PCI1750TCPReflector_ConsoleTester
{
    
    class StaticDIO
    {
        private byte _projectNum;
        private byte _robotNum;
        private byte _command;
        private byte _execute;
        private byte _statusAck;
        private bool _showVerboseMessage;
        private static bool StaticIO_firstTime;
        public StaticDIO(bool showVerboseMessage)
        {
            _projectNum = 0;
            _robotNum = 0;
            _command = 0;
            _showVerboseMessage = showVerboseMessage;
            StaticIO_firstTime = false;
        }
        public byte ProjectNum
        {
            get {
                if (!StaticIO_firstTime) { UpdateStaticDI(); }
                return _projectNum; }
        }
        public byte RobotNum
        {
            get {
                if (!StaticIO_firstTime) { UpdateStaticDI(); }
                return _robotNum; }
        }
        public byte Command
        {
            get {
                if (!StaticIO_firstTime) { UpdateStaticDI(); }
                return _command; }
        }
        public byte Execute
        {
            get
            {
                if (!StaticIO_firstTime) { UpdateStaticDI(); }
                return _execute;
            }
        }
        public byte StatusAck
        {
            get
            {
                if (!StaticIO_firstTime) { UpdateStaticDI(); }
                return _statusAck;
            }
        }
        private void extractAndsetNum(short fullResponse)
        {
            short projectBitmask   = 0b0000000000000111;
            short robotBitmask     = 0b0000000000011000;
            short commandBitmask   = 0b0000000111100111;
            short executeBitmask   = 0b0000001000000000;
            short statusAckBitmask = 0b0000010000000000;

            _projectNum = (byte)(fullResponse & projectBitmask);
            _robotNum = (byte)((fullResponse & robotBitmask) >> 3); 
            _command = (byte)((fullResponse & commandBitmask) >> 5);
            _execute = (byte)((fullResponse & executeBitmask) >> 9);
            _statusAck = (byte)((fullResponse & statusAckBitmask) >> 10);
        }
        
        public void UpdateStaticDI()
        {
            //-----------------------------------------------------------------------------------
            // Configure the following parameters before running the demo
            //-----------------------------------------------------------------------------------
            //The default device of project is demo device, users can choose other devices according to their needs. 
            string deviceDescription = "PCI-1750,BID#0";
            string profilePath = "../../profile/PCI-1750.xml";
            int startPort = 0;
            int portCount = 2;
            short fullResponse = 0;
            StaticIO_firstTime = true;
            ErrorCode errorCode = ErrorCode.Success;

            // Step 1: Create a 'InstantDiCtrl' for DI function.
            InstantDiCtrl instantDiCtrl = new InstantDiCtrl();

            try
            {
                // Step 2: Select a device by device number or device description and specify the access mode.
                // in this example we use ModeWrite mode so that we can fully control the device, including configuring, sampling, etc.
                instantDiCtrl.SelectedDevice = new DeviceInformation(deviceDescription);
                errorCode = instantDiCtrl.LoadProfile(profilePath);//Loads a profile to initialize the device.
                if (BioFailed(errorCode))
                {
                    throw new Exception();
                }
                // read DI ports' status and show.
                //Console.WriteLine("Reading ports' status is in progress..., any key to quit!\n");
                byte[] buffer = new byte[64];
                //byte data = 0;//data is used to the API ReadBit.
                //int  bit = 0;//bit is used to the API ReadBit.

                // Step 3: Read DI ports' status and show.
                errorCode = instantDiCtrl.Read(startPort, portCount, buffer);
                //errorCode = instantDiCtrl.ReadBit(startPort, bit, out data);
                if (BioFailed(errorCode))
                {
                    throw new Exception();
                }
                //Show ports' status
                for (int i = 0; i < portCount; ++i)
                {
                    if(_showVerboseMessage)
                        Console.WriteLine(" DI port {0} status : 0x{1:x}\n", startPort + i, buffer[i]);
                    

                    /************************************************************************/
                    //Console.WriteLine(" DI port {0} status : 0x{1:x}\n", startPort + i, data);
                    //NOTE:
                    //argument1:which port you want to control? For example, startPort is 0.
                    //argument2:which bit you want to control? You can write 0--7, any number you want.
                    //argument3:data is used to save the result.                                                                     
                    /************************************************************************/
                }
                fullResponse = (short)(buffer[0] | buffer[1] << 8);
                extractAndsetNum(fullResponse);

                //Thread.Sleep(100);

            }
            catch (Exception e)
            {
                // Something is wrong
                string errStr = BioFailed(errorCode) ? " Some error occurred. And the last error code is " + errorCode.ToString()
                                                          : e.Message;
                Console.WriteLine(errStr);
            }
            finally
            {
                // Step 4: Close device and release any allocated resource.
                instantDiCtrl.Dispose();
                //Console.ReadKey(false);
            }
        }

        public void UpdateStaticDO(short output)
        {
            //-----------------------------------------------------------------------------------
            // Configure the following parameters before running the demo
            //-----------------------------------------------------------------------------------
            //The default device of project is demo device, users can choose other devices according to their needs. 
            string deviceDescription = "PCI-1750,BID#0";
            string profilePath = "../../profile/PCI-1750.xml";
            int startPort = 0;
            int portCount = 2;
            ErrorCode errorCode = ErrorCode.Success;

            // Step 1: Create a 'InstantDoCtrl' for DO function.
            InstantDoCtrl instantDoCtrl = new InstantDoCtrl();
            try
            {
                // Step 2: Select a device by device number or device description and specify the access mode.
                // in this example we use ModeWrite mode so that we can fully control the device, including configuring, sampling, etc.
                instantDoCtrl.SelectedDevice = new DeviceInformation(deviceDescription);
                errorCode = instantDoCtrl.LoadProfile(profilePath);//Loads a profile to initialize the device.
                if (BioFailed(errorCode))
                {
                    throw new Exception();
                }

                // Step 3: Write DO ports
                byte[] bufferForWriting = new byte[64];
                //byte dataForWriteBit = 0;//data is used to the 'WriteBit'.
                //int bit = 1;//the bit is used to the 'WriteBit'.

                for (int i = 0; i < portCount; ++i)
                {
                    Console.WriteLine("Input a hexadecimal number for DO port {0} to output(for example, 0x11): ", startPort + i);
                    string data = Console.ReadLine();
                    bufferForWriting[i] = byte.Parse(data.Contains("0x") ? data.Remove(0, 2) : data, System.Globalization.NumberStyles.HexNumber);
                    /*
                     //for WriteBit
                     Console.WriteLine(" Input a hexadecimal number for DO port {0} to output(for example, 0x1 or 0x00): ", startPort + i);
                     string data = Console.ReadLine();
                     dataForWriteBit = byte.Parse(data.Contains("0x") ? data.Remove(0, 2) : data, System.Globalization.NumberStyles.HexNumber);
                    */
                }
                errorCode = instantDoCtrl.Write(startPort, portCount, bufferForWriting);
                /************************************************************************/
                //errorCode = instantDoCtrl.WriteBit(startPort, bit, dataForWriteBit); 
                //NOTE:
                //Every channel has 8 bits, which be used to control 0--7 bit of anyone channel.
                //argument1:which port you want to contrl? For example, startPort is 0.
                //argument2:which bit you want to control? You can write 0--7, any number you want.
                //argument3:What status you want, open or close? 1 menas open, 0 means close.*/
                /************************************************************************/
                if (BioFailed(errorCode))
                {
                    throw new Exception();
                }
                Console.WriteLine("DO output completed !");
                // Read back the DO status. 
                // Note: 
                // For relay output, the read back must be deferred until the relay is stable.
                // The delay time is decided by the HW SPEC.
                // byte[] bufferForReading = new byte[64];
                // instantDoCtrl.DoRead(startPort, portCount, bufferForReading);
                // if (BioFailed(errorCode))
                // {
                //    throw new Exception();
                // }
                // Show DO ports' status
                // for (int i = startPort; i < portCount + startPort; ++i)
                // {
                //    Console.WriteLine("Now, DO port {0} status is:  0x{1:x}", i, bufferForReading[i - startPort]);
                // }
            }
            catch (Exception e)
            {
                // Something is wrong
                string errStr = BioFailed(errorCode) ? " Some error occurred. And the last error code is " + errorCode.ToString()
                                                           : e.Message;
                Console.WriteLine(errStr);
            }
            finally
            {
                // Step 4: Close device and release any allocated resource.
                instantDoCtrl.Dispose();
                //Console.ReadKey(false);
            }

        }
        public void UpdateStaticDO_manual()
        {
            //-----------------------------------------------------------------------------------
            // Configure the following parameters before running the demo
            //-----------------------------------------------------------------------------------
            //The default device of project is demo device, users can choose other devices according to their needs. 
            string deviceDescription = "PCI-1750,BID#0";
            string profilePath = "../../profile/PCI-1750.xml";
            int startPort = 0;
            int portCount = 2;
            ErrorCode errorCode = ErrorCode.Success;

            // Step 1: Create a 'InstantDoCtrl' for DO function.
            InstantDoCtrl instantDoCtrl = new InstantDoCtrl();
            try
            {
                // Step 2: Select a device by device number or device description and specify the access mode.
                // in this example we use ModeWrite mode so that we can fully control the device, including configuring, sampling, etc.
                instantDoCtrl.SelectedDevice = new DeviceInformation(deviceDescription);
                errorCode = instantDoCtrl.LoadProfile(profilePath);//Loads a profile to initialize the device.
                if (BioFailed(errorCode))
                {
                    throw new Exception();
                }

                // Step 3: Write DO ports
                byte[] bufferForWriting = new byte[64];
                //byte dataForWriteBit = 0;//data is used to the 'WriteBit'.
                //int bit = 1;//the bit is used to the 'WriteBit'.

                for (int i = 0; i < portCount; ++i)
                {
                    Console.WriteLine("Input a hexadecimal number for DO port {0} to output(for example, 0x11): ", startPort + i);
                    string data = Console.ReadLine();
                    bufferForWriting[i] = byte.Parse(data.Contains("0x") ? data.Remove(0, 2) : data, System.Globalization.NumberStyles.HexNumber);
                    /*
                     //for WriteBit
                     Console.WriteLine(" Input a hexadecimal number for DO port {0} to output(for example, 0x1 or 0x00): ", startPort + i);
                     string data = Console.ReadLine();
                     dataForWriteBit = byte.Parse(data.Contains("0x") ? data.Remove(0, 2) : data, System.Globalization.NumberStyles.HexNumber);
                    */
                }
                errorCode = instantDoCtrl.Write(startPort, portCount, bufferForWriting);
                /************************************************************************/
                //errorCode = instantDoCtrl.WriteBit(startPort, bit, dataForWriteBit); 
                //NOTE:
                //Every channel has 8 bits, which be used to control 0--7 bit of anyone channel.
                //argument1:which port you want to contrl? For example, startPort is 0.
                //argument2:which bit you want to control? You can write 0--7, any number you want.
                //argument3:What status you want, open or close? 1 menas open, 0 means close.*/
                /************************************************************************/
                if (BioFailed(errorCode))
                {
                    throw new Exception();
                }
                Console.WriteLine("DO output completed !");
                // Read back the DO status. 
                // Note: 
                // For relay output, the read back must be deferred until the relay is stable.
                // The delay time is decided by the HW SPEC.
                // byte[] bufferForReading = new byte[64];
                // instantDoCtrl.DoRead(startPort, portCount, bufferForReading);
                // if (BioFailed(errorCode))
                // {
                //    throw new Exception();
                // }
                // Show DO ports' status
                // for (int i = startPort; i < portCount + startPort; ++i)
                // {
                //    Console.WriteLine("Now, DO port {0} status is:  0x{1:x}", i, bufferForReading[i - startPort]);
                // }
            }
            catch (Exception e)
            {
                // Something is wrong
                string errStr = BioFailed(errorCode) ? " Some error occurred. And the last error code is " + errorCode.ToString()
                                                           : e.Message;
                Console.WriteLine(errStr);
            }
            finally
            {
                // Step 4: Close device and release any allocated resource.
                instantDoCtrl.Dispose();
                //Console.ReadKey(false);
            }

        }

        static bool BioFailed(ErrorCode err)
        {
            return err < ErrorCode.Success && err >= ErrorCode.ErrorHandleNotValid;
        }

    }
}
