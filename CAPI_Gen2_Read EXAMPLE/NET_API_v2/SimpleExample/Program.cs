using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

using SkyeTek.Tags;
using SkyeTek.Devices;
using SkyeTek.STPv3;
using SkyeTek.Readers;

namespace SkyeTek
{
    class SimpleExample
    {
        static void Main(string[] args)
        {
            SerialDevice sd;
            STPv3Reader reader;
            STPv3Request request;
            STPv3Response response;
            Tag tag = new Tag();
            tag.Type = TagType.AUTO_DETECT;
            string port;
            byte[] resp;
            float temp;

            if (args.Length < 1)
                port = "COM1";
            else
                port = args[0];

            //this example uses the serial port
            sd = new SerialDevice();
            reader = new STPv3Reader(sd);

            try
            {
                sd.Address = port;
                sd.BaudRate = 38400;
                sd.Open();

                // read product code, reader name, hw version, fw version, and reader ID
                Console.Out.WriteLine(String.Format("Product Code: {0}", reader.ProductCode));
                Console.Out.WriteLine(String.Format("Reader Name: {0}", reader.ReaderName));
                Console.Out.WriteLine(String.Format("Hardware Version: {0}", reader.HardwareVersion));
                Console.Out.WriteLine(String.Format("Firmware Version: {0}", reader.FirmwareVersion));
                Console.Out.WriteLine(String.Format("Reader ID: {0}",
                    String.Join("", Array.ConvertAll<byte, string>(reader.ReaderID, delegate(byte value){ return String.Format("{0:X2}", value); }))));

                //scan for tags
                request = new STPv3Request();
                request.Tag = tag;
                request.Command = STPv3Commands.SELECT_TAG;
                request.Inventory = true;
                request.Issue(sd);

                while (((response = request.GetResponse()) != null) && (response.Success))
                {
                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_PASS)
                    {
                        Console.Out.WriteLine(String.Format("Tag found: {0} -> {1}",
                            Enum.GetName(typeof(SkyeTek.Tags.TagType),
                            response.TagType), String.Join("",
                            Array.ConvertAll<byte, string>(response.TID,
                            delegate(byte value) { return String.Format("{0:X2}", value); }))));
                    }    
                }
            }

            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }

            Console.In.ReadLine();

        }
    }
}
