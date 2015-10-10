using System;
using System.Collections.Generic;
using System.Text;
 
using System.IO;
using System.IO.Ports;

#if !PocketPC && !Mono
//using GenericHid;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
#endif

using GenericHid;
//using System.Linq;
using System.Data;
using System.Diagnostics;
//needed for TCP/IP
//using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Collections;
//using System.Threading;





namespace SkyeTek
{
    namespace Devices
    {
        /// <summary>
        /// Supported device types.  Not all device types are supported on all platforms
        /// </summary>
        public enum DeviceType
        {
            SERIAL,
            USB,
            BLUETOOTH,
            TCP  //Lachezar Temelkov 01.19.2010
        }

        public delegate void DataReceivedEventHandler(object sender, EventArgs e);

        public abstract class Device : System.IO.Stream
        {
            public abstract void Open();
            public abstract string Address { get; set; }
            public abstract DeviceType Type { get; }
           // public abstract int ReadTimeout { get; set;}
            //public abstract int WriteTimeout { get; set;}
            //public abstract void Close();
            public abstract bool IsOpen { get; }
            public abstract event DataReceivedEventHandler DataReceived;
            public virtual bool isDataAvailable
            {
                get
                {
                    return false;
                }

            }
            public virtual int AvailableTCPDataBytes
            {
                get
                {
                    return -1;
                }
            }
            public virtual bool TcpPollData(int microsec)
            {
                return false;

            }

            public virtual int SetReadTimeOut
            {
                set
                {
                    ;
                }

            }
        }

        /// <summary>
        /// Encapsulates a serial device.
        /// </summary>
        public class SerialDevice : Device
        {
            private SerialPort m_serialPort;
            private System.IO.Stream comportStream;

            public SerialDevice()
            {
                this.m_serialPort = new SerialPort();
#if !PocketPC
                if (System.Environment.OSVersion.Platform == PlatformID.Unix)
                    this.m_serialPort.PortName = "/dev/ttyS0";
                else
#endif
                this.m_serialPort.PortName = "COM1";
                this.m_serialPort.BaudRate = 38400;
                this.m_serialPort.ReadTimeout = 2000;
                this.m_serialPort.Handshake = Handshake.None;
                this.m_serialPort.DataBits = 8;
                this.m_serialPort.Parity = Parity.None;
                this.m_serialPort.StopBits = StopBits.One;
            }
            ~SerialDevice()
            {
                try
                {
                    
                 Dispose(true);
                }
                catch (Exception e)
                {


                }
                finally
                {
                    m_serialPort = null;

                }
            }

            public int BaudRate
            {
                get { return this.m_serialPort.BaudRate; }
                set { this.m_serialPort.BaudRate = value; }
            }

            protected override void Dispose(bool disposing)
            {
                if (this.m_serialPort.IsOpen)
                {
                    this.comportStream.Close();
                    this.m_serialPort.Close();
                }

                base.Dispose(disposing);
            }

            public override event DataReceivedEventHandler DataReceived
            {
                add
                {
                    this.m_serialPort.DataReceived += new SerialDataReceivedEventHandler(value);
                }

                remove
                {
                    this.m_serialPort.DataReceived -= new SerialDataReceivedEventHandler(value);
                }
            }

            #region Device Accessors
            /// <summary>
            /// Specifies the time in milliseconds before a read operation times out. 
            /// This must be set before opening the device
            /// </summary>
            public override int ReadTimeout
            {
                get { return this.m_serialPort.ReadTimeout; }
                set { this.m_serialPort.ReadTimeout = value; }
            }

            /// <summary>
            /// Shows if there is bytes in the buffer
            /// </summary>
            public override bool isDataAvailable
            {
                get
                {
                    if (this.m_serialPort != null)
                    {
                        if (this.m_serialPort.BytesToRead != 0)
                            return true;
                        else
                            return false;

                    }
                    else
                        return false;
                }

            }

            /// <summary>
            /// Specifies the time in milliseconds before a write operation times out. 
            /// This must be set before opening the device.
            /// </summary>
            public override int WriteTimeout
            {
                get { return this.m_serialPort.WriteTimeout; }
                set { this.m_serialPort.WriteTimeout = value; }
            }

            public override DeviceType Type
            {
                get { return DeviceType.SERIAL; }
            }

            /// <summary>
            /// Set and get the address of this serial device.  For this particular implmentation,
            /// the address is the name or path to the serial port.
            /// </summary>
            public override string Address
            {
                get { return this.m_serialPort.PortName; }
                set 
                {
                    string[] serialPorts = SerialPort.GetPortNames();

                    foreach (string sp in serialPorts)
                    {
                        if (sp == value)
                        {
                            if (this.IsOpen)
                                this.Close();

                            this.m_serialPort.PortName = value;
                            return;
                        }
                    }

                    throw new ArgumentException("The address provided is not a valid serial port");                    
                }
            }
            public override bool IsOpen
            {
                get { return this.m_serialPort.IsOpen; }
            }
            #endregion

            #region Device Functions
            public override void Open()
            {
                this.m_serialPort.Open();
                comportStream = this.m_serialPort.BaseStream;
            }

            public override void Close()
            {
                this.comportStream.Close();
                this.m_serialPort.Close();
            }

            #endregion

            #region System.IO.Stream Functions

            
            public override int ReadByte()
            {
                return this.m_serialPort.ReadByte();
            }
            public override void Flush()
            {
                this.m_serialPort.BaseStream.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return this.m_serialPort.BaseStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                this.m_serialPort.BaseStream.SetLength(value);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return this.m_serialPort.Read(buffer, offset, count);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                this.m_serialPort.Write(buffer, offset, count);
            }

            #endregion

            #region System.IO.Stream Accessors

            public override bool CanRead
            {
                get
                {
                    return this.m_serialPort.BaseStream.CanRead;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return this.m_serialPort.BaseStream.CanSeek;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return this.m_serialPort.BaseStream.CanWrite;
                }
            }

            public override long Length
            {
                get
                {
                    return this.m_serialPort.BytesToRead;
                }
            }

            public override long Position
            {
                get
                {
                    return this.m_serialPort.BaseStream.Position;
                }

                set
                {
                    this.m_serialPort.BaseStream.Position = value;
                }
            }




            #endregion
        }

        public static class SerialDeviceFactory
        {
            /// <summary>
            /// Enumerates serial ports on host.  These ports may or may not have readers attached to them
            /// </summary>
            /// <returns>Array of Devices.  They have not been opened yet (i.e. device.Open())</returns>

            public static SerialDevice[] Enumerate()
            {
                List<SerialDevice> devices;

                devices = new List<SerialDevice>();

                string[] serialPorts = SerialPort.GetPortNames();

                foreach(string sp in serialPorts)
                {
                    SerialDevice sd = new SerialDevice();
                    sd.Address = sp;
                    devices.Add(sd);
                }

                return devices.ToArray();
            }
        }



        /// <summary>
        /// Encapsulate a TCP/IP device, Running dot net micro framework
        /// </summary>
        public class MFDevice : Device
        {

            private int m_SkyeTekTCPPort = 2000; //TCP/IP port - can be changed later
            public int TCPport
            {

                get
                {
                    return m_SkyeTekTCPPort;

                }
            }
            private Socket TCPsocket = null;
            //private TcpClient TCPsocket = null;
            private NetworkStream ns = null;
            private string mfendpointip;
            private int streamtimeout =-1;
            /// <summary>
            /// Sets and Gets Endpoint IP address.
            /// </summary>
            public string MFIPEndPoint
            {
                get
                {
                    return mfendpointip;
                }
                set
                {
                    mfendpointip = value;
                }

            }

            private int rPort;
            /// <summary>
            /// return the remote TCP/IP port
            /// </summary>
            public int RemotePort
            {
                get
                {
                    return rPort;
                }


            }

            /// <summary>
            /// Sets or gets the blocking propery on the client socket
            /// </summary>
            //public bool isBlocking
            //{

            //    set
            //    {
            //        TCPsocket.Client.Blocking = value;
            //    }
            //    get
            //    {
            //        return TCPsocket.Client.Blocking;
            //    }


            //}


            public override int SetReadTimeOut
            {
                set
                {
                    streamtimeout = value;
                }


            }

            private string adrFamily;
            /// <summary>
            /// gets the addres family property of the Discovery UDP socket
            /// </summary>
            public string AddrFamily
            {
                get
                {
                    return adrFamily;

                }

            }

            private byte[] mAddr;
            /// <summary>
            /// Gets the MAC address of the remote Server.
            /// Remote server has to be discovered first
            /// </summary>
            public byte[] MacAddr
            {
                get
                {
                    return mAddr;

                }
            }


