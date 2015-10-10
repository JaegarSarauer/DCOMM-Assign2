using System;
using SkyeTek.Devices;
using SkyeTek.Tags;
using SkyeTek.STPv3;
using System.Text;

using System.ComponentModel;
using System.Collections;
using System.IO;

enum SYS_PARAMS
{
    SYS_SERIAL_NUMBER               = 0x0000,
    SYS_FIRMWARE_VER                = 0x0001,
    SYS_HARDWARE_VER                = 0x0002,
    SYS_PRODUCT_CODE                = 0x0003,
    SYS_RID                         = 0x0004,
    SYS_READER_NAME                 = 0x0005,
    SYS_HOST_INTERFACE              = 0x0006,
    SYS_BAUD_RATE                   = 0x0007,
    SYS_USER_PORT_DIR               = 0x0008,
    SYS_USER_PORT_VAL               = 0x0009,
    SYS_MUX_CONTROL                 = 0x000A,
    SYS_OPERATING_MODE              = 0x000C,
    SYS_ENCRYPTION_SCHEME           = 0x000D,
    SYS_HMAC_SCHEME                 = 0x000E,
    SYS_TAG_POPULATION              = 0x0010,
    SYS_RETRY_COUNT                 = 0x0011,
    SYS_TX_POWER                    = 0x0012,

    SYS_CURRENT_FREQUENCY           = 0x0030,
    SYS_START_FREQUENCY             = 0x0031,
    SYS_STOP_FREQUENCY              = 0x0032,
    SYS_FEATURE_LOCK                = 0x0033,
    SYS_HOP_CHANNEL_SPACING         = 0x0034,
    SYS_FREQEUNCY_HOP_SEQUENCE      = 0x0035,
    SYS_MODULATION_DEPTH            = 0x0036,
    SYS_REGULATORY_MODE             = 0x0037,
    SYS_LBT_ADJUST                  = 0x0038,
    SYS_BOARD_TEMPERATURE           = 0x0039,
    SYS_ETSI_SIGNAL_STRENGTH        = 0x003A,
    SYS_SYNTHESIZER_POWER_LEVEL     = 0x003C,
    SYS_CURRENT_DAC_VALUE           = 0x003F,
    SYS_POWER_DETECTOR_VALUE        = 0x0040,
    SYS_PULSE_SHAPING_MODE          = 0x0041,
    SYS_PA_TABLE                    = 0x0042,
    SYS_REGULATOR_SWITCH            = 0x0043,

    SYS_SITE_SURVEY                 = 0x0044,
    SYS_OPTIMAL_POWER_GEN1          = 0x0045,
    SYS_OPTIMAL_POWER_GEN2          = 0x0046,
    SYS_OPTIMAL_POWER_6B            = 0x0047,
    SYS_TEST_MODE                   = 0x0048,
    SYS_OPTIMAL_POWER_EM            = 0x0049
}

namespace SkyeTek
{
    namespace Readers
    {
        public class ReaderException : Exception
        {
            public ReaderException(string message) : base(message) { }
        }
        /// <summary>
        /// Inventory tag callback
        /// </summary>
        /// <param name="tag">A tag found by the inventory process</param>
        /// <param name="context">Object passed in for caller context</param>
        /// <returns>True to end the inventory process if InventoryTags was called with loop set to true</returns>
        public delegate bool InventoryTagDelegate(Tag tag, object context);

        public abstract class Reader
        {
            protected Device m_device;
            
            #region Constructors
            public Reader() : this(null) { }

            public Reader(Device device)
            {
                if (device == null)
                    throw new ArgumentException("Device cannot be null");

                this.m_device = device;
            }
            #endregion

            #region Properties
            public Device Device
            {
                get { return this.m_device; }
            }

            public virtual string FirmwareVersion
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public virtual string SerialNumber
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public virtual string ProductCode
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public virtual string ReaderName
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }
            #endregion

            public virtual void Close()
            {
                this.m_device.Close();
            }

            public virtual void Open()
            {
                this.m_device.Open();
            }

