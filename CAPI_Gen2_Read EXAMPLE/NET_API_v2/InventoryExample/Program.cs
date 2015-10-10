/********************************************************************************
 * Description: 
 *      InventoryExample is a general reference implementation of the Skyetek .NET API
 *  
 * Purpose:
 *      1. Device and reader creation
 *      2. Setting tag type
 *      3. Continuous inventory select mode operation
 *      4. System parameter reads and writes are performed when using a mux
 * 
 * Compatibility:
 *      1. Modules: M2, M4, M7, and M9
 *      2. Interfaces: USB and Serial
 *      3. Accessories: No mux, 4 port mux, 8 port mux
 * *****************************************************************************/

using System;
using System.Text;
using System.Threading;
using System.Collections;

using SkyeTek.Tags;
using SkyeTek.Devices;
using SkyeTek.STPv3;
using SkyeTek.Readers;

namespace Example
{
    class Run
    {
        static void Main(string[] args)
        {
            //declare settings and data structures
            const Boolean MUX_ENABLE = false;
            const int iterations = 10000;
            ArrayList myTags = new ArrayList();
            Controller c = new Controller();

            Console.Out.WriteLine("Setup");

            //create device
            if (c.CreateUSBDevice())
            {
                Console.Out.WriteLine("\tSkyetek USB Device Created");
            }
            else
            {
                c.CreateSerialDevice();
                Console.Out.WriteLine("\tSkyetek Serial Device Created");
            }

            //create reader (automatically opens the device too)
            c.CreateReader();
            Console.Out.WriteLine("\tSkyetek Reader Created");
            Console.Out.WriteLine(String.Format("\tProduct Code: {0}", c.GetProduct()));
            Console.Out.WriteLine(String.Format("\tReader Name: {0}", c.GetName()));

            //set tag type
            /************************************************************************************
             * If no type is specified, tag type defaults to auto detect all types. 
             * It is recommended the type be specified for quickest responses and best performance
             * ISO-18000-6C (gen2) is a common UHF tag protocol (for M7/M9 modules)
             * ISO-15693 is a common HF tag protocol (for M2/M4 modules)
            *************************************************************************************/
            if (c.GetProduct() == "0007" || c.GetProduct() == "0009")
            {
                c.SetTagType(TagType.ISO_18000_6C_AUTO_DETECT);
            }
            else if (c.GetProduct() == "0002" || c.GetProduct() == "0004")
            {
                c.SetTagType(TagType.ISO_15693_AUTO_DETECT);
            }
            Console.Out.WriteLine("\tTag Type Set To " + c.GetTagType() + "\n");

            //perform an inventory select for the set number of iterations
            //if a mux is connected, then cycle through all available ports
            if (!MUX_ENABLE)
            {
                for (int i = 1; i <= iterations; i++)
                {
                    myTags = c.GetTags();
                    Console.Out.WriteLine("Iteration #" + i.ToString());
                    foreach (string k in myTags)
                    {
                        Console.Out.WriteLine("\tTag Found: " + k.ToString());
                    }
                }
            }
            else //mux enabled
            {
                byte[] muxPorts;
                if (c.GetMuxPorts() == 4)
                {
                    muxPorts = new byte[] { 0, 2, 5, 7 };
                }
                else if (c.GetMuxPorts() == 8)
                {
                    muxPorts = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
                }
                else
                {
                    Console.Out.WriteLine("ERROR: NO MUX DETECTED");
                    muxPorts = new byte[] { 0 };
                }

                for (int i = 1; i <= iterations; i++)
                {
                    Console.Out.WriteLine("Iteration #" + i.ToString());
                    foreach (byte j in muxPorts)
                    {
                        Console.Out.WriteLine("\tPort=" + j);
                        c.SetMuxPort(j);
                        myTags = c.GetTags();
                        foreach (string k in myTags)
                        {
                            Console.Out.WriteLine("\t\tTag Found: " + k.ToString());
                        }
                    }
                }
            }

            //close the device
            c.Stop();
            Console.Out.WriteLine("\nSkyetek Device Closed");
        }
    }



    public class Controller
    {
        private Tag tag;
        private ArrayList tagList;
        private Device device;
        private STPv3Reader reader;

        //Time delay used between requests and responses
        private static int delay = 5; //ms

        //Constructor initializes data structure to hold tags and tag type
        public Controller()
        {
            tagList = new ArrayList();
            tag = new Tag();

        }

        //Create a device from the first module detected on the USB bus 
        public Boolean CreateUSBDevice()
        {
            Device[] devices;
            devices = USBDeviceFactory.Enumerate();
            if (devices.Length == 0)
            {
                //Console.Out.WriteLine("No USB devices found");
                return false;
            }
            else
            {
                device = devices[0];
                return true;
            }
        }

        //Create a device from a module connected to COM1 serial port
        public Boolean CreateSerialDevice()
        {
            device = new SerialDevice();
            try
            {
                device.Address = "COM1";
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("EXCEPTION:" + ex.ToString());
            }
            return true;
        }