            #region Helper methods
            /// <summary>
            /// method to validate an IP address
            /// using regular expressions. The pattern
            /// being used will validate an ip address
            /// with the range of 1.0.0.0 to 255.255.255.255
            /// </summary>
            /// <param name="addr">Address to validate</param>
            /// <returns></returns>
            private bool IsValidIP(string addr)
            {
                //create our match pattern
                string pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.
                             ([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";
                //create our Regular Expression object
                Regex check = new Regex(pattern);
                //boolean variable to hold the status
                bool valid = false;
                //check to make sure an ip address was provided
                if (addr == "")
                {
                    //no address provided so return false
                    valid = false;
                }
                else
                {
                    //address provided so use the IsMatch Method
                    //of the Regular Expression object
                    valid = check.IsMatch(addr, 0);
                }
                //return the results
                return valid;
            }

            public override bool isDataAvailable
            {
                get
                {
                    if (ns != Null)
                        return ns.DataAvailable;
                    else
                        return false;
                }



            }

            public override int AvailableTCPDataBytes
            {
                get
                {
                    if (TCPsocket != null)
                        return TCPsocket.Available;
                    else
                        return -1;
                }


            }

            public override bool TcpPollData(int microsec)
            {

                if (TCPsocket != null)
                {
                    return (TCPsocket.Poll(microsec, SelectMode.SelectRead));

                }
                return false;
            }
            #endregion

            #region Events and Delegates
            /// <summary>
            /// Synchronous TCP sockets dont implement that.
            /// Use multithreads instead
            /// </summary>
            /// <param name="addr">Address to validate</param>
            /// <returns></returns>
            public override event DataReceivedEventHandler DataReceived
            {
                add { }
                remove { }
            }
            #endregion

            #region Device Functions
            /// <summary>
            /// Opens up a connection to the TCP socket Server, and the underlying stream
            /// </summary>
            public override void Open()
            {//open a synchronous blocking socket
                if (mfendpointip == "")
                    throw new ArgumentException("Can not open TCP socket. No IP provided.Use MFDeviceFactury.Enumerate");
                ///////// UDP////////
                if (MacAddr == null) 
                   throw new ArgumentException("Invalid Server Mac Address");
                Socket ss = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPHostEntry entry = Dns.GetHostEntry(this.MFIPEndPoint);
                IPAddress DigiIpAddress = entry.AddressList[0];
                EndPoint ep = new IPEndPoint(DigiIpAddress, 2019);
                ss.SendTo(this.MacAddr, ep);
                Thread.Sleep(1500);//allow device to reset


                /////////////////////

               // TCPsocket = new Socket(AddressFamily.InterNetwork,
                                    //   SocketType.Stream, ProtocolType.Tcp);
                //TCPsocket = new TcpClient();
                TCPsocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);

                
                IPAddress ipAddress = IPAddress.Parse(mfendpointip);
                IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, m_SkyeTekTCPPort);
                TCPsocket.Connect(serverEndPoint);
                ns = new NetworkStream(TCPsocket, FileAccess.ReadWrite,true);
                TCPsocket.ReceiveBufferSize = 10000;
               
               
                
               
                





            }
            /// <summary>
            /// Closses connection between Client and Server
            /// </summary>
            public override void Close()
            {
                
                if (ns != null)
                {
                    ns.Dispose();
                    ns.Close();
                    ns = null;
                }
            }
            /// <summary>
            /// Determines if the Connection already exists.
            /// </summary>
            public override bool IsOpen
            {
                get
                {
                    if (TCPsocket != null)
                    {
                        //this.isBlocking = true;
                        return TCPsocket.Connected;
                    }
                    else
                        return false;
                }
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                return this.ns.Seek(offset, origin);
            }
            public override void SetLength(long value)
            {
                this.ns.SetLength(value);
            }
            /// <summary>
            /// Reads a single byte off the stream  ,once connection is open.
            /// </summary>
            /// <returns> -1 if end of stream</returns>
            public override int ReadByte()
            {
                ns.ReadTimeout = streamtimeout;
                return (this.ns.ReadByte());
               
            }
            /// <summary>
            /// writes a single Byte to the stream
            /// </summary>
            /// <param name="value"></param>
            public override void WriteByte(byte value)
            {
                this.ns.WriteByte(value);
            }
            #endregion

            #region Device Accessors
            /// <summary>
            /// Specifies the TCP/IP Receive timeout time in milliseconds. 
            /// The default is 500ms for TCP Socket operation.
            /// </summary>
            public override int ReadTimeout
            {
                get { return this.streamtimeout; }
                set 
                {
                    
                    this.streamtimeout = value;
                    
                }
            }
            /// <summary>
            /// Specifies the time in milliseconds before a write operation times out. 
            /// This must be set before opening the device.
            /// This operation does not do anything when used in TCP socket or
            /// Skyetek Ethernet enabled devices
            /// </summary>
            public override int WriteTimeout
            {
                get { return this.ns.WriteTimeout; }
                set { this.ns.WriteTimeout = value; }
            }

            /// <summary>
            /// Sets and gets IP string address of a remote server. 
            /// if the IP address of the remote server is not known. 
            /// use MFDiscovery.Enumerate()
            /// </summary>
            public override string Address
            {
                get
                {
                    return mfendpointip;
                }
                set
                {
                    if (IsValidIP(value))
                        mfendpointip = value;
                    else
                        throw new ArgumentException(" Invalid IPv4 address ex: \'255.255.255.255\' ");
                }
            }
            /// <summary>
            /// Gets the Device type over which the current communication takes place 
            /// </summary>
            public override DeviceType Type
            {
                get { return DeviceType.TCP; }
            }


            #endregion

            #region System.IO.Stream Accessors

            /// <summary>
            /// property, showing if the device is capable of reading  IO stream
            /// <returns>true for TCP/IP socket</returns>
            public override bool CanRead
            {
                get { return this.ns.CanRead; }
            }
            public override bool CanSeek
            {
                get
                {

                    return this.ns.CanSeek;
                }
            }
            public override bool CanWrite
            {
                get
                {
                    return this.ns.CanWrite;
                }
            }
            public override long Length
            {
                get { return this.ns.Length; }
            }
            public override long Position
            {
                get { return this.ns.Position; }
                set { this.ns.Position = value; }
            }
            


            #endregion

            #region System.IO.Stream Methods
            /// <summary>
            /// writes a buffer to Stream
            /// </summary>
            /// <param name="buffer"> reference to a buffer, holding data</param>
            /// <param name="offset"> denotes beginning offset of the buffer</param>
            /// <param name="count">number of bytes to be sent</param>
            public override void Write(byte[] buffer, int offset, int count)
            {
                ns.Write(buffer, offset, count);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                ns.ReadTimeout = streamtimeout;
                return ns.Read(buffer, offset, count);
                
            }

            public override void Flush()
            {
                this.ns.Flush();
            }


            #endregion

            //constructor
            public MFDevice()
            {
                //init the Network fields
                mfendpointip = "";
                rPort = 0;
                adrFamily = "Not Available";
                mAddr = new byte[6];
                streamtimeout = -1;

            }


            //set only properties - unavailable to user, only available within  dll
            internal void setPointRemoteIP(string remoteIP)
            {
                mfendpointip = remoteIP;
            }

            internal void setPointRemotePort(int remotePort)
            {
                rPort = remotePort;
            }

            internal void setPointRemoteAdrFamily(string addressFamily)
            {
                adrFamily = addressFamily;
            }


            internal void GetMacAddr(byte[] mac)
            {
                for (int x = 0; x < mac.Length; x++)
                {
                    mAddr[x] = mac[x];
                }
            }
        }
       /// <summary>
       /// Device Factory for SkyeTek TCP/IP devices
       /// </summary>
        public static class MFDeviceFactory
        {
            /// <summary>
            /// Enumerates Micro Framework devices
            /// </summary>
            /// <returns>Array of Devices.</returns>

            public static MFDevice[] Enumerate()
            {
                List<MFDevice> devices;
                Discovery d = new Discovery();
                devices = d.EnumPorts(TransportType.Udp);
                return devices.ToArray();
            }
        }
        /// <summary>
        /// Helper discovery class
        /// </summary>
        internal class Discovery : IDisposable
        {

            //Fields 
            private List<MFDevice> m_deviceList = new List<MFDevice>();
            private IPAddress m_DiscoveryMulticastAddress = IPAddress.Parse("234.102.98.44");    //Udp sent    IP
            private IPAddress m_DiscoveryMulticastAddressRecv = IPAddress.Parse("234.102.98.45");//Udp receive IP 
            private int m_DiscoveryMulticastPort = 0x6591;         // Udp sent port - destination, udp listen port
            private string m_DiscoveryMulticastToken = "DOTNETMF"; //udp sent discovery data


            //// Properties
            public IList DeviceList
            {
                get
                {
                    ArrayList list = new ArrayList();
                    list.AddRange(this.m_deviceList);
                    return list;
                }
            }

            public IPAddress DiscoveryMulticastAddress
            {
                get
                {
                    return this.m_DiscoveryMulticastAddress;
                }
                set
                {
                    this.m_DiscoveryMulticastAddress = value;
                }
            }
            public IPAddress DiscoveryMulticastAddressRecv
            {
                get
                {
                    return this.m_DiscoveryMulticastAddressRecv;
                }
                set
                {
                    this.m_DiscoveryMulticastAddressRecv = value;
                }
            }
            public int DiscoveryMulticastPort
            {
                get
                {
                    return this.m_DiscoveryMulticastPort;
                }
                set
                {
                    this.m_DiscoveryMulticastPort = value;
                }
            }
            public string DiscoveryMulticastToken
            {
                get
                {
                    return this.m_DiscoveryMulticastToken;
                }
                set
                {
                    this.m_DiscoveryMulticastToken = value;
                }
            }




