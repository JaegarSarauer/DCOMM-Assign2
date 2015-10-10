using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;

using SkyeTek.Tags;
using SkyeTek.Devices;
using SkyeTek.STPv3;
using SkyeTek.Readers;


namespace MultiThreadedUSB
{
    class Program
    {
        static void Main(string[] args)
        {
            int numTests = 10000;

            controller c0 = new controller(0);
            controller c1 = new controller(1);

            c0.SetIterations(numTests);
            c1.SetIterations(numTests);

            Thread t0 = new Thread(new ThreadStart(c0.RunTest));
            Thread t1 = new Thread(new ThreadStart(c1.RunTest));
            
            t0.Start();
            t1.Start();
            
            while (!t0.IsAlive && !t1.IsAlive) ;
            Console.In.ReadLine();
        }
    }


    class controller
    {
        private int errors;
        private Tag tag;
        private Device[] devices;
        private Device device;
        private List<byte[]> byteArrayTagList;
        private int reader_num;
        private int iterations;

        public controller(int i)
        {
            errors = 0;
            tag = new Tag();
            tag.Type = TagType.ISO_18000_6C_AUTO_DETECT;
            devices = USBDeviceFactory.Enumerate();
            reader_num = i;
            device = devices[i];
            byteArrayTagList = new List<byte[]>();
            device.Open();
        }

        public void SetIterations(int i)
        {
            iterations = i;
        }

        public int GetErrorCount()
        {
            return errors;
        }

        public Boolean SetMux(byte port)
        {
            byte[] p = new byte[1];
            p[0] = port;
            STPv3Response response;
            STPv3Request requestMux = new STPv3Request();
            requestMux.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
            requestMux.Address = 0x000A;
            requestMux.Blocks = 0x01;
            requestMux.Data = p;

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
                throw;
            }

            Console.Out.WriteLine("********** SetMux FAILED ***********");
            errors++;
            return false;
        }

        public List<byte[]> GetTagsByteArray()
        {
            byteArrayTagList.Clear();

            STPv3Response response;
            STPv3Request requestTag = new STPv3Request();
            requestTag.Tag = tag;
            requestTag.Command = STPv3Commands.SELECT_TAG;
            requestTag.Inventory = true;

            try
            {
                requestTag.Issue(device);
                response = requestTag.GetResponse();
                if (response == null)
                {
                    Console.Out.WriteLine("********** GetTags NULL RESPONSE ***********");
                    errors++;
                    return byteArrayTagList;
                }

                while (response.ResponseCode != STPv3ResponseCode.SELECT_TAG_INVENTORY_DONE)
                {
                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_PASS)
                    {
                        byteArrayTagList.Add(response.TID);
                    }

                    response = requestTag.GetResponse();
                    if (response == null)
                    {
                        Console.Out.WriteLine("********** GetTags NULL RESPONSE ***********");
                        errors++;
                        return byteArrayTagList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("EXCEPTION:" + ex.ToString());
                throw;
            }
            return byteArrayTagList;
        }

        public void RunTest()
        {
            try
            {
                List<byte[]> taglist = new List<byte[]>();
                //byte[] muxPorts = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
                byte[] muxPorts = new byte[] { };

                for (int i = 0; i < iterations; i++)
                {
                    Console.Out.WriteLine("READER_" + reader_num + ", Test#" + i);
                    //foreach (byte p in muxPorts)
                    //{
                        //if (this.SetMux(muxPorts[p]))
                        //{
                            Console.Out.WriteLine("\tREADER_" + reader_num);// + ", Port:" + p.ToString());
                            taglist = this.GetTagsByteArray();
                            foreach (byte[] k in taglist)
                            {
                                string tid = String.Join("", Array.ConvertAll<byte, string>(k, delegate(byte value) { return String.Format("{0:X2}", value); }));
                                Console.Out.WriteLine("\t\tREADER_" + reader_num + ", Tag Found: " + tid);
                            }
                        //}
                    //}
                }
                Console.Out.WriteLine(this.GetErrorCount() + "READER_" + reader_num + ", " + this.errors + " response errors out of " + iterations * 8 * 2 + " requests (" + (float)this.GetErrorCount() / (iterations * 8 * 2) * 100 + "%)");
            }
            catch
            {
                throw;
            }

        }
    }
}