            /// <summary>
            /// Selects a specified tag in the field.  Returns true if tag is selected.
            /// </summary>
            /// <param name="tag">Reference to a tag.  If tag type is not specified, auto-detect will be used.</param>
            /// <returns>True if a tag was selected, false otherwise</returns>
            public abstract bool SelectTag(ref Tag tag);
            
            /// <summary>
            /// Inventory tags in the readers field. 
            /// </summary>
            /// <param name="tag">This parameter describes the tags to inventory (e.g. by type, by epc, by afi, etc...).</param>
            /// <param name="loop">True indicates the reader should inventory tags until explicitly told to stop</param>
            /// <param name="itd">Delegate to call when a tag is inventoried.  Any long running or computationally intensive work should
            /// be done outside of the this function call so as not to impede the inventory process.</param>
            /// <param name="context">Context object passed through to delegate call</param>
            /// <returns>True if the call to InventoryTags was terminated by a call to the InventoryTagDelegate, false if terminated because of another reason</returns>
            public abstract bool InventoryTags(Tag tag, bool loop, InventoryTagDelegate itd, object context);

            public Tag[] SelectTags(Tag tag)
            {
                ArrayList its = new ArrayList();

                InventoryTagDelegate itd = delegate(Tag it, object ctx)
                {
                    its.Add(it);
                    return true;
                };

                this.InventoryTags(tag, false, itd, null);

                return (Tag[])its.ToArray(typeof(Tag));
            }

            /// <summary>
            /// Reads data from the specified tag
            /// </summary>
            /// <param name="tag">Tag to read data from</param>
            /// <param name="address">Start address of read</param>
            /// <param name="blocks">Number of blocks</param>
            /// <returns>Returns null if the read fails, data otherwise</returns>
            public abstract byte[] ReadTagData(Tag tag, ushort address, ushort blocks);

            /// <summary>
            /// Writes data to the specified tag
            /// </summary>
            /// <param name="tag">Tag to write data to</param>
            /// <param name="data">Data to be written</param>
            /// <param name="address">Start address of write</param>
            /// <param name="blocks">Number of blocks</param>
            /// <returns>True if the operation succeeded, false otherwise</returns>
            public abstract bool WriteTagData(Tag tag, byte[] data, ushort address, ushort blocks);
        }

        public class STPv3Reader : Reader
        {

            private byte[] m_RID = { 0xFF, 0xFF, 0xFF, 0xFF };
            private string m_FirmwareVersion;

            public STPv3Reader(Device device) : base(device)
            {
                this.m_FirmwareVersion = null;
            }

         
            /// <summary>
            /// Reader Serial Number
            /// </summary>
            public override string SerialNumber
            {
                get
                {
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_SERIAL_NUMBER;
                    request.Blocks = 4;
                    request.RID = this.m_RID;
                   
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return null;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return null;
                    
                    return BitConverter.ToString(response.Data).Replace("-", "");
                    
                }

            }

            /// <summary>
            /// Reader Firmware Version
            /// </summary>
            public override string FirmwareVersion
            {
                get
                {
                    if (this.m_FirmwareVersion != null)
                        return this.m_FirmwareVersion;

                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_FIRMWARE_VER;
                    request.Blocks = 4;
                    request.RID = this.m_RID;
          
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return null;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return null;

                    this.m_FirmwareVersion = BitConverter.ToString(response.Data).Replace("-", "");
                    
                    return this.m_FirmwareVersion;
                }
            }
            
            /// <summary>
            /// Reader Product Code
            /// </summary>
            public override string ProductCode
            {
                get
                {
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_PRODUCT_CODE;
                    request.Blocks = 2;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return null;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return null;

                    return BitConverter.ToString(response.Data).Replace("-", "");

                }
            }
            
            /// <summary>
            /// Reader hardware version
            /// </summary>
            public string HardwareVersion
            {
                get
                {
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_HARDWARE_VER;
                    request.Blocks = 4;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return null;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return null;


                    return BitConverter.ToString(response.Data).Replace("-", "");
                    
                }
            }