            public void Dispose()
            {
                this.Dispose();
                GC.SuppressFinalize(this);
            }
            public List<MFDevice> EnumPorts(TransportType type)
            {
                /*
                We need to create two socket for discovery Udp multicast
                socket  sends discovery message
                socket2 receive messages
                */
                if (type != TransportType.Udp) //sanity check
                    type = TransportType.Udp;

                this.m_deviceList.Clear();          //start anew

                try
                {

                    foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            int num = 0;          // num of bytes return with receive socket
                            int index = 0;        // denotes the beginning of each stream from MF modules
                            int iy = 0;           // counts macaddress bytes
                            byte[] buffer = new byte[0x400]; //data buffer
                            byte[] macarr = new byte[0x06];  //mac address from data buffer
                            Socket socket = null; //tx socket on host
                            Socket socket2 = null;//rx socket on host
                            IPEndPoint localEP = new IPEndPoint(address, 0);
                            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, this.m_DiscoveryMulticastPort);
                            IPEndPoint point3 = new IPEndPoint(address, this.m_DiscoveryMulticastPort);
                            IPEndPoint point4 = new IPEndPoint(this.m_DiscoveryMulticastAddress, this.m_DiscoveryMulticastPort);
                            try
                            {
                                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                socket2.Bind(point3);
                                socket2.ReceiveTimeout = 0x3e8;

                                socket2.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(this.m_DiscoveryMulticastAddressRecv, address));
                                socket.Bind(localEP);
                                socket.MulticastLoopback = false;
                                socket.Ttl = 1;
                                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 0x40);
                                socket.SendTo(Encoding.ASCII.GetBytes(this.m_DiscoveryMulticastToken), SocketFlags.None, point4);

                                while (0 < (num = socket2.ReceiveFrom(buffer, index, buffer.Length - index, SocketFlags.None, ref remoteEP)))
                                {
                                    MFDevice mfDev = new MFDevice();
                                    mfDev.setPointRemoteIP(((IPEndPoint)remoteEP).Address.ToString()); //add Remote IP
                                    mfDev.setPointRemotePort(((IPEndPoint)remoteEP).Port);             //add Remote Port
                                    mfDev.setPointRemoteAdrFamily(((IPEndPoint)remoteEP).AddressFamily.ToString());  //add Remote Type
                                    //get/add mac address
                                    iy = 0;
                                    while (iy < 6)
                                    {
                                        macarr[iy] = buffer[index + 8 + iy];
                                        iy++;
                                    }
                                    mfDev.GetMacAddr(macarr);                                           //add Remote Mac Address
                                    m_deviceList.Add(mfDev);     //add object to Factory 
                                    index += num;                // new ofset- remote dev answer simultaniously to multicast
                                    socket2.ReceiveTimeout = 200;//timeout of 200ms
                                }
                                socket2.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(this.m_DiscoveryMulticastAddressRecv));
                            }
                            catch (SocketException)
                            {
                            }
                            finally
                            {
                                if (socket2 != null)
                                {
                                    socket2.Close();
                                    socket2 = null;
                                }
                                if (socket != null)
                                {
                                    socket.Close();
                                    socket = null;
                                }
                            }


                        }
                    }
                }
                catch (Exception)
                {
                }
                return m_deviceList;
            }
        }
       
       
#if !PocketPC && !Mono
        public static class USBDeviceFactory
        {
            /*
            public static Device[] Enumerate()
            {
                List<Device> devices;
                devices = new List<Device>();
                USBDevice device;

                skyetek_hid h = new skyetek_hid();
                //string[] d = new string[];
                string[] d = h.FindTheHid();
                Console.Out.WriteLine("Device Count" + devices.Count);
                for (int i = 0; i < d.Length; i++)
                {
                    device = new USBDevice(d[i]);
                    devices.Add(device);
                }

                //Console.Out.WriteLine("Device Count" + devices.Count);
                return devices.ToArray();
            }
             */

            public static Device[] Enumerate()
            {
                List<Device> devices;
                devices = new List<Device>();
                USBDevice device;
                String emptyPath = "";

                skyetek_hid h = new skyetek_hid(emptyPath);
                //string[] d = new string[];
                //string[] d = h.FindTheHids();
                string[] d = h.GetGuids();
                //Console.Out.WriteLine("Device Count" + devices.Count);
                for (int i = 0; i < d.Length; i++)
                {
                    device = new USBDevice(d[i]);
                    devices.Add(device);
                }

                //Console.Out.WriteLine("Device Count" + devices.Count);
                return devices.ToArray();
            }
        }


        public class USBDevice : Device
        {
            private FileStream fs;
            private string path;
            private Queue<byte> readBuffer, writeBuffer;
            int writeBufferCount;
            skyetek_hid hd;

            public USBDevice(string path)
            {
                
                hd = new skyetek_hid(path);
                this.Address = path;
                //this.readHandle = this.writeHandle = IntPtr.Zero;
                this.readBuffer = new Queue<byte>(65);
                this.writeBuffer = new Queue<byte>(65);
            }
            public override void Open()
            {
                hd.Open();
            }

            public override void Close()
            {
            }

            public override event DataReceivedEventHandler DataReceived
            {
                add { }
                remove { }
            }

            public override unsafe void Flush()
            {
                //readBuffer.Clear();
                hd.RW(writeBuffer, writeBufferCount);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                writeBuffer.Clear();
               int writeSize, ix;

               while (count > 0)
               {
                   writeSize = (64 - this.writeBuffer.Count);
                   writeSize = (count > writeSize) ? writeSize : count;
                   count -= writeSize;
                   for (ix = 0; ix < writeSize; ix++)
                       this.writeBuffer.Enqueue(buffer[offset + ix]);

                   if (this.writeBuffer.Count == 64)
                       this.Flush();
               }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                //int bytesRead = 0;
                //return bytesRead;
                int bytesRead = 0;
                int readSize, ix;

                //this.Flush();
                //Thread.Sleep(10);
                //this.readBuffer = hd.GetReadBuffer();
                //CopyTo(this.readBuffer, 0);
                //this.readBuffer
                while (true)
                {
                    
                    if (this.readBuffer.Count > 0)
                    {
                        readSize = (this.readBuffer.Count > count) ? count : this.readBuffer.Count;
                        bytesRead += readSize;
                        count -= readSize;
                        for (ix = 0; ix < readSize; ix++)
                            buffer[offset + ix] = this.readBuffer.Dequeue();
                    }

                    //if(this.readBuffer.Count == 0)
                    if (count == 0)
                        break;

                    if (!this.FillReceiveBuffer())
                        break;

                    //if (readBuffer.Count == 0)
                    //    break;
                }
                //buffer.CopyTo(buffer, 2);
                return bytesRead;
            }


            public override int ReadByte()
            {
                hd.ClearReadBuffer();
                byte[] buffer = new byte[1];

                int count = this.Read(buffer, 0, 1);

                if (count > 0)
                    return buffer[0];
                else
                    return 0;

            }
            /*
            public override int ReadByte()
            {
                byte[] buffer = new byte[65];

                //int count = this.Read(buffer, 0, 1);
                
                this.readBuffer = hd.GetReadBuffer();
                int count = this.readBuffer.Count;

                //if(count > 0)
                if (count > 2)
                {
                    this.readBuffer.CopyTo(buffer,0);
                    return buffer[0];
                }
                else
                    return 0;

            }
             */

            private unsafe bool FillReceiveBuffer()
            {
                //Thread.Sleep(20);
                hd.FillRxBuffer();
                this.readBuffer = hd.GetReadBuffer();
                //this.readBuffer = hd.GetReadBuffer();
                if (this.readBuffer.Count == 0)
                    return false;
                else
                    return true;
                
                /*
                byte[] buffer;
                Win32USB.OVERLAPPED ovl;
                int bytesRead;
                int ix;

                buffer = new byte[65];

                ovl = new Win32USB.OVERLAPPED();
                ovl.hEvent = Win32USB.CreateEvent(IntPtr.Zero, true, false, null);

                bytesRead = 0;

                fixed (byte* p = buffer)
                {
                    Win32USB.ReadFile(this.readHandle, p, 65, &bytesRead, &ovl);
                }

                if (Win32USB.WaitForSingleObject(ovl.hEvent, 300) != Win32USB.WAIT_OBJECT_0)
                {
                    Win32USB.CancelIo(this.readHandle);
                    Win32USB.CloseHandle(ovl.hEvent);
                    return false;
                }

                Win32USB.CloseHandle(ovl.hEvent);

                if (!Win32USB.GetOverlappedResult(this.readHandle, &ovl, &bytesRead, false))
                    return false;

                if (bytesRead != 65)
                    return false;

                for (ix = 0; ix < buffer[1]; ix++)
                    this.readBuffer.Enqueue(buffer[ix + 2]);
                */
            }


            public override long Seek(long offset, SeekOrigin origin)
            {
                return 0;
            }

            public override int ReadTimeout
            {
                get { return this.fs.ReadTimeout; }
                set { this.fs.ReadTimeout = value; }
            }

            public override int WriteTimeout
            {
                get { return this.fs.WriteTimeout; }
                set { this.fs.WriteTimeout = value; }
            }

            public override DeviceType Type
            {
                get { return DeviceType.USB; }
            }

            /// <summary>
            /// Address of the usb device.  Address is the system path to the device.
            /// </summary>
            public override string Address
            {
                
                get { return this.path; }
                set { this.path = value; }
                /*
                set
                {
                    if (!value.ToLower().Contains("vid_afef") || !value.ToLower().Contains("pid_0f01"))
                        throw new ArgumentException("Address does not correspond to a known device");

                    this.path = value;
                }
                */
            }
            public override bool IsOpen
            {
                //get { return this.readHandle != null; }
                get { return true; }
            }

            public override void SetLength(long value) { }

            public override bool CanRead
            {
                get { return this.fs.CanRead; }
            }

            public override bool CanSeek
            {
                get { return this.fs.CanSeek; }
            }

            public override bool CanWrite
            {
                get { return this.fs.CanWrite; }
            }

            public override long Length
            {
                get { return this.fs.Length; }
            }

            public override long Position
            {
                get { return this.fs.Position; }
                set { }
            }


        }