        //Create a skyetek reader from a skyetek device.  Set default tag type.
        public void CreateReader()
        {
            device.Open();
            reader = new STPv3Reader(device);
        }

        //Wrapper for opening device.
        public void Start()
        {
           device.Open();
        }

        //Wrapper for closing device
        public void Stop()
        {
            device.Close();
        }

        public void SetTagType(TagType type)
        {
            tag.Type = type;
        }

        public TagType GetTagType()
        {
            return tag.Type;
        }

        public String GetProduct()
        {
            return reader.ProductCode;
        }

        public String GetName()
        {
            return reader.ReaderName;
        }

        //Returns the number of ports
        public int GetMuxPorts()
        {
            //used for READ_SYSTEM_PARAMETER which returns an array of bytes
            //the data field for the mux system parameter is 1 byte in length
            byte[] m = new byte[1];
            m[0] = 0;
            int ports = 0;

            //Build switch mux request.
            STPv3Response response;
            STPv3Request requestMux = new STPv3Request();
            requestMux.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
            requestMux.Address = 0x000A;
            requestMux.Blocks = 0x01;

            //send request and parse response
            try
            {
                requestMux.Issue(device);
                response = requestMux.GetResponse();
                if (response != null && response.ResponseCode == STPv3ResponseCode.READ_SYSTEM_PARAMETER_PASS)
                {
                    if (reader.ProductCode == "0002")
                    {
                        //check for 4 port HF mux
                        m[0] = 1;
                        if (response.Data[0] == m[0])
                        {
                            ports = 4;
                        }

                        //check for 8 port HF mux
                        m[0] = 2;
                        if (response.Data[0] == m[0])
                        {
                            ports = 8;
                        }
                    }

                    if (reader.ProductCode == "0007" || reader.ProductCode == "0009")
                    {
                        //check for 4 port UHF mux
                        m[0] = 5;
                        if (response.Data[0] == m[0])
                        {
                            ports = 4;
                        }

                        //check for 8 port UHF mux
                        m[0] = 6;
                        if (response.Data[0] == m[0])
                        {
                            ports = 8;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("EXCEPTION:" + ex.ToString());
            }
            return ports;
        }

        //Opens a specific mux port
        public Boolean SetMuxPort(byte port)
        {
            //used for WRITE_SYSTEM_PARAMETER which expects an array of bytes
            //the data field for the mux system parameter is 1 byte in length
            byte[] p = new byte[1];
            p[0] = port;

            //Build switch mux request.
            STPv3Response response;
            STPv3Request requestMux = new STPv3Request();
            requestMux.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
            requestMux.Address = 0x000A;
            requestMux.Blocks = 0x01;
            requestMux.Data = p;

            //send request
            try
            {
                requestMux.Issue(device);
                response = requestMux.GetResponse();
                if (response != null && response.ResponseCode == STPv3ResponseCode.WRITE_SYSTEM_PARAMETER_PASS)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("EXCEPTION:" + ex.ToString());
            }
            return false;
        }

        //Perform an Inventory Select to get a list of all tags in the field
        public ArrayList GetTags()
        {
            //start each time with empty list
            tagList.Clear();

            //Build select tag request. 
            STPv3Response response;
            STPv3Request requestTag = new STPv3Request();
            requestTag.Tag = tag;
            requestTag.Command = STPv3Commands.SELECT_TAG;
            requestTag.Inventory = true;

            try
            {
                //Send select tag request and get first response.
                requestTag.Issue(device);
                response = requestTag.GetResponse();
                //while (response == null)
                //{
                    //retry
                //    Thread.Sleep(delay);
                //    response = requestTag.GetResponse();
                //    Console.Out.WriteLine("null resp");
                //}
                if (response == null)
                {
                    Console.Out.WriteLine("null resp");
                    tagList.Clear();
                    return tagList;
                }

                //Continue getting responses and terminate parsing tag ID's upon finding SELECT_TAG_INVENTORY_DONE.
                while (response.ResponseCode != STPv3ResponseCode.SELECT_TAG_INVENTORY_DONE)
                {
                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_PASS)
                    {
                        // Add tag to list
                        string tid = String.Join("", Array.ConvertAll<byte, string>(response.TID, delegate(byte value) { return String.Format("{0:X2}", value); }));
                        tagList.Add(response.TagType + "-" + tid);
                    }
                    response = requestTag.GetResponse();
                    if (response == null)
                    {
                        Console.Out.WriteLine("null resp");
                        tagList.Clear();
                        return tagList;
                    }

                    /*
                    int counter = 0;
                    while (response == null)
                    {
                        //retry
                        Thread.Sleep(delay);
                        response = requestTag.GetResponse();
                        Console.Out.WriteLine("null resp");
                        //counter++;
                        //if (counter > 3)
                        //{
                        //    break;
                        //}
                    }
                    */
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("EXCEPTION:" + ex.ToString());
            }
            return tagList;
        }
    }
}