            /// <summary>
            /// Reader Name
            /// </summary>
            public override string ReaderName
            {
                get
                {
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_READER_NAME;
                    request.Blocks = 32;
                    request.RID = this.m_RID;
                
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();


                    if (response == null)
                        return null;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return null;

                    
                    return Encoding.ASCII.GetString(response.Data).Trim(new char[] { '\0' }); 
                }
                set
                {
                    STPv3Request request = new STPv3Request();
                    byte[] readername = new byte[32];
                    Encoding.ASCII.GetBytes(value).CopyTo(readername, 0);
                    

                    request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_READER_NAME;
                    request.Blocks = 32;
                    request.Data = readername;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                         throw new ReaderException(String.Format("Unable to set Reader ID, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Reader ID
            /// </summary>
            public byte[] ReaderID
            {
                get
                {
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_RID;
                    request.Blocks = 4;
                    request.RID = this.m_RID;
                    
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return null;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return null;

                    return response.Data;
                    
                }
                set
                {
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_RID;
                    request.Blocks = 4;
                    request.Data = value;
                    request.RID = this.m_RID;
                
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Reader ID, error 0x#{0:X}", response.ResponseCode));

                    request.Data.CopyTo(this.m_RID, 0);
                }
            }

            /// <summary>
            /// Current Frequency
            /// </summary>
            public uint CurrentFrequency
            {
                get
                {
                    int freq = 0;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_CURRENT_FREQUENCY;
                    request.Blocks = 4;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    freq |= response.Data[0] << 24;
                    freq |= response.Data[1] << 16;
                    freq |= response.Data[2] << 8;
                    freq |= response.Data[3];

                    return (uint)freq;

                }
                set
                {
                    byte[] data = new byte[4];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_CURRENT_FREQUENCY;
                    request.Blocks = 4;
                    data[0] = (byte)((value & 0xFF000000) >> 24);
                    data[1] = (byte)((value & 0x00FF0000) >> 16);
                    data[2] = (byte)((value & 0x0000FF00) >> 8);
                    data[3] = (byte)((value & 0x000000FF));
                    request.Data = data;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Current Frequency, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Start Frequency
            /// </summary>
            public uint StartFrequency
            {
                get
                {
                    int freq=0;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort) SYS_PARAMS.SYS_START_FREQUENCY;
                    request.Blocks = 4;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    freq |= response.Data[0] << 24;
                    freq |= response.Data[1] << 16;
                    freq |= response.Data[2] << 8;
                    freq |= response.Data[3];
                    
                    return (uint) freq;

                }
                set
                {
                    byte[] data = new byte[4];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_START_FREQUENCY;
                    request.Blocks = 4;
                    data[0] = (byte)((value & 0xFF000000) >> 24);
                    data[1] = (byte)((value & 0x00FF0000) >> 16);
                    data[2] = (byte)((value & 0x0000FF00) >> 8);
                    data[3] = (byte)((value & 0x000000FF));
                    request.Data = data;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Start Frequency, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Stop Frequency
            /// </summary>
            public uint StopFrequency
            {
                get
                {
                    int freq = 0;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_STOP_FREQUENCY;
                    request.Blocks = 4;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    freq |= response.Data[0] << 24;
                    freq |= response.Data[1] << 16;
                    freq |= response.Data[2] << 8;
                    freq |= response.Data[3];

                    return (uint) freq;

                }
                set
                {
                    byte[] data = new byte[4];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_STOP_FREQUENCY;
                    request.Blocks = 4;
                    data[0] = (byte)((value & 0xFF000000) >> 24);
                    data[1] = (byte)((value & 0x00FF0000) >> 16);
                    data[2] = (byte)((value & 0x0000FF00) >> 8);
                    data[3] = (byte)((value & 0x000000FF));
                    request.Data = data;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Stop Frequency, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Retry Count
            /// </summary>
            public byte RetryCount
            {
                get
                {
                    byte retryCount;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_RETRY_COUNT;
                    request.Blocks = 1;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    retryCount = response.Data[0];

                    return retryCount;
                }
                set
                {
                    byte[] data = new byte[1];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_RETRY_COUNT;
                    request.Blocks = 1;
                    data[0] = value;
                    request.Data = data;
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Retry Count, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Power Level
            /// </summary>
            public double PowerLevel
            {
                get
                {
                    double pwr;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_TX_POWER;
                    request.Blocks = 1;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    pwr = (response.Data[0] + 50) / 10;

                    return pwr;
                }
                set
                {
                    byte[] data = new byte[1];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_TX_POWER;
                    request.Blocks = 1;
                    data[0] = (byte)((value * 10) - 50);
                    request.Data = data;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Power Level, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Modulation Depth
            /// </summary>
            public byte ModulationDepth
            {
                get
                {
                    byte retryCount;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_MODULATION_DEPTH;
                    request.Blocks = 1;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    retryCount = response.Data[0];

                    return retryCount;
                }
                set
                {
                    byte[] data = new byte[1];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_MODULATION_DEPTH;
                    request.Blocks = 1;
                    data[0] = value;
                    request.Data = data;
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set the Modulation Depth, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Regulatory Mode
            /// </summary>
            public byte RegulatoryMode
            {
                get
                {
                    byte retryCount;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_REGULATORY_MODE;
                    request.Blocks = 1;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    retryCount = response.Data[0];

                    return retryCount;
                }
                set
                {
                    byte[] data = new byte[1];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_REGULATORY_MODE;
                    request.Blocks = 1;
                    data[0] = value;
                    request.Data = data;
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Regulatory Mode, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Frequency Hop Type
            /// </summary>
            public byte FrequencyHop
            {
                get
                {
                    byte retryCount;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_FREQEUNCY_HOP_SEQUENCE;
                    request.Blocks = 1;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    retryCount = response.Data[0];

                    return retryCount;
                }
                set
                {
                    byte[] data = new byte[1];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_FREQEUNCY_HOP_SEQUENCE;
                    request.Blocks = 1;
                    data[0] = value;
                    request.Data = data;
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Frequency Hop Type, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Hop Channel Spacing
            /// </summary>
            public uint HopSize
            {
                get
                {
                    int freq = 0;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_HOP_CHANNEL_SPACING;
                    request.Blocks = 4;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    freq |= response.Data[0] << 24;
                    freq |= response.Data[1] << 16;
                    freq |= response.Data[2] << 8;
                    freq |= response.Data[3];

                    return (uint)freq;

                }
                set
                {
                    byte[] data = new byte[4];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_HOP_CHANNEL_SPACING;
                    request.Blocks = 4;
                    data[0] = (byte)((value & 0xFF000000) >> 24);
                    data[1] = (byte)((value & 0x00FF0000) >> 16);
                    data[2] = (byte)((value & 0x0000FF00) >> 8);
                    data[3] = (byte)((value & 0x000000FF));
                    request.Data = data;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Channel Spacing, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Reader Name - Non-Volatile Memory
            /// </summary>
            public string defaultReaderName
            {
                get
                {
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_READER_NAME;
                    request.Blocks = 32;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();


                    if (response == null)
                        return null;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return null;


                    return Encoding.ASCII.GetString(response.Data).Trim(new char[] { '\0' });
                }
                set
                {
                    STPv3Request request = new STPv3Request();
                    byte[] readername = new byte[32];
                    Encoding.ASCII.GetBytes(value).CopyTo(readername, 0);


                    request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_READER_NAME;
                    request.Blocks = 32;
                    request.Data = readername;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Default Reader Name, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Reader ID - Non-Volatile Memory
            /// </summary>
            public byte[] defaultReaderID
            {
                get
                {
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_RID;
                    request.Blocks = 4;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return null;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return null;

                    return response.Data;

                }
                set
                {
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_RID;
                    request.Blocks = 4;
                    request.Data = value;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Default Reader ID, error 0x#{0:X}", response.ResponseCode));

                    request.Data.CopyTo(this.m_RID, 0);
                }
            }

            /// <summary>
            /// Current Frequency - Default Value
            /// </summary>
            public uint defaultCurrentFrequency
            {
                get
                {
                    int freq = 0;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_CURRENT_FREQUENCY;
                    request.Blocks = 4;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    freq |= response.Data[0] << 24;
                    freq |= response.Data[1] << 16;
                    freq |= response.Data[2] << 8;
                    freq |= response.Data[3];

                    return (uint)freq;

                }
                set
                {
                    byte[] data = new byte[4];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_CURRENT_FREQUENCY;
                    request.Blocks = 4;
                    data[0] = (byte)((value & 0xFF000000) >> 24);
                    data[1] = (byte)((value & 0x00FF0000) >> 16);
                    data[2] = (byte)((value & 0x0000FF00) >> 8);
                    data[3] = (byte)((value & 0x000000FF));
                    request.Data = data;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Default Current Frequency, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Start Frequency
            /// </summary>
            public uint defaultStartFrequency
            {
                get
                {
                    int freq = 0;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_START_FREQUENCY;
                    request.Blocks = 4;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    freq |= response.Data[0] << 24;
                    freq |= response.Data[1] << 16;
                    freq |= response.Data[2] << 8;
                    freq |= response.Data[3];

                    return (uint)freq;

                }
                set
                {
                    byte[] data = new byte[4];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_START_FREQUENCY;
                    request.Blocks = 4;
                    data[0] = (byte)((value & 0xFF000000) >> 24);
                    data[1] = (byte)((value & 0x00FF0000) >> 16);
                    data[2] = (byte)((value & 0x0000FF00) >> 8);
                    data[3] = (byte)((value & 0x000000FF));
                    request.Data = data;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Default Start Frequency, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Stop Frequency - Non-Volatile Memory
            /// </summary>
            public uint defaultStopFrequency
            {
                get
                {
                    int freq = 0;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_STOP_FREQUENCY;
                    request.Blocks = 4;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    freq |= response.Data[0] << 24;
                    freq |= response.Data[1] << 16;
                    freq |= response.Data[2] << 8;
                    freq |= response.Data[3];

                    return (uint)freq;

                }
                set
                {
                    byte[] data = new byte[4];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_STOP_FREQUENCY;
                    request.Blocks = 4;
                    data[0] = (byte)((value & 0xFF000000) >> 24);
                    data[1] = (byte)((value & 0x00FF0000) >> 16);
                    data[2] = (byte)((value & 0x0000FF00) >> 8);
                    data[3] = (byte)((value & 0x000000FF));
                    request.Data = data;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Default Stop Frequency, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Retry Count - Non-Volatile Memory
            /// </summary>
            public byte defaultRetryCount
            {
                get
                {
                    byte retryCount;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_RETRY_COUNT;
                    request.Blocks = 1;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    retryCount = response.Data[0];

                    return retryCount;
                }
                set
                {
                    byte[] data = new byte[1];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_RETRY_COUNT;
                    request.Blocks = 1;
                    data[0] = value;
                    request.Data = data;
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Default Retry Count, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Power Level - Non-Volatile Memory
            /// </summary>
            public double defaultPowerLevel
            {
                get
                {
                    double pwr;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_TX_POWER;
                    request.Blocks = 1;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    pwr = (response.Data[0] + 50) / 10;

                    return pwr;
                }
                set
                {
                    byte[] data = new byte[1];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_TX_POWER;
                    request.Blocks = 1;
                    data[0] = (byte)((value * 10) - 50);
                    request.Data = data;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Default Power Level, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Modulation Depth - Non-Volatile Memory
            /// </summary>
            public byte defaultModulationDepth
            {
                get
                {
                    byte retryCount;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_MODULATION_DEPTH;
                    request.Blocks = 1;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    retryCount = response.Data[0];

                    return retryCount;
                }
                set
                {
                    byte[] data = new byte[1];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_MODULATION_DEPTH;
                    request.Blocks = 1;
                    data[0] = value;
                    request.Data = data;
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set the Default Modulation Depth, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Regulatory Mode - Non-Volatile Memory
            /// </summary>
            public byte defaultRegulatoryMode
            {
                get
                {
                    byte regMode;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_REGULATORY_MODE;
                    request.Blocks = 1;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    regMode = response.Data[0];

                    return regMode;
                }
                set
                {
                    byte[] data = new byte[1];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_REGULATORY_MODE;
                    request.Blocks = 1;
                    data[0] = value;
                    request.Data = data;
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Default Regulatory Mode, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Frequency Hop Type - Non-Volatile Memory
            /// </summary>
            public byte defaultFrequencyHop
            {
                get
                {
                    byte retryCount;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_FREQEUNCY_HOP_SEQUENCE;
                    request.Blocks = 1;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    retryCount = response.Data[0];

                    return retryCount;
                }
                set
                {
                    byte[] data = new byte[1];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_FREQEUNCY_HOP_SEQUENCE;
                    request.Blocks = 1;
                    data[0] = value;
                    request.Data = data;
                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Default Frequency Hop Type, error 0x#{0:X}", response.ResponseCode));

                }
            }

            /// <summary>
            /// Hop Channel Spacing - Non-Volatile Memory
            /// </summary>
            public uint defaultHopSize
            {
                get
                {
                    int freq = 0;
                    STPv3Request request = new STPv3Request();

                    request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_HOP_CHANNEL_SPACING;
                    request.Blocks = 4;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        return 0;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        return 0;

                    freq |= response.Data[0] << 24;
                    freq |= response.Data[1] << 16;
                    freq |= response.Data[2] << 8;
                    freq |= response.Data[3];

                    return (uint)freq;

                }
                set
                {
                    byte[] data = new byte[4];
                    STPv3Request request = new STPv3Request();
                    request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                    request.Address = (ushort)SYS_PARAMS.SYS_HOP_CHANNEL_SPACING;
                    request.Blocks = 4;
                    data[0] = (byte)((value & 0xFF000000) >> 24);
                    data[1] = (byte)((value & 0x00FF0000) >> 16);
                    data[2] = (byte)((value & 0x0000FF00) >> 8);
                    data[3] = (byte)((value & 0x000000FF));
                    request.Data = data;
                    request.RID = this.m_RID;

                issue:
                    request.Issue(this.m_device);
                    STPv3Response response = request.GetResponse();

                    if (response == null)
                        throw new IOException("No response received from the reader");

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (!response.Success)
                        throw new ReaderException(String.Format("Unable to set Default Channel Spacing, error 0x#{0:X}", response.ResponseCode));

                }
            }

            public override byte[] ReadTagData(Tag tag, ushort address, ushort blocks)
            {

                STPv3Request request = new STPv3Request();
                request.Tag = tag;
                request.Command = STPv3Commands.READ_TAG;
                request.Address = address;
                request.Blocks = blocks;
                request.RID = this.m_RID;

              issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return null;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return null;

                return response.Data;
            }

            public override bool WriteTagData(Tag tag, byte[] data, ushort address, ushort blocks)
            {

                STPv3Request request = new STPv3Request();
                request.Tag = tag;
                request.Command = STPv3Commands.WRITE_TAG;
                request.Data = data;
                request.Address = address;
                request.Blocks = blocks;
                request.RID = this.m_RID;
                
              issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return false;

                return true;
            }

            /// <summary>
            /// This function is used for Locking the Tag Data / Tag Blocks. It is very similar to the Write Tag function
            /// with the exception of the Lock Flag being set in this case.
            /// </summary>
            /// <param name="tag"></param>
            /// <param name="data"></param>
            /// <param name="address"></param>
            /// <param name="blocks"></param>
            /// <returns></returns>
            public bool LockTagData(Tag tag, byte[] data, ushort address, ushort blocks)
            {

                STPv3Request request = new STPv3Request();
                request.Tag = tag;
                request.Command = STPv3Commands.WRITE_TAG;
                request.Data = data;
                request.Address = address;
                request.Blocks = blocks;
                request.Lock = true;
                request.RID = this.m_RID;

            issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return false;

                return true;
            }

            /// <summary>
            /// Writes a system parameter to the reader.  Address denotes parameter
            /// </summary>
            /// <param name="data">Data to be written</param>
            /// <param name="address">Address to write to</param>
            /// <param name="blocks">Number of blocks to write</param>
            public bool WriteSystemParameter(byte[] data, ushort address, ushort blocks)
            {
                STPv3Request request = new STPv3Request();

                request.Command = STPv3Commands.WRITE_SYSTEM_PARAMETER;
                request.Address = address;
                request.Blocks = blocks;
                request.Data = data;
                request.RID = this.m_RID;

              issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return false;

                if (address == 4) // Reader ID is being set
                    request.Data.CopyTo(this.m_RID, 0);

                return true;
            }

            /// <summary>
            /// Reads stored system parameter from reader.  Address denotes parameter
            /// </summary>
            /// <param name="address">Address to read from</param>
            /// <param name="blocks">Number of blocks to read</param>
            /// <returns>Returns null if read failed, data otherwise</returns>
            public byte[] ReadSystemParameter(ushort address, ushort blocks)
            {
                STPv3Request request = new STPv3Request();

                request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
                request.Address = address;
                request.Blocks = blocks;
                request.RID = this.m_RID;

              issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return null;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return null;

                return response.Data;
            }
              
            /// <summary>
            /// Reads a stored default parameter from the reader.  Address denotes parameter
            /// </summary>
            /// <param name="address">Address to read from</param>
            /// <param name="blocks">Number of blocks to write</param>
            /// <returns>Return null if read failed, data otherwise</returns>
            public byte[] RetrieveDefaultParameter(ushort address, ushort blocks)
            {
                STPv3Request request = new STPv3Request();

                request.Command = STPv3Commands.RETRIEVE_DEFAULT_SYSTEM_PARAMETER;
                request.Address = address;
                request.Blocks = blocks;
                request.RID = this.m_RID;

              issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return null;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return null;

                return response.Data;   
                
            }
            /// <summary>
            /// Writes a new default system parameter.  Address denotes parameter
            /// </summary>
            /// <param name="data">Data to write</param>
            /// <param name="address">Address to write to</param>
            /// <param name="blocks">Number of blocks to write</param>           
            public bool StoreDefaultParameter(byte[] data, ushort address, ushort blocks)
            {
                STPv3Request request = new STPv3Request();

                request.Command = STPv3Commands.STORE_DEFAULT_SYSTEM_PARAMETER;
                request.Address = address;
                request.Blocks = blocks;
                request.Data = data;
                request.RID = this.m_RID;

              issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return false;

                return true;
            }

            /// <summary>
            /// Send Tag Password - Sends the Tag Password to the Tag
            /// </summary>
            /// <param name="tag"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public bool SendTagPassword(Tag tag, byte[] data)
            {
                STPv3Request request = new STPv3Request();
                request.Tag = tag;
                request.Command = STPv3Commands.SEND_TAG_PASSWORD;
                request.Data = data;
                request.Address = 0;
                request.Blocks = 0;
                request.RID = this.m_RID;

            issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return false;

                return true;
            }

            /// <summary>
            /// Read Tag Configuration
            /// </summary>
            /// <param name="tag"></param>
            /// <param name="address"></param>
            /// <param name="blocks"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public byte[] ReadTagConfig(Tag tag, ushort address, ushort blocks, byte[] data)
            {
                byte[] tagConfig = new byte[1024];
                STPv3Request request = new STPv3Request();
                request.Tag = tag;
                request.Command = STPv3Commands.READ_TAG_CONFIG;
                request.Address = address;
                request.Blocks = blocks;
                request.Data = data;
                request.RID = this.m_RID;

            issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return null;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return null;

                return response.Data;

            }

            /// <summary>
            /// Write Tag Configuration
            /// </summary>
            /// <param name="tag"></param>
            /// <param name="address"></param>
            /// <param name="blocks"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public bool WriteTagConfig(Tag tag, ushort address, ushort blocks, byte[] data)
            {
                STPv3Request request = new STPv3Request();
                request.Tag = tag;
                request.Command = STPv3Commands.WRITE_TAG_CONFIG;
                request.Data = data;
                request.Address = address;
                request.Blocks = blocks;
                request.RID = this.m_RID;

            issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return false;

                return true;
            }

            /// <summary>
            /// Enable EAS
            /// </summary>
            /// <param name="tag"></param>
            /// <returns></returns>
            public bool enableEAS(Tag tag)
            {
                STPv3Request request = new STPv3Request();
                request.Tag = tag;
                request.Command = STPv3Commands.ENABLE_EAS;
                request.RID = this.m_RID;

            issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return false;

                return true;
            }

            /// <summary>
            /// Disable EAS
            /// </summary>
            /// <param name="tag"></param>
            /// <returns></returns>
            public bool disableEAS(Tag tag)
            {
                STPv3Request request = new STPv3Request();
                request.Tag = tag;
                request.Command = STPv3Commands.DISABLE_EAS;
                request.RID = this.m_RID;

            issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return false;

                return true;

            }

            /// <summary>
            /// Scan EAS
            /// </summary>
            /// <param name="tag"></param>
            /// <returns></returns>
            public bool scanEAS(Tag tag)
            {
                STPv3Request request = new STPv3Request();
                request.Tag = tag;
                request.Command = STPv3Commands.SCAN_EAS;
                request.RID = this.m_RID;

            issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return false;

                return true;
            }            
            
            public override bool SelectTag(ref Tag tag)
            {
                STPv3Request request = new STPv3Request();
                request.Tag = tag;
                request.RID = this.m_RID;
                request.Command = STPv3Commands.SELECT_TAG;
                
              issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();

                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return false;

                tag.TID = response.TID;
                if (((int)tag.Type & 0x000F) == 0x0000)     
                    tag.Type = response.TagType;

                return true;
                
            }

            public override bool InventoryTags(Tag tag, bool loop, InventoryTagDelegate itd, object context)
            {
                STPv3Response response;
                STPv3Request request = new STPv3Request();
                
                request.Tag = tag;
                //request.RID = reader.ReaderID;
                request.Command = STPv3Commands.SELECT_TAG;
                request.Inventory = true;
                request.Loop = loop;

            issue:
                request.Issue(this.m_device);

                response = request.GetResponse();

                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                while (true)
                {
                    response = request.GetResponse();

                    if (response == null)
                        continue;

                    if (!response.Success)
                        return false;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_PASS)
                    {
                        Tag iTag = new Tag();
                        iTag.TID = response.TID;
                        iTag.Type = response.TagType;

                        //BTM - If we are simply performing an inventory then we let it run to
                        //completion to prevent later synchronization issues
                        if (itd(iTag, context) && loop)
                        {
                            return true;
                        }
                    }
                }
            }

            public ArrayList InventoryTags(Tag tag)
            {
                ArrayList tagArray = new ArrayList();

                //Build select tag request. 
                STPv3Response response;
                STPv3Request requestTag = new STPv3Request();
                requestTag.Tag = tag;
                requestTag.Command = STPv3Commands.SELECT_TAG;
                requestTag.Inventory = true;

            issue:
                requestTag.Issue(this.m_device);

                while (true)
                {
                    response = requestTag.GetResponse();

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                        goto issue;

                    if (response == null)
                        return tagArray;

                    if (!response.Success)
                        return tagArray;

                    if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_PASS)
                    {
                        Tag iTag = new Tag();

                        iTag.Type = response.TagType;
                        iTag.TID = response.TID;

                        tagArray.Add(iTag);
                    }
                }
            }
            /// <summary>
            /// Uploads new firmware to a reader.  Takes a string containing the path to the .shf file
            /// </summary>
            /// <param name="file">Path to .shf file.  String.</param>
            /// <returns></returns>
            public bool UploadFirmware(string file)
            {
                STPv3Request request = new STPv3Request();
                request.RID = this.m_RID;
                request.Command = STPv3Commands.ENTER_BOOTLOAD;

              issue:
                request.Issue(this.m_device);
                STPv3Response response = request.GetResponse();
                
                if (response == null)
                    return false;

                if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_LOOP_OFF)
                    goto issue;

                if (!response.Success)
                    return false;

                STPv3Request.UploadFirmware(this.m_device, file);
                
                return true;
            }
                        
        }

    }
}