#endif


        class skyetek_hid
        {
            //private IntPtr deviceNotificationHandle;
            private Boolean exclusiveAccess;
            private SafeFileHandle hidHandle;
            private String hidUsage;
            private Boolean myDeviceDetected;
            private String myDevicePathName;
            private SafeFileHandle readHandle;
            private SafeFileHandle writeHandle;
            public byte[] buf1 = new byte[65];
            private byte[] receiveBuffer = new byte[65];
            private Queue<byte> readBuffer;
            bool ReadFinished;
            int ReadStart;

            private Debugging MyDebugging;
            private DeviceManagement MyDeviceManagement;
            private Hid MyHid;

            public skyetek_hid(String path)
            {
                MyDebugging = new Debugging(); //  For viewing results of API calls via Debug.Write.
                MyDeviceManagement = new DeviceManagement();
                MyHid = new Hid();
                readBuffer = new Queue<byte>(65);
                ReadFinished = false;
                ReadStart = 0;
                myDevicePathName = path;
            }

            ///  <summary>
            ///  Define a class of delegates that point to the Hid.ReportIn.Read function.
            ///  The delegate has the same parameters as Hid.ReportIn.Read.
            ///  Used for asynchronous reads from the device.       
            ///  </summary>
            private delegate void ReadInputReportDelegate(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] readBuffer, ref Boolean success);

            //  This delegate has the same parameters as AccessForm.
            //  Used in accessing the application's form from a different thread.
            private delegate void MarshalToForm(String action, String textToAdd);

            /*
            try 
            { 
                FindTheHid();                 
            } 
            catch ( Exception ex ) 
            { 
                DisplayException( this.Name, ex ); 
                throw ; 
            } 
             */



            /*
            try 
            { 
                //  Close open handles to the device.
                
                if ( !( hidHandle == null ) ) 
                { 
                    if ( !( hidHandle.IsInvalid ) ) 
                    { 
                        hidHandle.Close(); 
                    } 
                } 
                
                if ( !( readHandle == null ) ) 
                { 
                    if ( !( readHandle.IsInvalid ) ) 
                    { 
                        readHandle.Close(); 
                    } 
                } 
                
                if ( !( writeHandle == null ) ) 
                { 
                    if ( !( writeHandle.IsInvalid ) ) 
                    { 
                        writeHandle.Close(); 
                    } 
                } 
                
                //  Stop receiving notifications.
                
                MyDeviceManagement.StopReceivingDeviceNotifications( deviceNotificationHandle );                 
            } 
            catch ( Exception ex ) 
            { 
                DisplayException( this.Name, ex ); 
                throw ; 
            }  
             */

            public void Open()
            {
                try
                {
                    //Returns: a handle without read or write access. This enables obtaining information about all HIDs, even system keyboards and mice. 
                    hidHandle = FileIO.CreateFile(myDevicePathName, 0, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                    if (!hidHandle.IsInvalid)
                    {
                        //  Set the Size property of DeviceAttributes to the number of bytes in the structure.
                        MyHid.DeviceAttributes.Size = Marshal.SizeOf(MyHid.DeviceAttributes);
                    }

                    //  Learn the capabilitides of the device.
                    MyHid.Capabilities = MyHid.GetDeviceCapabilities(hidHandle);

                        //  Find out if the device is a system mouse or keyboard.
                        hidUsage = MyHid.GetHidUsage(MyHid.Capabilities);

                        //  Get the Input report buffer size.
                        //GetInputReportBufferSize();

                        //  Get handles to use in requesting Input and Output reports.
                        readHandle = FileIO.CreateFile(myDevicePathName, FileIO.GENERIC_READ, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, FileIO.FILE_FLAG_OVERLAPPED, 0);

                        //functionName = "CreateFile, ReadHandle";
                        //Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                        //Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                        //Debug.WriteLine("  Returned handle: " + readHandle.ToString());
                        //Console.Out.WriteLine("  Returned handle: " + readHandle.ToString());

                        if (readHandle.IsInvalid)
                        {
                            exclusiveAccess = true;
                        }
                        else
                        {
                            writeHandle = FileIO.CreateFile(myDevicePathName, FileIO.GENERIC_WRITE, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                            //functionName = "CreateFile, WriteHandle";
                            //Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                            //Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                            //Debug.WriteLine("  Returned handle: " + writeHandle.ToString());
                            //Console.Out.WriteLine("  Returned handle: " + writeHandle.ToString());

                            //  Flush any waiting reports in the input buffer. (optional)
                            MyHid.FlushQueue(readHandle);
                        }
                    }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.ToString());
                    throw new Exception(); //rethrow the exception -Lachezar
                }
            }



            public String[] FindTheHid()
            {
                Boolean deviceFound = false;
                String[] devicePathName = new String[128];
                String functionName = "";
                Guid hidGuid = Guid.Empty;
                Int32 memberIndex = 0;
                Int16 myProductID = 0;
                Int16 myVendorID = 0;
                //Int32 product = 0;
                //Int32 vendor = 0;

                Boolean success = false;

                try
                {
                    myDeviceDetected = false;

                    //  Get the device's Vendor ID and Product ID from the form's text boxes.

                    //GetVendorAndProductIDsFromTextBoxes(ref myVendorID, ref myProductID);

                    myVendorID = -20497;
                    myProductID = 3841;

                    //vendor = 0xAFEF;
                    //product = 0x0F01;

                    //  ***
                    //  API function: 'HidD_GetHidGuid

                    //  Purpose: Retrieves the interface class GUID for the HID class.

                    //  Accepts: 'A System.Guid object for storing the GUID.
                    //  ***

                    Hid.HidD_GetHidGuid(ref hidGuid);

                    functionName = "GetHidGuid";
                    Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                    //Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                    Debug.WriteLine("  GUID for system HIDs: " + hidGuid.ToString());
                    //Console.Out.WriteLine("  GUID for system HIDs: " + hidGuid.ToString());

                    //  Fill an array with the device path names of all attached HIDs.
                    deviceFound = MyDeviceManagement.FindDeviceFromGuid(hidGuid, ref devicePathName);

                    //  If there is at least one HID, attempt to read the Vendor ID and Product ID
                    //  of each device until there is a match or all devices have been examined.

                    if (deviceFound)
                    {
                        memberIndex = 0;

                        do
                        {
                            //  ***
                            //  API function:
                            //  CreateFile

                            //  Purpose:
                            //  Retrieves a handle to a device.

                            //  Accepts:
                            //  A device path name returned by SetupDiGetDeviceInterfaceDetail
                            //  The type of access requested (read/write).
                            //  FILE_SHARE attributes to allow other processes to access the device while this handle is open.
                            //  A Security structure or IntPtr.Zero. 
                            //  A creation disposition value. Use OPEN_EXISTING for devices.
                            //  Flags and attributes for files. Not used for devices.
                            //  Handle to a template file. Not used.

                            //  Returns: a handle without read or write access.
                            //  This enables obtaining information about all HIDs, even system
                            //  keyboards and mice. 
                            //  Separate handles are used for reading and writing.
                            //  ***

                            hidHandle = FileIO.CreateFile(devicePathName[memberIndex], 0, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                            functionName = "CreateFile";
                            Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                            //Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                            Debug.WriteLine("  Returned handle: " + hidHandle.ToString());
                            //Console.Out.WriteLine("  Returned handle: " + hidHandle.ToString());

                            if (!hidHandle.IsInvalid)
                            {
                                //  The returned handle is valid, 
                                //  so find out if this is the device we're looking for.

                                //  Set the Size property of DeviceAttributes to the number of bytes in the structure.

                                MyHid.DeviceAttributes.Size = Marshal.SizeOf(MyHid.DeviceAttributes);

                                //  ***
                                //  API function:
                                //  HidD_GetAttributes

                                //  Purpose:
                                //  Retrieves a HIDD_ATTRIBUTES structure containing the Vendor ID, 
                                //  Product ID, and Product Version Number for a device.

                                //  Accepts:
                                //  A handle returned by CreateFile.
                                //  A pointer to receive a HIDD_ATTRIBUTES structure.

                                //  Returns:
                                //  True on success, False on failure.
                                //  ***                            

                                success = Hid.HidD_GetAttributes(hidHandle, ref MyHid.DeviceAttributes);

                                if (success)
                                {
                                    Debug.WriteLine("  HIDD_ATTRIBUTES structure filled without error.");
                                    //Console.Out.WriteLine("  HIDD_ATTRIBUTES structure filled without error.");
                                    Debug.WriteLine("  Structure size: " + MyHid.DeviceAttributes.Size);
                                    //Console.Out.WriteLine("  Structure size: " + MyHid.DeviceAttributes.Size);
                                    Debug.WriteLine("  Vendor ID: " + Convert.ToString(MyHid.DeviceAttributes.VendorID, 16));
                                    //Console.Out.WriteLine("  Vendor ID: " + Convert.ToString(MyHid.DeviceAttributes.VendorID, 16));
                                    Debug.WriteLine("  Product ID: " + Convert.ToString(MyHid.DeviceAttributes.ProductID, 16));
                                    //Console.Out.WriteLine("  Product ID: " + Convert.ToString(MyHid.DeviceAttributes.ProductID, 16));
                                    Debug.WriteLine("  Version Number: " + Convert.ToString(MyHid.DeviceAttributes.VersionNumber, 16));
                                    //Console.Out.WriteLine("  Version Number: " + Convert.ToString(MyHid.DeviceAttributes.VersionNumber, 16));

                                    //  Find out if the device matches the one we're looking for.

                                    //if ((MyHid.DeviceAttributes.VendorID == vendor) & (MyHid.DeviceAttributes.ProductID == product))
                                    if ((MyHid.DeviceAttributes.VendorID == myVendorID) & (MyHid.DeviceAttributes.ProductID == myProductID))
                                    {

                                        Debug.WriteLine("  My device detected");
                                        //Console.Out.WriteLine("  My device detected");
                                        //  Display the information in form's list box.
                                        /*
                                        lstResults.Items.Add("Device detected:");
                                        lstResults.Items.Add("  Vendor ID= " + Convert.ToString(MyHid.DeviceAttributes.VendorID, 16));
                                        lstResults.Items.Add("  Product ID = " + Convert.ToString(MyHid.DeviceAttributes.ProductID, 16));
                                        ScrollToBottomOfListBox();
                                        */
                                        myDeviceDetected = true;

                                        //  Save the DevicePathName for OnDeviceChange().

                                        myDevicePathName = devicePathName[memberIndex];
                                    }
                                    else
                                    {
                                        //  It's not a match, so close the handle.

                                        myDeviceDetected = false;
                                        hidHandle.Close();
                                    }
                                }
                                else
                                {
                                    //  There was a problem in retrieving the information.

                                    Debug.WriteLine("  Error in filling HIDD_ATTRIBUTES structure.");
                                    //Console.Out.WriteLine("  Error in filling HIDD_ATTRIBUTES structure.");
                                    myDeviceDetected = false;
                                    hidHandle.Close();
                                }
                            }

                            //  Keep looking until we find the device or there are no devices left to examine.

                            memberIndex = memberIndex + 1;
                        }
                        while (!((myDeviceDetected | (memberIndex == devicePathName.Length))));
                    }

                    //if (myDeviceDetected)
                    //{
                    //    Console.Out.WriteLine("Device Detected");
                    //}


                    if (myDeviceDetected)
                    {
                        //  The device was detected.
                        //  Register to receive notifications if the device is removed or attached.

                        //success = MyDeviceManagement.RegisterForDeviceNotifications(myDevicePathName, FrmMy.Handle, hidGuid, ref deviceNotificationHandle);

                        //Debug.WriteLine("RegisterForDeviceNotifications = " + success);

                        //  Learn the capabilities of the device.

                        MyHid.Capabilities = MyHid.GetDeviceCapabilities(hidHandle);

                        if (success)
                        {
                            //  Find out if the device is a system mouse or keyboard.

                            hidUsage = MyHid.GetHidUsage(MyHid.Capabilities);

                            //  Get the Input report buffer size.

                            //GetInputReportBufferSize();
                            //cmdInputReportBufferSize.Enabled = true;

                            //  Get handles to use in requesting Input and Output reports.

                            readHandle = FileIO.CreateFile(myDevicePathName, FileIO.GENERIC_READ, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, FileIO.FILE_FLAG_OVERLAPPED, 0);

                            functionName = "CreateFile, ReadHandle";
                            Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                            //Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                            Debug.WriteLine("  Returned handle: " + readHandle.ToString());
                            //Console.Out.WriteLine("  Returned handle: " + readHandle.ToString());

                            if (readHandle.IsInvalid)
                            {
                                exclusiveAccess = true;
                                //lstResults.Items.Add("The device is a system " + hidUsage + ".");
                                //lstResults.Items.Add("Windows 2000 and Windows XP obtain exclusive access to Input and Output reports for this devices.");
                                //lstResults.Items.Add("Applications can access Feature reports only.");
                                //ScrollToBottomOfListBox();
                            }
                            else
                            {
                                writeHandle = FileIO.CreateFile(myDevicePathName, FileIO.GENERIC_WRITE, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                                functionName = "CreateFile, WriteHandle";
                                Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                                //Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                                Debug.WriteLine("  Returned handle: " + writeHandle.ToString());
                                //Console.Out.WriteLine("  Returned handle: " + writeHandle.ToString());

                                //  Flush any waiting reports in the input buffer. (optional)

                                MyHid.FlushQueue(readHandle);
                            }
                        }
                    }
                    else
                    {
                        //  The device wasn't detected.

                        //lstResults.Items.Add("Device not found.");
                        //cmdInputReportBufferSize.Enabled = false;
                        //cmdOnce.Enabled = true;

                        Debug.WriteLine(" Device not found.");
                        //Console.Out.WriteLine(" Device not found.");
                        //ScrollToBottomOfListBox();
                    }
                    //return myDeviceDetected;
                    return devicePathName; 
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.ToString());
                    throw new Exception();  //rethrow the exception - Lachezar
                }
            }


            public String[] GetGuids()
            {
                Boolean deviceFound = false;
                String[] devicePathName = new String[128];
                Queue<string> queueDevicePathName = new Queue<string>();
                String functionName = "";
                String devpath;
                Guid hidGuid = Guid.Empty;
                Int32 memberIndex = 0;
                Int16 myVendorID = -20497;
                Int16 myProductID = 3841;
                Boolean success = false;

                Hid.HidD_GetHidGuid(ref hidGuid);
                deviceFound = MyDeviceManagement.FindDeviceFromGuid(hidGuid, ref devicePathName);

                    for (int p = 0; p < devicePathName.Length; p++)
                    {
                        Console.Out.WriteLine(devicePathName[p]);
                    }

                    if (deviceFound)
                    {
                        for (int m = 0; m < devicePathName.Length; m++)
                        {
                            hidHandle = FileIO.CreateFile(devicePathName[m], 0, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);
                            functionName = "CreateFile";
                            Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));

                            if (!hidHandle.IsInvalid)
                            {
                                MyHid.DeviceAttributes.Size = Marshal.SizeOf(MyHid.DeviceAttributes);
                                success = Hid.HidD_GetAttributes(hidHandle, ref MyHid.DeviceAttributes);

                                if (success)
                                {
                                    if ((MyHid.DeviceAttributes.VendorID == myVendorID) & (MyHid.DeviceAttributes.ProductID == myProductID))
                                    {
                                        Debug.WriteLine("  My device detected");
                                        //Console.Out.WriteLine("  My device detected");
                                        myDeviceDetected = true;
                                        //  Save the DevicePathName for OnDeviceChange().
                                        //myDevicePathName = devicePathName[memberIndex];
                                        devpath = devicePathName[m];
                                        hidHandle.Close();
                                        queueDevicePathName.Enqueue(devpath);
                                    }
                                    else
                                    {
                                        //  It's not a match, so close the handle.
                                        myDeviceDetected = false;
                                        hidHandle.Close();
                                    }
                                }
                                else
                                {
                                    //  There was a problem in retrieving the information.
                                    Debug.WriteLine("  Error in filling HIDD_ATTRIBUTES structure.");
                                    //Console.Out.WriteLine("  Error in filling HIDD_ATTRIBUTES structure.");
                                    myDeviceDetected = false;
                                    hidHandle.Close();
                                }
                            }
                        }
                    }
                return queueDevicePathName.ToArray();
            }


            public String[] FindTheHids()
            {
                Boolean deviceFound = false;
                String[] devicePathName = new String[128];
                String functionName = "";
                Guid hidGuid = Guid.Empty;
                Int32 memberIndex = 0;
                Int16 myProductID = 0;
                Int16 myVendorID = 0;
                //Int32 product = 0;
                //Int32 vendor = 0;

                Boolean success = false;

                try
                {
                    myDeviceDetected = false;

                    //TODO: WTF is going on with this formatting?
                    //vendor = 0xAFEF;
                    //product = 0x0F01;
                    myVendorID = -20497;
                    myProductID = 3841;

                    //  ***
                    //  API function: 'HidD_GetHidGuid
                    //  Purpose: Retrieves the interface class GUID for the HID class.
                    //  Accepts: 'A System.Guid object for storing the GUID.
                    //  ***
                    Hid.HidD_GetHidGuid(ref hidGuid);

                    functionName = "GetHidGuid";
                    Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                    //Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                    Debug.WriteLine("  GUID for system HIDs: " + hidGuid.ToString());
                    //Console.Out.WriteLine("  GUID for system HIDs: " + hidGuid.ToString());

                    //  Fill an array with the device path names of all attached HIDs.
                    deviceFound = MyDeviceManagement.FindDeviceFromGuid(hidGuid, ref devicePathName);

                    for (int p = 0; p < devicePathName.Length; p++)
                    {
                        Console.Out.WriteLine(devicePathName[p]);
                    }

                    if (deviceFound)
                    {
                        for (int m = 0; m < devicePathName.Length; m++)
                        {
                            hidHandle = FileIO.CreateFile(devicePathName[memberIndex], 0, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);
                            functionName = "CreateFile";
                            Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));

                            if (!hidHandle.IsInvalid)
                            {
                                MyHid.DeviceAttributes.Size = Marshal.SizeOf(MyHid.DeviceAttributes);
                                success = Hid.HidD_GetAttributes(hidHandle, ref MyHid.DeviceAttributes);

                                if (success)
                                {
                                    if ((MyHid.DeviceAttributes.VendorID == myVendorID) & (MyHid.DeviceAttributes.ProductID == myProductID))
                                    {
                                        Debug.WriteLine("  My device detected");
                                        //Console.Out.WriteLine("  My device detected");
                                        myDeviceDetected = true;
                                        //  Save the DevicePathName for OnDeviceChange().
                                        myDevicePathName = devicePathName[memberIndex];
                                    }
                                    else
                                    {
                                        //  It's not a match, so close the handle.
                                        myDeviceDetected = false;
                                        hidHandle.Close();
                                    }
                                }
                                else
                                {
                                    //  There was a problem in retrieving the information.
                                    Debug.WriteLine("  Error in filling HIDD_ATTRIBUTES structure.");
                                    //Console.Out.WriteLine("  Error in filling HIDD_ATTRIBUTES structure.");
                                    myDeviceDetected = false;
                                    hidHandle.Close();
                                }
                            }
                        }
                        if (myDeviceDetected)
                        {
                            MyHid.Capabilities = MyHid.GetDeviceCapabilities(hidHandle);
                            if (success)
                            {
                                //  Find out if the device is a system mouse or keyboard.
                                hidUsage = MyHid.GetHidUsage(MyHid.Capabilities);

                                //  Get the Input report buffer size.
                                //GetInputReportBufferSize();
                                //cmdInputReportBufferSize.Enabled = true;

                                //  Get handles to use in requesting Input and Output reports.
                                readHandle = FileIO.CreateFile(myDevicePathName, FileIO.GENERIC_READ, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, FileIO.FILE_FLAG_OVERLAPPED, 0);

                                functionName = "CreateFile, ReadHandle";
                                Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                                //Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                                Debug.WriteLine("  Returned handle: " + readHandle.ToString());
                                //Console.Out.WriteLine("  Returned handle: " + readHandle.ToString());
                                if (readHandle.IsInvalid)
                                {
                                    exclusiveAccess = true;
                                    //lstResults.Items.Add("The device is a system " + hidUsage + ".");
                                    //lstResults.Items.Add("Windows 2000 and Windows XP obtain exclusive access to Input and Output reports for this devices.");
                                    //lstResults.Items.Add("Applications can access Feature reports only.");
                                    //ScrollToBottomOfListBox();
                                }
                                else
                                {
                                    writeHandle = FileIO.CreateFile(myDevicePathName, FileIO.GENERIC_WRITE, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                                    functionName = "CreateFile, WriteHandle";
                                    Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                                    //Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                                    Debug.WriteLine("  Returned handle: " + writeHandle.ToString());
                                    //Console.Out.WriteLine("  Returned handle: " + writeHandle.ToString());

                                    //  Flush any waiting reports in the input buffer. (optional)

                                    MyHid.FlushQueue(readHandle);
                                }
                            }
                            else
                            {
                                //  The device wasn't detected.

                                //lstResults.Items.Add("Device not found.");
                                //cmdInputReportBufferSize.Enabled = false;
                                //cmdOnce.Enabled = true;

                                Debug.WriteLine(" Device not found.");
                                //Console.Out.WriteLine(" Device not found.");
                                //ScrollToBottomOfListBox();
                            }
                            //return myDeviceDetected;
                        }
                    }
                    return devicePathName;
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.ToString());
                    throw new Exception(); //rethrow the exception - Lachezar
                }
            }



                    //  If there is at least one HID, attempt to read the Vendor ID and Product ID
                    //  of each device until there is a match or all devices have been examined.
                    /*
                    if (deviceFound)
                    {
                        memberIndex = 0;
                        do
                        {
                            //  ***
                            //  API function:
                            //  CreateFile

                            //  Purpose:
                            //  Retrieves a handle to a device.

                            //  Accepts:
                            //  A device path name returned by SetupDiGetDeviceInterfaceDetail
                            //  The type of access requested (read/write).
                            //  FILE_SHARE attributes to allow other processes to access the device while this handle is open.
                            //  A Security structure or IntPtr.Zero. 
                            //  A creation disposition value. Use OPEN_EXISTING for devices.
                            //  Flags and attributes for files. Not used for devices.
                            //  Handle to a template file. Not used.

                            //  Returns: a handle without read or write access.
                            //  This enables obtaining information about all HIDs, even system
                            //  keyboards and mice. 
                            //  Separate handles are used for reading and writing.
                            //  ***
                            hidHandle = FileIO.CreateFile(devicePathName[memberIndex], 0, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                            functionName = "CreateFile";
                            Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                            Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                            Debug.WriteLine("  Returned handle: " + hidHandle.ToString());
                            Console.Out.WriteLine("  Returned handle: " + hidHandle.ToString());

                            if (!hidHandle.IsInvalid)
                            {
                                //  The returned handle is valid, 
                                //  so find out if this is the device we're looking for.

                                //  Set the Size property of DeviceAttributes to the number of bytes in the structure.
                                MyHid.DeviceAttributes.Size = Marshal.SizeOf(MyHid.DeviceAttributes);

                                //  ***
                                //  API function:
                                //  HidD_GetAttributes

                                //  Purpose:
                                //  Retrieves a HIDD_ATTRIBUTES structure containing the Vendor ID, 
                                //  Product ID, and Product Version Number for a device.

                                //  Accepts:
                                //  A handle returned by CreateFile.
                                //  A pointer to receive a HIDD_ATTRIBUTES structure.

                                //  Returns:
                                //  True on success, False on failure.
                                //  ***                            

                                success = Hid.HidD_GetAttributes(hidHandle, ref MyHid.DeviceAttributes);

                                if (success)
                                {
                                    Debug.WriteLine("  HIDD_ATTRIBUTES structure filled without error.");
                                    Console.Out.WriteLine("  HIDD_ATTRIBUTES structure filled without error.");
                                    Debug.WriteLine("  Structure size: " + MyHid.DeviceAttributes.Size);
                                    Console.Out.WriteLine("  Structure size: " + MyHid.DeviceAttributes.Size);
                                    Debug.WriteLine("  Vendor ID: " + Convert.ToString(MyHid.DeviceAttributes.VendorID, 16));
                                    Console.Out.WriteLine("  Vendor ID: " + Convert.ToString(MyHid.DeviceAttributes.VendorID, 16));
                                    Debug.WriteLine("  Product ID: " + Convert.ToString(MyHid.DeviceAttributes.ProductID, 16));
                                    Console.Out.WriteLine("  Product ID: " + Convert.ToString(MyHid.DeviceAttributes.ProductID, 16));
                                    Debug.WriteLine("  Version Number: " + Convert.ToString(MyHid.DeviceAttributes.VersionNumber, 16));
                                    Console.Out.WriteLine("  Version Number: " + Convert.ToString(MyHid.DeviceAttributes.VersionNumber, 16));

                                    //  Find out if the device matches the one we're looking for.

                                    //if ((MyHid.DeviceAttributes.VendorID == vendor) & (MyHid.DeviceAttributes.ProductID == product))
                                    if ((MyHid.DeviceAttributes.VendorID == myVendorID) & (MyHid.DeviceAttributes.ProductID == myProductID))
                                    {
                                        Debug.WriteLine("  My device detected");
                                        Console.Out.WriteLine("  My device detected");
                                        myDeviceDetected = true;
                                        //  Save the DevicePathName for OnDeviceChange().
                                        myDevicePathName = devicePathName[memberIndex];
                                    }
                                    else
                                    {
                                        //  It's not a match, so close the handle.
                                        myDeviceDetected = false;
                                        hidHandle.Close();
                                    }
                                }
                                else
                                {
                                    //  There was a problem in retrieving the information.
                                    Debug.WriteLine("  Error in filling HIDD_ATTRIBUTES structure.");
                                    Console.Out.WriteLine("  Error in filling HIDD_ATTRIBUTES structure.");
                                    myDeviceDetected = false;
                                    hidHandle.Close();
                                }
                            }
                            //  Keep looking until we find the device or there are no devices left to examine.
                            memberIndex = memberIndex + 1;
                        }
                        while (!((myDeviceDetected | (memberIndex == devicePathName.Length))));
                    }
                    



                    //if (myDeviceDetected)
                    //{
                    //    Console.Out.WriteLine("Device Detected");
                    //}
                    if (myDeviceDetected)
                    {
                        //  The device was detected.
                        //  Register to receive notifications if the device is removed or attached.

                        //success = MyDeviceManagement.RegisterForDeviceNotifications(myDevicePathName, FrmMy.Handle, hidGuid, ref deviceNotificationHandle);

                        //Debug.WriteLine("RegisterForDeviceNotifications = " + success);

                        //  Learn the capabilities of the device.

                        MyHid.Capabilities = MyHid.GetDeviceCapabilities(hidHandle);

                        if (success)
                        {
                            //  Find out if the device is a system mouse or keyboard.

                            hidUsage = MyHid.GetHidUsage(MyHid.Capabilities);

                            //  Get the Input report buffer size.

                            //GetInputReportBufferSize();
                            //cmdInputReportBufferSize.Enabled = true;

                            //  Get handles to use in requesting Input and Output reports.

                            readHandle = FileIO.CreateFile(myDevicePathName, FileIO.GENERIC_READ, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, FileIO.FILE_FLAG_OVERLAPPED, 0);

                            functionName = "CreateFile, ReadHandle";
                            Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                            Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                            Debug.WriteLine("  Returned handle: " + readHandle.ToString());
                            Console.Out.WriteLine("  Returned handle: " + readHandle.ToString());

                            if (readHandle.IsInvalid)
                            {
                                exclusiveAccess = true;
                                //lstResults.Items.Add("The device is a system " + hidUsage + ".");
                                //lstResults.Items.Add("Windows 2000 and Windows XP obtain exclusive access to Input and Output reports for this devices.");
                                //lstResults.Items.Add("Applications can access Feature reports only.");
                                //ScrollToBottomOfListBox();
                            }
                            else
                            {
                                writeHandle = FileIO.CreateFile(myDevicePathName, FileIO.GENERIC_WRITE, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                                functionName = "CreateFile, WriteHandle";
                                Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                                Console.Out.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                                Debug.WriteLine("  Returned handle: " + writeHandle.ToString());
                                Console.Out.WriteLine("  Returned handle: " + writeHandle.ToString());

                                //  Flush any waiting reports in the input buffer. (optional)

                                MyHid.FlushQueue(readHandle);
                            }
                        }
                    }
                    else
                    {
                        //  The device wasn't detected.

                        //lstResults.Items.Add("Device not found.");
                        //cmdInputReportBufferSize.Enabled = false;
                        //cmdOnce.Enabled = true;

                        Debug.WriteLine(" Device not found.");
                        Console.Out.WriteLine(" Device not found.");
                        //ScrollToBottomOfListBox();
                    }
                    //return myDeviceDetected;
                    return devicePathName;
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.ToString());
                    throw;
                }
            }
                        */



            public void write()
            {
                /*
                try
                {
                    //  Don't allow another transfer request until this one completes.
                    ReadAndWriteToDevice();
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex);
                    throw;
                }
                 */
            }

            public void RW(Queue<byte> writeBuffer, int count)
            {
                String byteValue = null;
                Int32 counter = 0;
                Byte[] inputReportBuffer = null;
                Byte[] outputReportBuffer = null;
                Boolean success = false;

                Thread.Sleep(10);

                try
                {
                int c = writeBuffer.Count;
                byte[] buffer = new byte[65];;


                if (writeBuffer.Count > 0)
                {
                    buffer[0] = 0;
                    buffer[1] = (byte)writeBuffer.Count;
                    writeBuffer.ToArray().CopyTo(buffer, 2);
                    writeBuffer.Clear();
                }

                //else if(writeBuffer.Count == 0)
                //{
                //    return;
                //}

                    success = false;

                    //this.FindTheHid();
                    //if ((myDeviceDetected == false))
                    //{
                    //    FindTheHid();
                    //    myDeviceDetected = true;
                    //}

                    //  Don't attempt to exchange reports if valid handles aren't available (as for a mouse or keyboard under Windows 2000/XP.)
                    if (!readHandle.IsInvalid & !writeHandle.IsInvalid)
                    {
                        //  Don't attempt to send an Output report if the HID has no Output report.
                        if (MyHid.Capabilities.OutputReportByteLength > 0)
                        {
                            //  Set the size of the Output report buffer.   
                            outputReportBuffer = new Byte[MyHid.Capabilities.OutputReportByteLength];

                            //  Store the report ID in the first byte of the buffer:
                            outputReportBuffer[0] = 0;

                            //  Store the report data following the report ID.
                            buffer.CopyTo(outputReportBuffer, 0);

                            //outputReportBuffer[1] = 0x0D;
                            /*
                            for (int i = 0; i < c; i++)
                            {
                                outputReportBuffer[i] = buffer[0];
                                writeBuffer.ToArray().CopyTo(buffer, 2);
                            }
                             */

                            //outputReportBuffer[1] = Convert.ToByte(cboByte0.SelectedIndex);

                            //if (Information.UBound(outputReportBuffer, 1) > 1)
                            //{
                            //    outputReportBuffer[2] = Convert.ToByte(cboByte1.SelectedIndex);
                            //}

                            //WriteFile uses an interrupt transfer to send the report. 
                            Hid.OutputReportViaInterruptTransfer myOutputReport = new Hid.OutputReportViaInterruptTransfer();
                            success = myOutputReport.Write(outputReportBuffer, writeHandle);

                            if (success)
                            {
                                //Console.Out.WriteLine("\nAn Output report has been written.");
                                Debug.WriteLine("\nAn Output report has been written.");
                                //Console.Out.WriteLine("Output Report ID: " + String.Format("{0:X2} ", outputReportBuffer[0]));
                                Debug.WriteLine("Output Report ID: " + String.Format("{0:X2} ", outputReportBuffer[0]));
                                //Console.Out.WriteLine("Output Report Data:");
                                Debug.WriteLine("Output Report Data:");
                                for (counter = 0; counter <= outputReportBuffer.Length - 1; counter++)
                                {
                                    //  Display bytes as 2-character hex strings.
                                    byteValue = String.Format("{0:X2} ", outputReportBuffer[counter]);
                                    //Console.Out.Write(" " + byteValue);
                                    Debug.Write(" " + byteValue);
                                }
                            }
                            else
                            {
                                //Console.Out.WriteLine("The attempt to write an Output report has failed.");
                                Debug.WriteLine("The attempt to write an Output report has failed.");
                            }
                        }
                        else
                        {
                            //Console.Out.WriteLine("The HID doesn't have an Output report.");
                            Debug.WriteLine("The HID doesn't have an Output report.");
                        }



                        /*
                        //  Read an Input report.
                        success = false;
                        //  Don't attempt to send an Input report if the HID has no Input report.
                        //  (The HID spec requires all HIDs to have an interrupt IN endpoint,
                        //  which suggests that all HIDs must support Input reports.)
                        if (MyHid.Capabilities.InputReportByteLength > 0)
                        {
                            //  Set the size of the Input report buffer. 
                            inputReportBuffer = new Byte[MyHid.Capabilities.InputReportByteLength];

                            //  Read a report using interrupt transfers.                
                            //  To enable reading a report without blocking the main thread, this
                            //  application uses an asynchronous delegate.

                            IAsyncResult ar = null;
                            Hid.InputReportViaInterruptTransfer myInputReport = new Hid.InputReportViaInterruptTransfer();

                            //  Define a delegate for the Read method of myInputReport.
                            ReadInputReportDelegate MyReadInputReportDelegate = new ReadInputReportDelegate(myInputReport.Read);

                            //  The BeginInvoke method calls myInputReport.Read to attempt to read a report.
                            //  The method has the same parameters as the Read function,
                            //  plus two additional parameters:
                            //  GetInputReportData is the callback procedure that executes when the Read function returns.
                            //  MyReadInputReportDelegate is the asynchronous delegate object.
                            //  The last parameter can optionally be an object passed to the callback.
                            ar = MyReadInputReportDelegate.BeginInvoke(hidHandle, readHandle, writeHandle, ref myDeviceDetected, ref inputReportBuffer, ref success, new AsyncCallback(GetInputReportData), MyReadInputReportDelegate);

                        }
                        else
                        {
                            Console.Out.WriteLine("No attempt to read an Input report was made.");
                            Console.Out.WriteLine("The HID doesn't have an Input report.");
                        }

                        while (this.readBuffer.Count < 63)
                        {
                            Thread.Sleep(5);
                        }
                        this.ReadFinished = true;

                        //this.readBuffer.Clear();
                        //this.ReadFinished = false;
                        this.ReadStart = 0;
                    */


                    }
                    else
                    {
                        //Console.Out.WriteLine("Invalid handle. The device is probably a system mouse or keyboard.");
                        Debug.WriteLine("Invalid handle. The device is probably a system mouse or keyboard.");
                        //Console.Out.WriteLine("No attempt to write an Output report or read an Input report was made.");
                        Debug.WriteLine("No attempt to write an Output report or read an Input report was made.");
                    }
                }

                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.ToString());
                    throw new Exception(); //rethrow the exception - Lachezar
                }
                Thread.Sleep(10);
            }

            public bool FillRxBuffer()
            {
                Byte[] inputReportBuffer = null;
                Boolean success = false;

                readBuffer.Clear();
                this.readBuffer.Clear();
                this.ReadFinished = false;
                this.ReadStart = 0;

                //Thread.Sleep(10);
                /*
                if (readHandle.IsClosed)
                {
                    Console.Out.WriteLine("readHandle closed");
                }
                */

                if (!readHandle.IsClosed)
                {
                    if (!readHandle.IsInvalid && MyHid.Capabilities.InputReportByteLength > 0)
                    //if (MyHid.Capabilities.InputReportByteLength > 0)
                    {
                        //  Set the size of the Input report buffer. 
                        inputReportBuffer = new Byte[MyHid.Capabilities.InputReportByteLength];

                        //  Read a report using interrupt transfers.                
                        //  To enable reading a report without blocking the main thread, this
                        //  application uses an asynchronous delegate.
                        IAsyncResult ar = null;
                        Hid.InputReportViaInterruptTransfer myInputReport = new Hid.InputReportViaInterruptTransfer();

                        //  Define a delegate for the Read method of myInputReport.
                        ReadInputReportDelegate MyReadInputReportDelegate = new ReadInputReportDelegate(myInputReport.Read);

                        /*
                        if (readHandle.IsClosed)
                        {
                            Debug.WriteLine("readHandle Closed");
                        }
                         */

                        //  The BeginInvoke method calls myInputReport.Read to attempt to read a report.
                        //  The method has the same parameters as the Read function,
                        //  plus two additional parameters:
                        //  GetInputReportData is the callback procedure that executes when the Read function returns.
                        //  MyReadInputReportDelegate is the asynchronous delegate object.
                        //  The last parameter can optionally be an object passed to the callback.
                        ar = MyReadInputReportDelegate.BeginInvoke(hidHandle, readHandle, writeHandle, ref myDeviceDetected, ref inputReportBuffer, ref success, new AsyncCallback(GetInputReportData), MyReadInputReportDelegate);

                    }
                    else
                    {
                        //Console.Out.WriteLine("No attempt to read an Input report was made.");
                        Debug.WriteLine("No attempt to read an Input report was made.");
                        //Console.Out.WriteLine("The HID doesn't have an Input report.");
                        Debug.WriteLine("The HID doesn't have an Input report.");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine("readHandle Closed");
                    return false;
                }

                //while (this.readBuffer.Count < 63)
                //{
                //    Thread.Sleep(5);
                //}
                //ReadFinished = true;
                return true;
            }

            private void GetInputReportData(IAsyncResult ar)
            {
                String byteValue = null;
                Int32 count = 0;
                Byte[] inputReportBuffer = null;
                Boolean success = false;

                //this.readBuffer.Clear();
                //this.ReadFinished = false;
                //this.ReadStart = 0;

                if (readHandle.IsClosed)
                {
                    Console.Out.WriteLine("readHandle closed");
                }

                if (!readHandle.IsClosed)
                {
                    try
                    {
                        // Define a delegate using the IAsyncResult object.
                        ReadInputReportDelegate deleg = ((ReadInputReportDelegate)(ar.AsyncState));

                        //  Get the IAsyncResult object and the values of other paramaters that the
                        //  BeginInvoke method passed ByRef.
                        deleg.EndInvoke(ref myDeviceDetected, ref inputReportBuffer, ref success, ar);

                        if ((ar.IsCompleted & success))
                        {
                            //Console.Out.WriteLine("\nAn Input report has been read.");
                            Debug.WriteLine("\nAn Input report has been read.");
                            //Console.Out.WriteLine("\nInput Report ID: " + String.Format("{0:X2} ", inputReportBuffer[0]));
                            Debug.WriteLine("\nInput Report ID: " + String.Format("{0:X2} ", inputReportBuffer[0]));
                            //Console.Out.WriteLine("\nInput Report Data:");
                            Debug.WriteLine("\nInput Report Data:");

                            for (count = 0; count <= inputReportBuffer.Length - 1; count++)
                            {
                                //send bytes to the callback as they come in
                                byteValue = String.Format("{0:X2} ", inputReportBuffer[count]);
                                MyMarshalToForm("", byteValue);
                            }
                        }
                        else
                        {
                            //Console.Out.WriteLine("The attempt to read an Input report has failed.");
                            Debug.WriteLine("The attempt to read an Input report has failed.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Out.WriteLine(ex.ToString());
                        //DisplayException(this.Name, ex);
                        //throw;
                        //if (ex == System.ObjectDisposedException)
                        //{
                        //    Console.Out.WriteLine(ex.ToString());
                        //}
                    }
                }
                else
                {
                    Console.Out.WriteLine("readHandle closed");
                }

            }



            ///  <summary>
            ///  Enables accessing a form's controls from another thread 
            ///  </summary>
            ///  
            ///  <param name="action"> a String that names the action to perform on the form </param>
            ///  <param name="textToDisplay"> text that the form displays or the code uses for 
            ///  another purpose. Actions that don't use text ignore this parameter.  </param>
            private void MyMarshalToForm(String action, String str)
            {
                //TODO: check for encoding.GetBytes(str) > 1  ?
                int discarded;
                byte[] byteArray = HexEncoding.GetBytes(str, out discarded);
                byte b = byteArray[0];

                if (ReadStart < 2)
                {
                    ReadStart++;
                }
                else
                {
                    this.readBuffer.Enqueue(b);
                    //Console.Out.Write(str + " ");
                    Debug.Write(str + " ");
                    if (this.readBuffer.Count > 64)
                    {
                        //Console.Out.WriteLine("readbuffer count:" + this.readBuffer.Count);
                        Debug.WriteLine("readbuffer count:" + this.readBuffer.Count);
                    }
                }
                //object[] args = { action, textToDisplay };
                //MarshalToForm MarshalToFormDelegate = null;
                //  The AccessForm routine contains the code that accesses the form.
                //MarshalToFormDelegate = new MarshalToForm(AccessForm);
                //  Execute AccessForm, passing the parameters in args.
                //base.Invoke(MarshalToFormDelegate, args);
            }

            /*
            public static byte[] ToByteArray(String HexString)
            {
                int NumberChars = HexString.Length;
                byte[] bytes = new byte[NumberChars]; 
                //byte[] bytes = new byte[NumberChars / 2];

                for (int i = 0; i < NumberChars; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(HexString.Substring(i, 2), 16);
                }
                return bytes;
            }
            */

            public Queue<byte> GetReadBuffer()
            {
                int retry = 0;
                if (ReadFinished == false)
                {
                    while (this.readBuffer.Count < 63)
                    {
                        Thread.Sleep(50);
                        retry++;
                        if (retry > 20)
                        {
                            Debug.WriteLine("readbuffer count:" + this.readBuffer.Count);
                            Debug.WriteLine("READ TIMEOUT");
                            readBuffer.Clear();
                            return readBuffer;
                            //break;
                        }
                    }
                    //Thread.Sleep(10);
                    ReadFinished = true;
                }
                return readBuffer;

                //temp = new byte[65];
                //readBuffer = readBuffer.CopyTo(temp, 2);
                //temp.

                //Queue<byte> buf = new Queue<byte>(65);
                //readBuffer.CopyTo(buf, 0);
                //return buf; 
            }
            public void ClearReadBuffer()
            {
                readBuffer.Clear();
            }
        }

        public class HexEncoding
        {
            public HexEncoding()
            {
                //
                // TODO: Add constructor logic here
                //
            }
            public static int GetByteCount(string hexString)
            {
                int numHexChars = 0;
                char c;
                // remove all none A-F, 0-9, characters
                for (int i = 0; i < hexString.Length; i++)
                {
                    c = hexString[i];
                    if (IsHexDigit(c))
                        numHexChars++;
                }
                // if odd number of characters, discard last character
                if (numHexChars % 2 != 0)
                {
                    numHexChars--;
                }
                return numHexChars / 2; // 2 characters per byte
            }
            /// <summary>
            /// Creates a byte array from the hexadecimal string. Each two characters are combined
            /// to create one byte. First two hexadecimal characters become first byte in returned array.
            /// Non-hexadecimal characters are ignored. 
            /// </summary>
            /// <param name="hexString">string to convert to byte array</param>
            /// <param name="discarded">number of characters in string ignored</param>
            /// <returns>byte array, in the same left-to-right order as the hexString</returns>
            public static byte[] GetBytes(string hexString, out int discarded)
            {
                discarded = 0;
                string newString = "";
                char c;
                // remove all none A-F, 0-9, characters
                for (int i = 0; i < hexString.Length; i++)
                {
                    c = hexString[i];
                    if (IsHexDigit(c))
                        newString += c;
                    else
                        discarded++;
                }
                // if odd number of characters, discard last character
                if (newString.Length % 2 != 0)
                {
                    discarded++;
                    newString = newString.Substring(0, newString.Length - 1);
                }

                int byteLength = newString.Length / 2;
                byte[] bytes = new byte[byteLength];
                string hex;
                int j = 0;
                for (int i = 0; i < bytes.Length; i++)
                {
                    hex = new String(new Char[] { newString[j], newString[j + 1] });
                    bytes[i] = HexToByte(hex);
                    j = j + 2;
                }
                return bytes;
            }
            public static string ToString(byte[] bytes)
            {
                string hexString = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    hexString += bytes[i].ToString("X2");
                }
                return hexString;
            }
            /// <summary>
            /// Determines if given string is in proper hexadecimal string format
            /// </summary>
            /// <param name="hexString"></param>
            /// <returns></returns>
            public static bool InHexFormat(string hexString)
            {
                bool hexFormat = true;

                foreach (char digit in hexString)
                {
                    if (!IsHexDigit(digit))
                    {
                        hexFormat = false;
                        break;
                    }
                }
                return hexFormat;
            }

            /// <summary>
            /// Returns true is c is a hexadecimal digit (A-F, a-f, 0-9)
            /// </summary>
            /// <param name="c">Character to test</param>
            /// <returns>true if hex digit, false if not</returns>
            public static bool IsHexDigit(Char c)
            {
                int numChar;
                int numA = Convert.ToInt32('A');
                int num1 = Convert.ToInt32('0');
                c = Char.ToUpper(c);
                numChar = Convert.ToInt32(c);
                if (numChar >= numA && numChar < (numA + 6))
                    return true;
                if (numChar >= num1 && numChar < (num1 + 10))
                    return true;
                return false;
            }
            /// <summary>
            /// Converts 1 or 2 character string into equivalant byte value
            /// </summary>
            /// <param name="hex">1 or 2 character string</param>
            /// <returns>byte</returns>
            private static byte HexToByte(string hex)
            {
                if (hex.Length > 2 || hex.Length <= 0)
                    throw new ArgumentException("hex must be 1 or 2 characters in length");
                byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                return newByte;
            }


        }
    }
}
