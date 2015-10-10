using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

using SkyeTek.Tags;
using SkyeTek.Devices;
using SkyeTek.STPv3;
using SkyeTek.Readers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SR100SimpleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            STPv3Reader reader = null;
            string s;
            MFDevice myDevice = null;
            MFDevice[] ar = MFDeviceFactory.Enumerate();
            if (ar.Length != 0)
            {
                foreach (MFDevice mfd in ar)
                {
                    s = "";
                    Debug.Print("");
                    Debug.Print("Begin Report");
                    Debug.Print("************************");
                    Debug.Print("IP Address:" + mfd.MFIPEndPoint);
                    s=BitConverter.ToString(mfd.MacAddr).Replace("-", "");
                    Debug.Print("MAC ADDRESS:" + s);
                    Debug.Print("ADDRESS FAMILY:" + mfd.AddrFamily);
                    Debug.Print("REMOTE SOCKET PORT:" + mfd.RemotePort.ToString());
                    if (s == "00409D3D4897") //<--your Device Mac Address here
                    {

                        myDevice = mfd;
                        break;

                    }
                }
                try
                    {
                        
                       //System parameter reads
                        if (myDevice == null)
                        {
                            //My Device is not on the Network
                            Debug.Print("NULL OBJECT ERROR");
                            return;
                        }
                        myDevice.SetReadTimeOut = 500; 
                        reader = new STPv3Reader(myDevice); 
                        reader.Open();
                        
                        Debug.Print("Hardware Version:" + reader.HardwareVersion);
                        Debug.Print("Product Code:" + reader.ProductCode);
                        Debug.Print("Reader Firmware:" + reader.FirmwareVersion);
                        Debug.Print(String.Format("Reader ID: {0}",
                               String.Join("", Array.ConvertAll<byte, string>(reader.ReaderID, delegate(byte value) { return String.Format("{0:X2}", value); }))));
                        Debug.Print("Reader Start Frequency:" + reader.StartFrequency);
                        Debug.Print("Reader Stop Frequency: " + reader.StopFrequency);
                        Debug.Print("Reader Power Level:" + reader.PowerLevel);

                        
                        

                        Debug.Print("INVENTORY EXAMPLE");
                        byte[] r = new byte[1];
                        r[0] = 20; //20 retries, anticipate 10 tags in the field
                        STPv3Response response = null;
                        STPv3Request requestTag = new STPv3Request();
                        requestTag.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;

                        // set reader retries, usually the number of retries should be twice as much as
                        //the anticipated tags in the field!

                        requestTag.Address = 0x0011; 
                        requestTag.Blocks = 0x01;
                        requestTag.Data = r;
                        requestTag.Issue(myDevice);
                        response = requestTag.GetResponse();
                        if(!response.Success)  Debug.Print("Cannot set retries");
                        STPv3Response stpresponse = null;
                        STPv3Request request = new STPv3Request();
                        request.Command = STPv3Commands.SELECT_TAG;
                        request.Inventory = true;
                        Tag tag = new Tag();
                        tag.Type = TagType.AUTO_DETECT;
                        request.Tag = tag;
                        
                   

                       //change the time out for Inventory and Loop modes
                        myDevice.SetReadTimeOut = 20;
                        try
                        {
                            request.Issue(myDevice);
                            while (true)
                            {
                                stpresponse = request.GetResponse();
                                if (stpresponse == null)
                                    Debug.Print("NULL RESPONSE");


                                if (stpresponse.ResponseCode == STPv3ResponseCode.SELECT_TAG_PASS)
                                {
                                    Debug.Print(String.Format("Tag found: {0} -> {1}",
                                    Enum.GetName(typeof(SkyeTek.Tags.TagType),
                                    stpresponse.TagType), String.Join("",
                                    Array.ConvertAll<byte, string>(stpresponse.TID,
                                    delegate(byte value) { return String.Format("{0:X2}", value); }))));
                                }
                                

                                if (stpresponse.ResponseCode == STPv3ResponseCode.SELECT_TAG_INVENTORY_DONE)
                                {
                                    Debug.Print("RECEIVED SELECT_TAG_INVENTORY_DONE");
                                    break;
                                }

                                //Readers return select tag fail as inventory end,  if no tags in field
                                if (stpresponse.ResponseCode == STPv3ResponseCode.SELECT_TAG_FAIL)
                                {
                                    Debug.Print("RECEIVED SELECT_TAG_FAIL");
                                    break;
                                }

                            }
                        }
                        catch (Exception ee)
                        {
                            Debug.Print("Exception "  + ee.Message);
                        }
                       
                        Debug.Print("");
                        Debug.Print("***************  End Report  ************************");
                        reader.Close();



                    }
                    catch (SocketException ex)
                    {

                        Debug.Print(ex.ToString());
                    }
                    catch (Exception e)
                    {

                        Debug.Print("Exception " + e.ToString());
                        reader.Close();
                        

                    }
                }
            }
        }
    }


           