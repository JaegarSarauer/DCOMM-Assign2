using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;

using SkyeTek.Tags;
using SkyeTek.Devices;
using SkyeTek.STPv3;
using SkyeTek.Readers;


namespace Gen2Lock
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] data = new byte[]{};
            LockController lc = new LockController();
            bool reply;

            byte[] epc1 = new byte[] { 0xAA, 0x00, 0xAA, 0x00, 0xAA, 0x00, 0xAA, 0x00, 0xAA, 0x00, 0xAA, 0x00 };
            byte[] epc2 = new byte[] { 0x00, 0xBB, 0x00, 0xBB, 0x00, 0xBB, 0x00, 0xBB, 0x00, 0xBB, 0x00, 0xBB };

            byte[] passwd_write = new byte[] { 0x56, 0x78, 0x12, 0x34 };
            byte[] passwd_send = new byte[] { 0x12, 0x34, 0x56, 0x78 };
            byte[] passwd_clear = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            //(from the gen2 spec)
            byte[] lock_epc = new byte[] { 0x00, 0x00, 0xC0, 0x20 };
            byte[] lock_clear = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            //select tag
            reply = lc.Select();
            if (reply == true) 
            {
                Console.Out.WriteLine("Select: PASS");
            }
            else
            {
                Console.Out.WriteLine("Select: FAIL");
            }

            //read EPC memory bank
            data = lc.ReadEPC();
            if (data.Length > 0)
            {
                Console.Out.WriteLine("Read EPC: PASS");
            }
            else
            {
                Console.Out.WriteLine("Read EPC: FAIL");
            }

            //write EPC memory bank
            reply = lc.WriteEPC(epc1);
            if (reply == true)
            {
                Console.Out.WriteLine("Write EPC: PASS");
            }
            else
            {
                Console.Out.WriteLine("Write EPC: FAIL");
            }

            //write password
            reply = lc.WritePassword(passwd_write);
            if (reply == true)
            {
                Console.Out.WriteLine("Write Password: PASS");
            }
            else
            {
                Console.Out.WriteLine("Write Password: FAIL");
            }

            //send password
            reply = lc.SendTagPassword(passwd_send);
            if (reply == true)
            {
                Console.Out.WriteLine("Send Password: PASS");
            }
            else
            {
                Console.Out.WriteLine("Send Password: FAIL");
            }

            //lock epc
            reply = lc.WriteLock(lock_epc);
            if (reply == true)
            {
                Console.Out.WriteLine("Lock EPC: PASS");
            }
            else
            {
                Console.Out.WriteLine("Lock EPC: FAIL");
            }

            //select tag
            reply = lc.Select();
            if (reply == true)
            {
                Console.Out.WriteLine("Select: PASS");
            }
            else
            {
                Console.Out.WriteLine("Select: FAIL");
            }

            //read EPC memory bank
            data = lc.ReadEPC();
            if (data.Length > 0)
            {
                Console.Out.WriteLine("Read EPC: PASS");
            }
            else
            {
                Console.Out.WriteLine("Read EPC: FAIL");
            }

            //write EPC memory bank (should fail)
            reply = lc.WriteEPC(epc2);
            if (reply == true)
            {
                Console.Out.WriteLine("Write EPC: PASS");
            }
            else
            {
                Console.Out.WriteLine("Write EPC: FAIL");
            }

            //select tag
            reply = lc.Select();
            if (reply == true)
            {
                Console.Out.WriteLine("Select: PASS");
            }
            else
            {
                Console.Out.WriteLine("Select: FAIL");
            }

            //send password
            reply = lc.SendTagPassword(passwd_send);
            if (reply == true)
            {
                Console.Out.WriteLine("Send Password: PASS");
            }
            else
            {
                Console.Out.WriteLine("Send Password: FAIL");
            }

            //read EPC memory bank
            data = lc.ReadEPC();
            if (data.Length > 0)
            {
                Console.Out.WriteLine("Read EPC: PASS");
            }
            else
            {
                Console.Out.WriteLine("Read EPC: FAIL");
            }

            //write EPC memory bank
            reply = lc.WriteEPC(epc2);
            if (reply == true)
            {
                Console.Out.WriteLine("Write EPC: PASS");
            }
            else
            {
                Console.Out.WriteLine("Write EPC: FAIL");
            }

            //clear epc
            reply = lc.WriteLock(lock_clear);
            if (reply == true)
            {
                Console.Out.WriteLine("Clear EPC Lock: PASS");
            }
            else
            {
                Console.Out.WriteLine("Clear EPC Lock: FAIL");
            }

            //write password
            reply = lc.WritePassword(passwd_clear);
            if (reply == true)
            {
                Console.Out.WriteLine("Write Clear Password: PASS");
            }
            else
            {
                Console.Out.WriteLine("Write Clear Password: FAIL");
            }

            //select tag
            reply = lc.Select();
            if (reply == true)
            {
                Console.Out.WriteLine("Select: PASS");
            }
            else
            {
                Console.Out.WriteLine("Select: FAIL");
            }

            //read EPC memory bank
            data = lc.ReadEPC();
            if (data.Length > 0)
            {
                Console.Out.WriteLine("Read EPC: PASS");
            }
            else
            {
                Console.Out.WriteLine("Read EPC: FAIL");
            }

            //write EPC memory bank
            reply = lc.WriteEPC(epc2);
            if (reply == true)
            {
                Console.Out.WriteLine("Write EPC: PASS");
            }
            else
            {
                Console.Out.WriteLine("Write EPC: FAIL");
            }

            Console.In.ReadLine();
        }
    }


    class LockController
    {
        private Device[] devices;
        private Device device;
        private Tag tag;

        //constructor
        //operates on a usb device
        public LockController()
        {
            devices = USBDeviceFactory.Enumerate();
            if (devices.Length != 0)
            {
                device = devices[0];
                device.Open();
            }
            tag = new Tag();
            tag.Type = TagType.ISO_18000_6C_AUTO_DETECT;
        }

        public bool Select()
        {
            STPv3Request request = new STPv3Request();
            STPv3Response response;
            request.Tag = tag;
            request.Command = STPv3Commands.SELECT_TAG;
            request.Issue(device);

            response = request.GetResponse();
            if (response != null && response.ResponseCode == STPv3ResponseCode.SELECT_TAG_PASS)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public byte[] ReadEPC()
        {
            STPv3Request request = new STPv3Request();
            STPv3Response response;
            request.Tag = tag;
            request.Command = STPv3Commands.READ_TAG;
            request.Address = 0x1002;
            request.Blocks = 6; 
            request.Issue(device);

            response = request.GetResponse();
            if ((response != null) && (response.Success))
            {
                return response.Data;
            }
            else
            {
                return new byte[] { };
            }
        }

        public bool WriteEPC(byte[] epc)
        {
            STPv3Request request = new STPv3Request();
            STPv3Response response;
            request.Tag = tag;
            request.Command = STPv3Commands.WRITE_TAG;
            request.Data = epc;
            request.Address = 0x1002;
            request.Blocks = 6;
            request.Issue(device);

            response = request.GetResponse();
            if ((response != null) && (response.Success))
            {
                return true;
            }
            return false;
        }

        public bool WritePassword(byte[] password)
        {
            STPv3Request request = new STPv3Request();
            STPv3Response response;
            request.Tag = tag;
            request.Command = STPv3Commands.WRITE_TAG;
            request.Data = password;
            request.Address = 0x0002;
            request.Blocks = 2;
            request.Issue(device);

            response = request.GetResponse();
            if ((response != null) && (response.Success))
            {
                return true;
            }
            return false;
        }

        public bool SendTagPassword(byte[] password)
        {
            STPv3Request request = new STPv3Request();
            STPv3Response response;
            request.Tag = tag;
            request.Command = STPv3Commands.SEND_TAG_PASSWORD;
            request.Data = password;
            request.Issue(device);

            response = request.GetResponse();
            if ((response != null) && (response.Success))
            {
                return true;
            }
            return false;
        }

        public bool WriteLock(byte[] lock_data)
        {
            STPv3Request request = new STPv3Request();
            STPv3Response response;
            request.Tag = tag;
            request.Command = STPv3Commands.WRITE_TAG;
            request.Data = lock_data;
            request.Address = 0x0000;
            request.Blocks = 0;
            request.Lock = true;
            request.Issue(device);

            response = request.GetResponse();
            if ((response != null) && (response.Success))
            {
                return true;
            }
            return false;
        }

        public String ByteArrayToString(byte[] data)
        {
            return String.Format(String.Join("", Array.ConvertAll<byte, string>(data, delegate(byte value) { return String.Format("{0:X2}", value); })));
        }
    }

}
