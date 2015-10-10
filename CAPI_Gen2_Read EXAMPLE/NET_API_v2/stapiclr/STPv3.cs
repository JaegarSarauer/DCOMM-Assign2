using System;
using SkyeTek.Devices;
using SkyeTek.Tags;
using System.IO;

using System.ComponentModel;

namespace SkyeTek
{
    namespace STP
    {

        /// <summary>
        /// STPv2 and STPv3 can operate in one of two modes.  Currently only BINARY mode is supported.
        /// </summary>
        public enum ProtocolMode
        {
            ASCII,
            BINARY
        }

        /// <summary>
        /// Wrapper around the base Exception class to denote a CRC mismatch
        /// </summary>
        public class CRCException : Exception
        {
            public CRCException()
            {
            }
        }

        public class Utils
        {
            public static ushort crc16(ushort seed, byte[] data, int offset, int count)
            {
                ushort crc16 = seed;

                if (count < 1)
                    return seed;

                if ((offset + count) > data.Length)
                    throw new ArgumentException("offset + count > data.Length");

                do
                {
                    crc16 ^= data[offset++];

                    for (int j = 0; j < 8; j++)
                    {
                        if ((crc16 & 0x0001) == 1)
                        {
                            crc16 >>= 1;
                            crc16 ^= 0x8408;
                        }
                        else
                            crc16 >>= 1;
                    }
                } while (--count != 0);

                return crc16;
            }
        }
    }

    /// The following is a typical use case:
    /// <example>
    /// STPv3Request request;
    /// STPv3Response response;
    /// Device dd;
    /// 
    /// //Get a list of the attached devices
    /// Device[] devices = USBDeviceFactory.Enumerate();
    ///             
    /// if(devices.Length == 0)
    /// {
    ///   Console.Out.WriteLine("No USB Devices found");
    ///   return;
    /// }
    /// 
    /// dd = devices[0];
    /// 
    /// dd.Open();
    /// 
    /// try
    /// {
    ///   request = new STPv3Request();
    ///                 
    ///   //Read serial number from reader
    ///  request.Command = STPv3Commands.READ_SYSTEM_PARAMETER;
    ///   request.Address = 0;
    ///   request.Blocks = 4;
    /// 
    ///   request.Issue(dd);
    ///              
    ///   response = request.GetResponse();
    /// 
    ///   if ((response == null) || (!response.Success))
    ///   {
    ///     Console.Out.WriteLine("Unable to read serial number from reader");
    ///     return;
    ///   }
    /// 
    ///   Console.Out.WriteLine(String.Format("Serial Number: {0}",
    ///                     String.Join("", Array.ConvertAll<byte, string>
    ///                     (response.Data, delegate(byte value) { 
    ///                      return String.Format("{0:X2}", value); }))));
    /// }
    /// catch (Exception ex)
    /// {
    ///   Console.Out.WriteLine(ex.ToString());
    /// }
    /// </example>
    namespace STPv3
    {

        static internal class STPv3
        {
            public static byte STX = 0x02;
            public static byte ACK = 0x06;
            public static byte NACK = 0x15;
            //This number is totally made up
            public static int MAX_LENGTH = 1200;
            public static uint TIMEOUT = 10000;
        }

        [Flags]
        internal enum STPv3Flags : ushort
        {
            NONE = 0x0000,
            LOOP = 0x0001,
            INV = 0x0002,
            LOCK = 0x0004,
            RF = 0x0008,
            AFI = 0x0010,
            CRC = 0x0020,
            TID = 0x0040,
            RID = 0x0080,
            ENCRYPTION = 0x0100,
            HMAC = 0x0200,
            SESSION = 0x0400,
            DATA = 0x0800,
            RFU = 0xF000
        }

        /// <summary>
        /// This class encapsulates all of the information we need to know about
        /// a STPv3 command.
        /// </summary>
        public class STPv3Command
        {
            private ushort m_code;
            private string m_name;
            private bool m_tagRequired;
            private bool m_addressRequired;
            private bool m_dataRequired;
            private int m_timeout;

            public STPv3Command(ushort code,
                string name,
                int timeout,
                bool tagRequired,
                bool addressRequired,
                bool dataRequired)
            {
                this.m_code = code;
                this.m_timeout = timeout;
                this.m_name = name;
                this.m_tagRequired = tagRequired;
                this.m_addressRequired = addressRequired;
                this.m_dataRequired = dataRequired;
            }

            public ushort Code
            {
                get { return this.m_code; }
            }

            public string Name
            {
                get { return this.m_name; }
            }

            public int Timeout
            {
                get { return this.m_timeout; }
            }

            public bool TagRequired
            {
                get { return this.m_tagRequired; }
            }

            public bool AddressRequired
            {
                get { return this.m_addressRequired; }
            }

            public bool DataRequired
            {
                get { return this.m_dataRequired; }
            }

            public override string ToString()
            {
                return this.m_name;
            }
        }

        /// <summary>
        /// This class holds the STPv3 Commands supported by this API.  Please
        /// see SkyeTek Protocol v3 documentation for a description of what each
        /// command does.
        /// </summary>
        public static class STPv3Commands
        {
            #region Commands
            public static STPv3Command SELECT_TAG = new STPv3Command(0x0101, "Select Tag", 300, true, false, false);
            public static STPv3Command READ_TAG = new STPv3Command(0x0102, "Read Tag", 300, true, true, false);
            public static STPv3Command WRITE_TAG = new STPv3Command(0x0103, "Write Tag", 300, true, true, true);
            public static STPv3Command ACTIVATE_TAG = new STPv3Command(0x0104, "Activate Tag", 300, true, false, false);
            public static STPv3Command DEACTIVATE_TAG = new STPv3Command(0x0105, "Deactivate Tag", 300, true, false, false);
            public static STPv3Command SET_TAG_BIT_RATE = new STPv3Command(0x0106, "Set Tag Bitrate", 300, true, false, false);
            public static STPv3Command GET_TAG_INFO = new STPv3Command(0x0107, "Get Tag Info", 300, true, false, false);
            public static STPv3Command GET_LOCK_STATUS = new STPv3Command(0x0108, "Get Lock Status", 300, true, true, false);
            public static STPv3Command KILL_TAG = new STPv3Command(0x0109, "Kill Tag", 300, true, false, false);
            public static STPv3Command REVIVE_TAG = new STPv3Command(0x010A, "Revive Tag", 300, true, false, false);
            public static STPv3Command ERASE_TAG = new STPv3Command(0x010B, "Erase Tag", 300, true, true, false);
            public static STPv3Command FORMAT_TAG = new STPv3Command(0x010C, "Format Tag", 300, true, false, false);
            public static STPv3Command DESELECT_TAG = new STPv3Command(0x010D, "Deselect Tag", 300, true, false, false);
            public static STPv3Command READ_TAG_CONFIG = new STPv3Command(0x0110, "Read Tag Config", 300, true, true, false);
            public static STPv3Command WRITE_TAG_CONFIG = new STPv3Command(0x0111, "Write Tag Config", 300, true, true, true);

            public static STPv3Command AUTHENTICATE_TAG = new STPv3Command(0x0201, "Authenticate Tag", 300, true, false, true);
            public static STPv3Command SEND_TAG_PASSWORD = new STPv3Command(0x0202, "Send Tag Password", 300, true, false, true);
            public static STPv3Command INIT_SECURE_MEMORY = new STPv3Command(0x0203, "Initialize Secure Memory", 300, true, false, true);
            public static STPv3Command SETUP_SECURE_MEMORY = new STPv3Command(0x0204, "Setup Secure Memory", 300, true, false, true);

            public static STPv3Command GET_APPLICATION_IDS = new STPv3Command(0x0301, "Get Application IDs", 400, true, false, true);
            public static STPv3Command SELECT_APPLICATION = new STPv3Command(0x0302, "Select Application", 400, true, false, true);
            public static STPv3Command CREATE_APPLICATION = new STPv3Command(0x0303, "Create Application", 400, true, false, true);
            public static STPv3Command DELETE_APPLICATION = new STPv3Command(0x0304, "Delete Application", 400, true, false, true);

            public static STPv3Command GET_FILE_IDS = new STPv3Command(0x0401, "Get File Ids", 400, true, false, true);
            public static STPv3Command SELECT_FILE = new STPv3Command(0x0402, "Select File", 400, true, false, true);
            public static STPv3Command CREATE_FILE = new STPv3Command(0x0403, "Create File", 400, true, false, true);
            public static STPv3Command GET_FILE_SETTINGS = new STPv3Command(0x0404, "Get File Settings", 400, true, false, true);
            public static STPv3Command CHANGE_FILE_SETTINGS = new STPv3Command(0x0405, "Change File Settings", 400, true, false, true);
            public static STPv3Command READ_FILE = new STPv3Command(0x0406, "Read File", 400, true, true, true);
            public static STPv3Command WRITE_FILE = new STPv3Command(0x0407, "Write File", 400, true, true, true);
            public static STPv3Command DELETE_FILE = new STPv3Command(0x0408, "Delete File", 400, true, false, true);
            public static STPv3Command CLEAR_FILE = new STPv3Command(0x0409, "Clear File", 400, true, false, true);
            public static STPv3Command CREDIT_VALUE_FILE = new STPv3Command(0x040A, "Credit Value File", 400, true, false, true);
            public static STPv3Command DEBIT_VALUE_FILE = new STPv3Command(0x040B, "Debit Value File", 400, true, false, true);
            public static STPv3Command LIMITED_CREDIT_VALUE_FILE = new STPv3Command(0x040C, "Limited Credit Value File", 400, true, false, true);
            public static STPv3Command GET_VALUE = new STPv3Command(0x040D, "Get Value", 400, true, false, true);
            public static STPv3Command COMMIT_TRANSACTION = new STPv3Command(0x040E, "Commit Transaction", 400, true, false, false);
            public static STPv3Command ABORT_TRANSACTION = new STPv3Command(0x040F, "Abort Transaction", 400, true, false, false);
            public static STPv3Command READ_RECORDS = new STPv3Command(0x0410, "Read Records", 400, true, true, true);
            public static STPv3Command WRITE_RECORD = new STPv3Command(0x0411, "Write Record", 400, true, true, true);
            public static STPv3Command CHANGE_KEY_SETTINGS = new STPv3Command(0x0412, "Change Key Settings", 400, true, false, true);
            public static STPv3Command GET_KEY_SETTINGS = new STPv3Command(0x0413, "Get Key Settings", 400, true, false, false);
            public static STPv3Command GET_KEY_VERSION = new STPv3Command(0x0414, "Get Key Version", 400, true, false, true);
            public static STPv3Command CHANGE_KEY = new STPv3Command(0x0415, "Change Key", 400, true, false, true);

            public static STPv3Command ENABLE_EAS = new STPv3Command(0x0501, "Enable EAS", 300, true, false, false);
            public static STPv3Command DISABLE_EAS = new STPv3Command(0x0502, "Disable EAS", 300, true, false, false);
            public static STPv3Command SCAN_EAS = new STPv3Command(0x0503, "Scan EAS", 300, true, false, false);
            public static STPv3Command WRITE_AFI = new STPv3Command(0x0504, "Write AFI", 300, true, false, true);
            public static STPv3Command READ_AFI = new STPv3Command(0x0505, "Read AFI", 300, true, false, false);
            public static STPv3Command WRITE_DSFID = new STPv3Command(0x0506, "Write DSFID", 300, true, false, true);
            public static STPv3Command READ_DSFID = new STPv3Command(0x0507, "Read DSFID", 300, true, false, false);

            public static STPv3Command STORE_KEY = new STPv3Command(0x0601, "Store Key", 300, true, true, true);
            public static STPv3Command LOAD_KEY = new STPv3Command(0x0602, "Load Key", 300, true, true, true);

            public static STPv3Command INTERFACE_SEND = new STPv3Command(0x0701, "Interface Send", 400, true, false, true);
            public static STPv3Command TRANSPORT_SEND = new STPv3Command(0x0702, "Transport Send", 400, true, false, true);

            public static STPv3Command INITIATE_PAYMENT = new STPv3Command(0x0801, "Initiate Payment", 500, true, false, true);
            public static STPv3Command COMPUTE_PAYMENT = new STPv3Command(0x0802, "Compute Payment", 500, true, false, true);

            public static STPv3Command RESET_DEVICE = new STPv3Command(0x1102, "Reset Device", 500, false, false, false);
            public static STPv3Command ENTER_BOOTLOAD = new STPv3Command(0x1103, "Enter Bootload Mode", 500, false, false, false);

            public static STPv3Command READ_SYSTEM_PARAMETER = new STPv3Command(0x1201, "Read System Parameter", 300, false, true, false);
            public static STPv3Command WRITE_SYSTEM_PARAMETER = new STPv3Command(0x1202, "Write System Parameter", 300, false, true, true);

            public static STPv3Command STORE_DEFAULT_SYSTEM_PARAMETER = new STPv3Command(0x1301, "Store Default System Parameter", 300, false, true, true);
            public static STPv3Command RETRIEVE_DEFAULT_SYSTEM_PARAMETER = new STPv3Command(0x1302, "Retrieve Default System Parameter", 300, false, true, false);

            public static STPv3Command AUTHENTICATE_READER = new STPv3Command(0x1401, "Authenticate Reader", 300, false, false, true);
            public static STPv3Command ENABLE_DEBUG = new STPv3Command(0x1402, "Enable Debug", 300, false, false, false);
            public static STPv3Command DISABLE_DEBUG = new STPv3Command(0x1403, "Disable Debug", 300, false, false, false);
            public static STPv3Command DEBUG_MESSAGES = new STPv3Command(0x1404, "Debug Messages", 300, false, false, false);
            public static STPv3Command ENTER_PAYMENT_SCAN_MODE = new STPv3Command(0x1405, "Enter Payment Scan Mode", 300, false, false, false);



            #endregion

            /// <summary>
            /// Retrieves the supported commands <see cref="STPv3Command"/> and puts them into an array
            /// </summary>
            /// <returns>Array of supported commands</returns>
            public static STPv3Command[] GetCommands()
            {
                STPv3Command[] commands;
                System.Reflection.FieldInfo[] fieldInfos;

                fieldInfos = typeof(STPv3Commands).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                commands = new STPv3Command[fieldInfos.Length];

                for (int ix = 0; ix < fieldInfos.Length; ix++)
                    if (fieldInfos[ix].FieldType == typeof(STPv3Command))
                        commands[ix] = (STPv3Command)fieldInfos[ix].GetValue(null);

                return commands;
            }
        }

        /// <summary>
        /// Enumeration of Command codes supported
        /// </summary>
        public enum Commands: ushort
        {
            SELECT_TAG = 0x0101,
            READ_TAG = 0x0102,
            WRITE_TAG = 0x0103,
            ACTIVATE_TAG = 0x0104,
            DEACTIVATE_TAG = 0x0105,
            SET_TAG_BIT_RATE = 0x0106, 
            GET_TAG_INFO = 0x0107,
            GET_LOCK_STATUS = 0x0108, 
            KILL_TAG = 0x0109,
            REVIVE_TAG = 0x010A,
            ERASE_TAG = 0x010B, 
            FORMAT_TAG = 0x010C,
            DESELECT_TAG = 0x010D, 
            READ_TAG_CONFIG = 0x0110,  
            WRITE_TAG_CONFIG = 0x0111, 
            AUTHENTICATE_TAG = 0x0201,
            SEND_TAG_PASSWORD = 0x0202, 
            INIT_SECURE_MEMORY = 0x0203,
            SETUP_SECURE_MEMORY = 0x0204, 
            GET_APPLICATION_IDS = 0x0301, 
            SELECT_APPLICATION = 0x0302, 
            CREATE_APPLICATION = 0x0303, 
            DELETE_APPLICATION = 0x0304,
            GET_FILE_IDS = 0x0401, 
            SELECT_FILE = 0x0402, 
            CREATE_FILE = 0x0403, 
            GET_FILE_SETTINGS = 0x0404, 
            CHANGE_FILE_SETTINGS = 0x0405, 
            READ_FILE = 0x0406, 
            WRITE_FILE = 0x0407, 
            DELETE_FILE = 0x0408, 
            CLEAR_FILE = 0x0409,
            CREDIT_VALUE_FILE = 0x040A, 
            DEBIT_VALUE_FILE = 0x040B, 
            LIMITED_CREDIT_VALUE_FILE = 0x040C, 
            GET_VALUE = 0x040D, 
            COMMIT_TRANSACTION = 0x040E,
            ABORT_TRANSACTION = 0x040F, 
            READ_RECORDS = 0x0410, 
            WRITE_RECORD = 0x0411, 
            CHANGE_KEY_SETTINGS = 0x0412, 
            GET_KEY_SETTINGS = 0x0413, 
            GET_KEY_VERSION = 0x0414,
            CHANGE_KEY = 0x0415, 
            ENABLE_EAS = 0x0501, 
            DISABLE_EAS = 0x0502, 
            SCAN_EAS = 0x0503, 
            WRITE_AFI = 0x0504, 
            READ_AFI = 0x0505, 
            WRITE_DSFID = 0x0506,
            READ_DSFID = 0x0507, 
            STORE_KEY = 0x0601,
            LOAD_KEY = 0x0602,
            INTERFACE_SEND = 0x0701,
            TRANSPORT_SEND = 0x0702,
            INITIATE_PAYMENT = 0x0801,
            COMPUTE_PAYMENT = 0x0802,
            RESET_DEVICE = 0x1102,
            ENTER_BOOTLOAD = 0x1103,
            READ_SYSTEM_PARAMETER = 0x1201,
            WRITE_SYSTEM_PARAMETER = 0x1202,
            STORE_DEFAULT_SYSTEM_PARAMETER = 0x1301,
            RETRIEVE_DEFAULT_SYSTEM_PARAMETER = 0x1302,
            AUTHENTICATE_READER = 0x1401,
            ENABLE_DEBUG = 0x1402,
            DISABLE_DEBUG = 0x1403,
            DEBUG_MESSAGES = 0x1404,
            ENTER_PAYMENT_SCAN_MODE = 0x1405
        }

        /// <summary>
        /// Enumeration of STPv3 response codes supported
        /// </summary>
        public enum STPv3ResponseCode : ushort
        {
            SELECT_TAG_PASS = 0x101,
            READ_TAG_DATA_PASS = 0x102,
            WRITE_TAG_DATA_PASS = 0x103,
            ACTIVATE_TAG_TYPE_PASS = 0x104,
            DEACTIVATE_TAG_TYPE_PASS = 0x105,
            SET_TAG_BIT_RATE_PASS = 0x106,
            GET_TAG_INFO_PASS = 0x107,
            GET_LOCK_STATUS_PASS = 0x108,
            KILL_TAG_PASS = 0x109,
            REVIVE_TAG_PASS = 0x10A,
            ERASE_TAG_PASS = 0x10B,
            FORMAT_TAG_PASS = 0x10C,
            DESELECT_TAG_PASS = 0x10D,
            READ_TAG_CONFIG_PASS = 0x110,
            WRITE_TAG_CONFIG_PASS = 0x111,
            SELECT_TAG_LOOP_ON = 0x1C1,
            AUTHENTICATE_TAG_PASS = 0x201,
            SEND_TAG_PASSWORD_PASS = 0x202,
            INIT_SECURE_MEMORY_PASS = 0x203,
            SETUP_SECURE_MEMORY_PASS = 0x204,
            GET_APPLICATION_IDS_PASS = 0x301,
            SELECT_APPLICATION_PASS = 0x302,
            CREATE_APPLICATION_PASS = 0x303,
            DELETE_APPLICATION_PASS = 0x304,
            GET_FILE_IDS_PASS = 0x401,
            SELECT_FILE_PASS = 0x402,
            CREATE_FILE_PASS = 0x403,
            GET_FILE_SETTINGS_PASS = 0x404,
            CHANGE_FILE_SETTINGS_PASS = 0x405,
            READ_FILE_PASS = 0x406,
            WRITE_FILE_PASS = 0x407,
            DELETE_FILE_PASS = 0x408,
            CLEAR_FILE_PASS = 0x409,
            CREDIT_VALUE_FILE_PASS = 0x40A,
            DEBIT_VALUE_FILE_PASS = 0x40B,
            LIMITED_CREDIT_VALUE_FILE_PASS = 0x40C,
            GET_VALUE_PASS = 0x40D,
            COMMIT_TRANSACTION_PASS = 0x40E,
            ABORT_TRANSACTION_PASS = 0x40F,
            READ_RECORDS_PASS = 0x410,
            WRITE_RECORD_PASS = 0x411,
            CHANGE_KEY_SETTINGS_PASS = 0x412,
            GET_KEY_SETTINGS_PASS = 0x413,
            GET_KEY_VERSION_PASS = 0x414,
            CHANGE_KEY_PASS = 0x415,
            ENABLE_EAS_PASS = 0x501,
            DISABLE_EAS_PASS = 0x502,
            SCAN_EAS_PASS = 0x503,
            WRITE_AFI_PASS = 0x504,
            READ_AFI_PASS = 0x505,
            WRITE_DSFID_PASS = 0x506,
            READ_DSFID_PASS = 0x507,
            STORE_KEY_PASS = 0x601,
            LOAD_KEY_PASS = 0x602,
            INTERFACE_SEND_PASS = 0x701,
            TRANSPORT_SEND_PASS = 0x702,
            INITIATE_PAYMENT_PASS = 0x801,
            COMPUTE_PAYMENT_PASS = 0x802,
            LOAD_DEFAULTS_PASS = 0x1101,
            RESET_DEVICE_PASS = 0x1102,
            BOOTLOAD_PASS = 0x1103,
            READ_SYSTEM_PARAMETER_PASS = 0x1201,
            WRITE_SYSTEM_PARAMETER_PASS = 0x1202,
            STORE_DEFAULT_SYSTEM_PARAMETER_PASS = 0x1301,
            RETRIEVE_DEFAULT_SYSTEM_PARAMETER_PASS = 0x1302,
            AUTHENTICATE_READER_PASS = 0x1401,
            ENABLE_DEBUG_PASS = 0x1402,
            DISABLE_DEBUG_PASS = 0x1403,
            GET_DEBUG_MESSAGES_PASS = 0x1404,
            ENTER_PAYMENT_SCAN_MODE_PASS = 0x1405,
            INVALID_TAG_TYPE = 0x8001,
            NO_TAG_IN_FIELD = 0x8002,
            COLLISION_DETECTED = 0x8003,
            TAG_DATA_INTEGRITY_CHECK_FAILED = 0x8004,
            TAG_BLOCKS_LOCKED = 0x8005,
            NOT_AUTHENTICATED = 0x8006,
            NO_TAG_ID_MATCH = 0x8007,
            TAG_DATA_RATE_NOT_SUPPORTED = 0x800B,
            ENCRYPT_TAG_DATA_FAIL = 0x800C,
            DECRYPT_TAG_DATA_FAIL = 0x800D,
            INVALID_SIGNATURE_HMAC = 0x800E,
            INVALID_KEY_FOR_AUTHENTICATION = 0x800F,
            NO_APPLICATION_PRESENT = 0x8010,
            FILE_NOT_FOUND = 0x8011,
            NO_FILE_SELECTED = 0x8012,
            INVALID_KEY_NUMBER = 0x8013,
            INVALID_KEY_LENGTH = 0x8014,
            SELECT_TAG_FAIL = 0x8101,
            READ_TAG_DATA_FAIL = 0x8102,
            WRITE_TAG_DATA_FAIL = 0x8103,
            ACTIVATE_TAG_TYPE_FAIL = 0x8104,
            DEACTIVATE_TAG_TYPE_FAIL = 0x8105,
            SET_TAG_BIT_RATE_FAIL = 0x8106,
            GET_TAG_INFO_FAIL = 0x8107,
            GET_LOCK_STATUS_FAIL = 0x8108,
            KILL_TAG_FAIL = 0x8109,
            REVIVE_TAG_FAIL = 0x810A,
            ERASE_TAG_FAIL = 0x810B,
            FORMAT_TAG_FAIL = 0x810C,
            DESELECT_TAG_FAIL = 0x810D,
            READ_TAG_CONFIG_FAIL = 0x8110,
            WRITE_TAG_CONFIG_FAIL = 0x8111,
            SELECT_TAG_INVENTORY_DONE = 0x810F,
            SELECT_TAG_LOOP_OFF = 0x81C1,
            AUTHENTICATE_TAG_FAIL = 0x8201,
            SEND_TAG_PASSWORD_FAIL = 0x8202,
            INIT_SECURE_MEMORY_FAIL = 0x8203,
            SETUP_SECURE_MEMORY_FAIL = 0x8204,
            GET_APPLICATION_IDS_FAIL = 0x8301,
            SELECT_APPLICATION_FAIL = 0x8302,
            CREATE_APPLICATION_FAIL = 0x8303,
            DELETE_APPLICATION_FAIL = 0x8304,
            GET_FILE_IDS_FAIL = 0x8401,
            SELECT_FILE_FAIL = 0x8402,
            CREATE_FILE_FAIL = 0x8403,
            GET_FILE_SETTINGS_FAIL = 0x8404,
            CHANGE_FILE_SETTINGS_FAIL = 0x8405,
            READ_FILE_FAIL = 0x8406,
            WRITE_FILE_FAIL = 0x8407,
            DELETE_FILE_FAIL = 0x8408,
            CLEAR_FILE_FAIL = 0x8409,
            CREDIT_VALUE_FILE_FAIL = 0x840A,
            DEBIT_VALUE_FILE_FAIL = 0x840B,
            LIMITED_CREDIT_VALUE_FILE_FAIL = 0x840C,
            GET_VALUE_FAIL = 0x840D,
            COMMIT_TRANSACTION_FAIL = 0x840E,
            ABORT_TRANSACTION_FAIL = 0x840F,
            READ_RECORDS_FAIL = 0x8410,
            WRITE_RECORD_FAIL = 0x8411,
            CHANGE_KEY_SETTINGS_FAIL = 0x8412,
            GET_KEY_SETTINGS_FAIL = 0x8413,
            GET_KEY_VERSION_FAIL = 0x8414,
            CHANGE_KEY_FAIL = 0x8415,
            ENABLE_EAS_FAIL = 0x8501,
            DISABLE_EAS_FAIL = 0x8502,
            SCAN_EAS_FAIL = 0x8503,
            WRITE_AFI_FAIL = 0x8504,
            READ_AFI_FAIL = 0x8505,
            WRITE_DSFID_FAIL = 0x8506,
            READ_DSFID_FAIL = 0x8507,
            SCAN_EAS_LOOP_EXIT = 0x85C3,
            STORE_KEY_FAIL = 0x8601,
            LOAD_KEY_FAIL = 0x8602,
            INTERFACE_SEND_FAIL = 0x8701,
            TRANSPORT_SEND_FAIL = 0x8702,
            INITIATE_PAYMENT_FAIL = 0x8801,
            COMPUTE_PAYMENT_FAIL = 0x8802,
            UNKOWN_ERROR = 0x9001,
            INVALID_COMMAND = 0x9002,
            INVALID_CRC = 0x9003,
            INVALID_MESSAGE_LENGTH = 0x9004,
            INVALID_ADDRESS = 0x9005,
            INVALID_FLAGS = 0x9006,
            INVALID_ASCII_CHAR = 0x9007,
            INVALID_NUMBER_OF_BLOCKS = 0x9008,
            INVALID_DATA_LEN = 0x9009,
            NO_ANTENNA_DETECTED = 0x900F,
            INVALID_ENCODING = 0x9010,
            INVALID_ARGUMENT = 0x9011,
            INVALID_SESSION = 0x9012,
            CMD_NOT_IMPLEMENTED = 0x9013,
            LOAD_DEFAULTS_FAIL = 0x9101,
            RESET_DEVICE_FAIL = 0x9102,
            BOOTLOAD_FAIL = 0x9103,
            READ_SYSTEM_PARAMETER_FAIL = 0x9201,
            WRITE_SYSTEM_PARAMETER_FAIL = 0x9202,
            STORE_DEFAULT_SYSTEM_PARAMETER_FAIL = 0x9301,
            RETRIEVE_DEFAULT_SYSTEM_PARAMETER_FAIL = 0x9302,
            AUTHENTICATE_READER_FAIL = 0x9401,
            ENABLE_DEBUG_FAIL = 0x9402,
            DISABLE_DEBUG_FAIL = 0x9403,
            GET_DEBUG_MESSAGES_FAIL = 0x9404,
            ENTER_PAYMENT_SCAN_MODE_FAIL = 0x9405
        }

        /// <summary>
        /// Wraps a response from the reader.  This should not be instantiated by itself, it must be
        /// created by STPv3Request.GetResponse <see cref="STPv3Request"/>
        /// </summary>
        public class STPv3Response
        {
            private byte[] m_responseBuffer;
            private STPv3Request m_request;

            internal STPv3Response(STPv3Request request, byte[] response)
            {
                if (response.Length < 7)
                    throw new ArgumentException("Response is not long enough to be a valid STPv3 response");

                if (request.Mode == STP.ProtocolMode.ASCII)
                    throw new NotImplementedException("ASCII mode is currently not supported");


                this.m_request = request;
                this.m_responseBuffer = response;
            }

            /// <summary>
            /// </summary>
            /// <returns>Returns a hexadecimal encoded string of the bytes that make up this response</returns>
            public override string ToString()
            {
                return BitConverter.ToString(this.Bytes).Replace("-", "");
            }

            #region Attributes
            /// <summary>
            /// Returns the actual bytes that make up this response
            /// </summary>
            public byte[] Bytes
            {
                get { return this.m_responseBuffer; }
            }

            /// <summary>
            /// Returns the response code for this response. <see cref="STPv3ResponseCode"/>
            /// </summary>
            public STPv3ResponseCode ResponseCode
            {
                get { return (STPv3ResponseCode)((this.m_responseBuffer[3] << 8) | (this.m_responseBuffer[4])); }
            }

            /// <summary>
            /// Returns the length of the response message
            /// </summary>
            public ushort MessageLength
            {
                get { return (ushort)((this.m_responseBuffer[1] << 8) | (this.m_responseBuffer[2])); }
            }

            /// <summary>
            /// Returns the tag type for this response if it has one. <see cref="Tags.TagType"/>
            /// </summary>
            public TagType TagType
            {
                get
                {
                    if (!Success)
                        throw new Exception("Cannot get TagType for a non-successful response");
                    // case where response will not contain tag type
                    if ((m_request.Command.Code < 0x8000) && (m_request.Command.Code > 0x1100))
                        return 0;

                    if ((this.m_request.RID[0] != 0xFF) ||
                         (this.m_request.RID[1] != 0xFF) ||
                         (this.m_request.RID[2] != 0xFF) ||
                         (this.m_request.RID[3] != 0xFF))
                    {
                        ushort offset = 9;
                        return (TagType)((this.m_responseBuffer[offset] << 8) | (this.m_responseBuffer[offset + 1]));
                    }
                    else if ((ResponseCode == STPv3ResponseCode.SELECT_TAG_PASS)
                        && (((ushort)this.m_request.Tag.Type & 0x000f) == 0))
                    {
                        ushort offset = 5;
                        return (TagType)((this.m_responseBuffer[offset] << 8) | (this.m_responseBuffer[offset + 1]));
                    }
                    else
                        return this.m_request.Tag.Type;
                }

            }

            /// <summary>
            /// Returns the data associated with this response if any
            /// </summary>
            public byte[] Data
            {
                get
                {
                    if (!Success)
                        throw new Exception("Cannot get data for a non-successful response");

                    ushort offset = 5;
                    ushort length, ix;
                    byte[] data;

                    if ((ResponseCode == STPv3ResponseCode.SELECT_TAG_PASS)
                        && (((ushort)this.m_request.Tag.Type & 0x000f) == 0))
                        offset = 7;

                    if((this.m_request.RID[0] != 0xFF) || 
                        (this.m_request.RID[1] != 0xFF) ||
                        (this.m_request.RID[2] != 0xFF) ||
                        (this.m_request.RID[3] != 0xFF))
                    {
                        offset += 4;
                        length = (ushort)((this.m_responseBuffer[offset] << 8) | this.m_responseBuffer[offset + 1]);
                        data = new byte[length];
                        offset += 2;
                       
                        Array.Copy(this.m_responseBuffer, offset, data, 0, length);
                       
                        return data;

                    }
                    else
                    {
                        length = (ushort)((this.m_responseBuffer[offset] << 8) | this.m_responseBuffer[offset + 1]);
                        data = new byte[length];
                        offset += 2;
                        Array.Copy(this.m_responseBuffer, offset, data, 0, length);

                        return data;
                    }

                    //return data;
                }
            }

            /// <summary>
            /// Returns the tag id associated with this response if any. 
            /// Variable length with maximum of 16 bytes.
            /// </summary>
            public byte[] TID
            {
                get
                {
                    if (!Success)
                        throw new Exception("Cannot get TID for a non-successful response");
                    // case where response will not contain TID
                    if ((m_request.Command.Code < 0x8000) && (m_request.Command.Code > 0x1100))
                        return null;

                    if (ResponseCode == STPv3ResponseCode.SELECT_TAG_PASS)
                        return Data;
                    else
                        return this.m_request.Tag.TID;
                }
            }

            /// <summary>
            /// Returns true if the response indicates the request was successful, false otherwise
            /// </summary>
            public bool Success
            {
                get { return ((ushort)ResponseCode & 0x8000) == 0; }
            }
            #endregion
        }

        /// <summary>
        /// Encapsulates a request to a STPv3 compliant reader.  This class
        /// exposes all of the various STPv3 fields as attributes.  Usage pattern
        /// consists of instantiating this class, setting the various attributes,
        /// calling Issue to transmit the command and then retrieving any
        /// responses via GetResponse
        /// </summary>
        public class STPv3Request
        {
            private Device m_device;
            private STP.ProtocolMode m_mode;
            private Tag m_tag;
            private STPv3Command m_command;
            private byte m_session;
            private byte[] m_data;
            private ushort m_address;
            private byte[] m_rid = { 0xFF, 0xFF, 0xFF, 0xFF };
            private int m_flags;
            private ushort m_blocks;
            private byte m_afi;

            /// <summary>
            /// 
            /// </summary>
            public STPv3Request()
            {
                this.m_mode = STP.ProtocolMode.BINARY;
                this.m_command = STPv3Commands.SELECT_TAG;
                this.m_tag = null;
                this.m_session = 0;
                this.m_address = 0x0000;
                //this.m_rid = { 0xFF, 0xFF, 0xFF, 0xFF };
                this.m_data = null;
                this.m_blocks = 0x0000;
                this.m_afi = 0x00;
                this.m_flags = 0x00000000;
            }

            /// <summary>
            /// </summary>
            /// <returns>Returns a hexadecimal encoded string of the bytes that make up this request</returns>
            public override string ToString()
            {
                return BitConverter.ToString(this.Bytes).Replace("-", "");
            }

            #region Functions

            /// <summary>
            /// Sends this request via the given Device. <see cref="Devices.Device"/>
            /// </summary>
            /// <param name="device">Device to send the request to</param>
            public void Issue(Device device)
            {
                if (!device.IsOpen)
                    throw new Exception("Device must be opened before a request can be issued");


                this.m_device = device;
                byte[] requestBytes = this.Bytes;
                this.m_device.Write(requestBytes, 0, requestBytes.Length);
                //this.m_device.Open();
                this.m_device.Flush();
                //this.m_device.Close();
            }

            /// <summary>
            /// Call this after Issue to retrieve any response(s) sent back by the reader
            /// </summary>
            /// <returns>STPv3Reponse <see cref="STPv3Response"/> object or null if there was no reponsed</returns>
            public STPv3Response GetResponse()
            {
                byte[] buffer;
                byte[] lbuf = new byte[2];
                int rb = -1;
                ushort length = 0;
                ushort total;
                int read = 0;

                //this.m_device.Open();
                if (this.m_mode == STP.ProtocolMode.BINARY)
                {
                    //this.m_device.ReadTimeout = this.m_command.Timeout;

                    try { rb = this.m_device.ReadByte(); } catch (Exception ex) { }

                    if (rb == -1)
                    {
                        this.m_device.Close();
                        return null;
                        //throw new Exception("Reader didn't reply with any data");
                    }
                    if (rb != STPv3.STX)
                    {
                        //this.m_device.Close();
                        return null;
                        /*if (rb == STPv3.NACK)
                            throw new Exception("Reader in bootload mode");
                        else
                            throw new Exception("Binary response does not begin with STX");*/
                    }

                    try { read = this.m_device.Read(lbuf, 0, 2); } catch (Exception ex) { }

                    if (read != 2)
                    {
                        //this.m_device.Close();
                        return null;
                        //throw new Exception("Reader didn't reply with length");
                    }

                    length = (ushort)((ushort)(lbuf[0] << 8) | (lbuf[1]));

                    if (length > STPv3.MAX_LENGTH)
                    {
                        //this.m_device.Close();
                        return null;
                        //throw new Exception("Response exceeds maximum allowed length");
                    }

                    buffer = new byte[length + 3];
                    total = 0;
                    while ((read = this.m_device.Read(buffer, total + 3, (length - total))) > 0)
                        total += (ushort)read;

                    buffer[0] = STPv3.STX;
                    buffer[1] = (byte)(length >> 8);
                    buffer[2] = (byte)(length & 0xff);

                    ushort crc = (ushort)((buffer[buffer.Length - 2] << 8) | buffer[buffer.Length - 1]);

                    if (crc != STP.Utils.crc16(0, buffer, 1, length))
                    {
                        //this.m_device.Close();
                        //return null;
                        //throw new STP.CRCException();
                    }

                    //this.m_device.Close();
                    return new STPv3Response(this, buffer);
                }

                //this.m_device.Close();
                return null;
            }
            #endregion
            #region Attributes

            /// <summary>
            /// For SelectTag requests, setting this to true indicates the reader should
            /// inventory all tags in the RF field
            /// </summary>
            public bool Inventory
            {
                get { return (this.m_flags & (ushort)STPv3Flags.INV) != 0; }
                set
                {
                    if (value)
                        this.m_flags |= (ushort)STPv3Flags.INV;
                    else
                        this.m_flags &= ~(ushort)STPv3Flags.INV;
                }
            }

            /// <summary>
            /// True indicates the reader should leave the radio on even after the command has completed
            /// This keeps any tags in the field powered and allows them to maintain state between requests.
            /// </summary>
            public bool RF
            {
                get { return (this.m_flags & (ushort)STPv3Flags.RF) != 0; }
                set
                {
                    if (value)
                        this.m_flags |= (ushort)STPv3Flags.RF;
                    else
                        this.m_flags &= ~(ushort)STPv3Flags.RF;
                }
            }

            /// <summary>
            /// For SelectTag requests, true indicates the reader should continually look
            /// for tags entering its field.  The reader may not necessarily return all of the
            /// tags in field.
            /// </summary>
            public bool Loop
            {
                get { return (this.m_flags & (ushort)STPv3Flags.LOOP) != 0; }
                set
                {
                    if (value)
                        this.m_flags |= (ushort)STPv3Flags.LOOP;
                    else
                        this.m_flags &= ~(ushort)STPv3Flags.LOOP;
                }
            }

            /// <summary>
            /// For Read and Write requests, true indicates the data should be verified
            /// by HMAC
            /// </summary>
            public bool HMAC
            {
                get { return (this.m_flags & (ushort)STPv3Flags.HMAC) != 0; }
                set
                {
                    if (value)
                        this.m_flags |= (ushort)STPv3Flags.HMAC;
                    else
                        this.m_flags &= ~(ushort)STPv3Flags.HMAC;
                }
            }

            /// <summary>
            /// For Read and Write requests, true indicates the data should be encrypted
            /// </summary>
            public bool Encryption
            {
                get { return (this.m_flags & (ushort)STPv3Flags.ENCRYPTION) != 0; }
                set
                {
                    if (value)
                        this.m_flags |= (ushort)STPv3Flags.ENCRYPTION;
                    else
                        this.m_flags &= ~(ushort)STPv3Flags.ENCRYPTION;
                }
            }
            /// <summary>
            /// For a Write request, true indicates the reader should lock the specified memory blocks
            /// </summary>
            public bool Lock
            {
                get { return (this.m_flags & (ushort)STPv3Flags.LOCK) != 0; }
                set
                {
                    if (value)
                        this.m_flags |= (ushort)STPv3Flags.LOCK;
                    else
                        this.m_flags &= ~(ushort)STPv3Flags.LOCK;
                }
            }

            /// <summary>
            ///  Specifies a particular reader module to which the request is directed. 
            /// </summary>
            public byte[] RID
            {
                get { return this.m_rid; }
                set { this.m_rid = value; }
            }

            /// <summary>
            /// Sets the specific session for this request
            /// </summary>
            public byte Session
            {
                get { return this.m_session; }
                set { this.m_session = value; }
            }

            /// <summary>
            /// Specifies an address to be used by for this request.
            /// It may be a specific address in the tag memory or an address that 
            /// gets translated into a specific tag memory address. 
            /// </summary>
            public ushort Address
            {
                get { return this.m_address; }
                set { this.m_address = value; }
            }

            /// <summary>
            /// Specifies the amount of data to be written by or read from the reader 
            /// module. (The location of the data is specified by the Address field.)
            /// Note that the number of blocks is not equal to the number of bytes of
            /// data. How many bytes exist in a block depends on the command, tag type,
            /// and address fields.
            /// </summary>
            public ushort Blocks
            {
                get { return this.m_blocks; }
                set { this.m_blocks = value; }
            }


            /// <summary>
            /// Sets the data for this request
            /// </summary>
#if !PocketPC
            [Browsable(false)]
#endif
            public byte[] Data
            {
                get { return this.m_data; }
                set
                {
                    this.m_data = value;

                    if (value == null)
                        this.m_flags &= ~(ushort)STPv3Flags.DATA;
                }
            }

            /// <summary>
            /// Specifies an Application Field Identifier (AFI) used to detect
            /// a tag in the field that belongs to a specific family of tags.
            /// Valid only for tag types that support AFI functionality. 
            /// </summary>
            public byte AFI
            {
                get { return this.m_afi; }
                set { this.m_afi = value; }
            }

            /// <summary>
            /// Sets the tag for this request.  Changing the tag after Issue is called but before
            /// GetResponse is called can result in the response being interpreted incorrectly.
            /// </summary>
#if !PocketPC
            [Browsable(false)]
#endif
            public Tag Tag
            {
                get { return this.m_tag; }
                set { this.m_tag = value; }
            }

            /// <summary>
            /// Retrieves the protocol mode (ASCII or BINARY) for the request.  Currently
            /// only BINARY is supported
            /// </summary>
#if !PocketPC
            [Browsable(false)]
#endif
            public STP.ProtocolMode Mode
            {
                get { return this.m_mode; }
                /*set { this.m_mode = value; }*/
            }

            /// <summary>
            /// Sets the command for this request <see cref="STPv3Command"/>.  The static STPv3Commands class
            /// contains a list of the supported commands.
            /// </summary>
#if !PocketPC
            [Browsable(false)]
#endif
            public STPv3Command Command
            {
                get { return this.m_command; }
                set { this.m_command = value; }
            }


#if !PocketPC
            [Browsable(false)]
#endif
            public byte[] Bytes
            {
                get
                {
                    if (this.m_mode == STP.ProtocolMode.BINARY)
                    {
                        //length + flags + command = 6
                        ushort length = 6;
                        ushort crc = 0;

                        System.Collections.ArrayList command = new System.Collections.ArrayList();

                        this.m_flags |= (ushort)STPv3Flags.CRC;

                        command.Add(STPv3.STX);
                        //This is place holding
                        command.Add(length);
                        //as well as this
                        command.Add(this.m_flags);
                        command.Add(this.m_command.Code);
                        
                        if ((this.m_rid[0] != 0xFF) ||
                         (this.m_rid[1] != 0xFF) ||
                         (this.m_rid[2] != 0xFF) ||
                         (this.m_rid[3] != 0xFF))
                        {
                            this.m_flags |= (ushort)STPv3Flags.RID;
                            command.Add(this.m_rid);
                            length += 4;
                        }

                        if (this.m_command.TagRequired)
                        {
                            if (this.m_tag == null)
                                throw new ArgumentNullException(String.Format("{0} requires a Tag to be set", this.m_command.Name));

                            command.Add((ushort)this.m_tag.Type);
                            length += 2;

                            if ((this.m_tag.TID != null) && (this.m_tag.TID.Length > 0))
                            {
                                this.m_flags |= (ushort)STPv3Flags.TID;
                                command.Add((byte)this.m_tag.TID.Length);
                                length += 1;
                                command.Add(this.m_tag.TID);
                                length += (ushort)this.m_tag.TID.Length;
                            }
                            else
                                this.m_flags &= ~(ushort)STPv3Flags.TID;

                        }

                        if (this.m_afi != 0x00)
                        {
                            this.m_flags |= (ushort)STPv3Flags.AFI;
                            command.Add(this.m_afi);
                            length += 1;
                        }

                        if (this.m_session != 0)
                        {
                            this.m_flags |= (ushort)STPv3Flags.SESSION;
                            command.Add(this.m_session);
                            length += 1;
                        }

                        if (this.m_command.AddressRequired)
                        {
                            if (this.m_blocks < 0)
                                throw new ArgumentNullException(String.Format("{0} requires blocks > 0", this.m_command.Name));

                            command.Add(this.m_address);
                            command.Add(this.m_blocks);
                            length += 4;
                        }

                        if (this.m_command.DataRequired)
                        {
                            if ((this.Data == null) || (this.Data.Length == 0))
                                throw new ArgumentNullException(String.Format("{0} requires data length > 0", this.m_command.Name));

                            this.m_flags |= (ushort)STPv3Flags.DATA;
                            command.Add((ushort)this.Data.Length);
                            length += 2;
                            command.Add(this.Data);
                            length += (ushort)this.Data.Length;
                        }


                        //crc
                        command.Add(crc);

                        byte[] commandBytes = new byte[length + 3];
                        command[1] = length;
                        command[2] = (ushort)(this.m_flags & 0xffff);
                        ushort ptr = 0;
                        foreach (object part in command)
                        {
                            if (part is byte)
                                commandBytes[ptr++] = (byte)part;
                            else if (part is ushort)
                            {
                                commandBytes[ptr++] = (byte)((ushort)part >> 8);
                                commandBytes[ptr++] = (byte)((ushort)part & 0xff);
                            }
                            else if (part is uint)
                            {
                                commandBytes[ptr++] = (byte)(((uint)part >> 24) & 0xff);
                                commandBytes[ptr++] = (byte)(((uint)part >> 16) & 0xff);
                                commandBytes[ptr++] = (byte)(((uint)part >> 8) & 0xff);
                                commandBytes[ptr++] = (byte)((uint)part & 0xff);
                            }
                            else if (part.GetType().IsArray)
                            {
                                Array ar = (Array)part;

                                for (ushort ix = 0; ix < ar.Length; ix++)
                                    commandBytes[ptr++] = (byte)ar.GetValue(ix);
                            }
                        }

                        crc = STP.Utils.crc16(crc, commandBytes, 1, length);
                        commandBytes[commandBytes.Length - 2] = (byte)(crc >> 8);
                        commandBytes[commandBytes.Length - 1] = (byte)(crc & 0xff);

                        return commandBytes;
                    }
                    else
                        throw new NotImplementedException("STPv3 ASCII mode is currently not implemented");
                }
            }

            [Flags]
            public enum STPv3CommandCodes : uint
            {
                QUERY_BOOTLDR_VER = 0x01,
                GET_SYSTEM_DEFAULTS = 0x02,
                PROGRAM_DEFAULTS = 0x03,
                WRITE_DATA = 0x04,
                SELECT_ENCRYPTION_SCHEME = 0x05,
                RESET_RESTART_DEVICE = 0x06,
                UPDATE_COMPLETE_RESET = 0x07,
                SETUP_BOOTLOADER = 0x08
            }
            public static bool UploadFirmware(Device device, string file)
            {
                int Size = 1024;
                UInt16 bytecount = 0;
                byte[] tempbuf = new byte[16];
                long minfilesize = 8;
                uint commandcode;
                uint sysParamStrLen;
                int nr; // 
                int retryCount;
                long filesize = 0;
                byte[] responsebuf = new byte[512];
                byte[] sysparambuf = new byte[100];
                byte[] sysparamstr = new byte[100];
                
                FileStream fs = null;
                
                if(!System.IO.File.Exists(file))
                   return false;
              
                fs = new FileStream(file, FileMode.OpenOrCreate);

                filesize = fs.Length;
                if (filesize < minfilesize)
                    return false;
                
                byte[] fsdata = new byte[Size];
                byte[] encrypteddatabuf = new byte[512];
                nr = fs.Read(tempbuf, 0, 2);
                byte[] length = new byte[2];

                length[1] = tempbuf[0];
                length[0] = tempbuf[1];

                UInt16 wordlength = 0x0000;
                wordlength = BitConverter.ToUInt16(length, 0);

                nr = fs.Read(tempbuf, 0, 2);
                byte[] structversion = new byte[2];
                structversion[1] = tempbuf[0];
                structversion[0] = tempbuf[1];
                bytecount += 2;

                nr = fs.Read(tempbuf, 0, 4);
                byte[] blversion = new byte[4];
                blversion[3] = tempbuf[0];
                blversion[2] = tempbuf[1];
                blversion[1] = tempbuf[2];
                blversion[0] = tempbuf[3];
                bytecount += 4;

                nr = fs.Read(tempbuf, 0, 1);
                byte[] encryptionscheme = new byte[1];
                encryptionscheme[0] = tempbuf[0];
                bytecount += 1;

                byte[] blinitstring = new byte[9];
                nr = fs.Read(blinitstring, 0, 1);
                
                if (blinitstring[0] > 8)
                    return false;
               
                if (blinitstring[0] != 0)
                {
                    minfilesize += blinitstring[0];
                    
                    if (filesize < minfilesize)
                        return false;
                    
                    nr = fs.Read(tempbuf, 0, (int)blinitstring[0]);
                    if (nr < 1)
                        return false;

                    Array.Copy(tempbuf, 0, blinitstring, 1, (int)blinitstring[0]);
                    
                    bytecount += (ushort)blinitstring[0];
                }
                bytecount += 1;

                //commandcode = QUERY_BOOTLDR_VER;
                commandcode = 0x01;

                if (sendBLCommand(device, commandcode, tempbuf, 0, responsebuf) == 0)
                    return false;
                
                fs.Seek(wordlength + 2, SeekOrigin.Begin);
                nr = fs.Read(tempbuf, 0, 4);

                byte[] datalen = new byte[4];
                datalen[3] = tempbuf[0];
                datalen[2] = tempbuf[1];
                datalen[1] = tempbuf[2];
                datalen[0] = tempbuf[3];

                UInt32 encrypteddatalen = 0;
                encrypteddatalen = BitConverter.ToUInt32(datalen, 0);
                minfilesize += encrypteddatalen;
                if (filesize < minfilesize)
                    return false;
               
                //commandcode = SELECT_ENCRYPTION_SCHEME;
                commandcode = 0x05;
                tempbuf[0] = encryptionscheme[0];
                if (sendBLCommand(device, commandcode, tempbuf, 1, responsebuf) == 0) 
                    return false;
                
                if (encrypteddatalen > 0)
                {
                    //commandcode = SETUP_BOOTLOADER;
                    commandcode = 0x08;
                    if (sendBLCommand(device, commandcode, blinitstring, 9, responsebuf) == 0)
                        return false;
                    
                    int i = 0;
                    retryCount = 0;
                    //commandcode = WRITE_DATA;
                    commandcode = 0x04;
                    while (i < encrypteddatalen) 
                    {
                        byte[] blocklen = new byte[2]; 
                        nr = fs.Read(encrypteddatabuf, 0, 2); 
                        
                        if (nr < 1)
                            return false;
                        
                        blocklen[1] = encrypteddatabuf[0];
                        blocklen[0] = encrypteddatabuf[1];
                        UInt16 blocklength = 0;
                        blocklength = BitConverter.ToUInt16(blocklen, 0); 
                        nr = fs.Read(encrypteddatabuf, 2, blocklength);
                        nr += 2;
                        //commandcode = WRITE_DATA;
                        commandcode = 0x04;
                        if(sendBLCommand(device, commandcode, encrypteddatabuf, (ushort)nr, responsebuf) == 0)
                            return false;
                        
                        i += nr;
                    }
                }
                retryCount = 0;
                //check if there are any default system parameters to be programmed
                if (wordlength > bytecount)
                {  
                    fs.Seek(bytecount + 2, SeekOrigin.Begin);
                    while( wordlength > bytecount )
                    {
                        nr = fs.Read(sysparamstr, 0, 1);
                        sysParamStrLen = BitConverter.ToUInt16(sysparamstr, 0);
                        if( nr < 1)
                            return false;
                       
                        bytecount += 1;
                        if(sysParamStrLen < 1 || sysParamStrLen > 100)
                            return false;
                        
                        nr = fs.Read(sysparambuf, 0, (int)sysParamStrLen);
                        if( nr < 1)
                            return false;
                        
                        bytecount += (ushort)sysParamStrLen;
 	                                               
                        //commandcode = PROGRAM_DEFAULTS;
                        commandcode = 0x03;
                        if(sendBLCommand(device, commandcode, sysparambuf, (ushort)sysParamStrLen, responsebuf) == 0)
                            return false;
                    }
                }
                commandcode = 0x07;
                sendBLCommand(device, commandcode, tempbuf, 0, responsebuf);
                System.Threading.Thread.Sleep(1000);
                return true;
        }
            
            


         

            private static UInt16 sendBLCommand(Device device, uint commandCode, byte[] commandData, UInt16 numBytes, byte[] responseBuffer)
            {
                UInt16 crc;
                UInt16 len;
                uint temp;

                byte[] crcarray = new byte[2];
                byte[] lenarray = new byte[2];
                int i = 0, num = 0, count = 0, totalCount = 0;
                UInt16 bytes = 0;
                byte[] temparray = new byte[1];
                
                crc = 0;
                len = (ushort)(numBytes + 3);
                lenarray[0] = (byte)len;
                uint shiftlen = (uint)(len >> 8);
                lenarray[1] = (byte)shiftlen;
                temparray[0] = lenarray[1];
                crc = crcBL16(temparray, 1, crc);
                temparray[0] = lenarray[0];
                crc = crcBL16(temparray, 1, crc);
                
                //calculate the crc over the command code and command data

                temparray[0] = (byte)commandCode;
                crc = crcBL16(temparray, 1, crc);
	            crc = crcBL16(commandData, numBytes, crc);

            sendcommand:
                //return crc; //
                //send command to reader
                //temp = lenarray[1];

                temparray[0] = lenarray[1];
                device.Write(temparray, 0, 1);
                temparray[0] = lenarray[0];
                device.Write(temparray, 0, 1);
                temparray[0] = (byte)commandCode;
                device.Write(temparray, 0, 1);
                device.Write(commandData, 0, numBytes);
                
                //need to get crcarray from crc
                crcarray[0] = (byte)crc;
                
                int shiftcrc = crc >> 8;
                //UInt16Converter shiftcrc = crc >> 8;
                crcarray[1] = (byte)shiftcrc;
                //temp = (byte)shiftcrc;
                temparray[0] = crcarray[1];
                device.Write(temparray, 0, 1);
                //temp = (byte)crc;
                temparray[0] = crcarray[0];
                device.Write(temparray, 0, 1);

                device.Flush();

                if (commandCode == 0x07)
                    return 0;
                
                count = 0;
                
                System.Threading.Thread.Sleep(300);
            readresponse:

                num = device.Read(responseBuffer, 0, 2); 
                
                if( num != 2 )
                {
                    count++;
                    if( count > 5 )
                        return (UInt16)num;

                    device.Flush();
                    goto readresponse;
                }


                bytes = (UInt16)((responseBuffer[0] << 8) | responseBuffer[1]);

                if (bytes > 512)
                    return 0;
                
                num = device.Read(responseBuffer, 2, bytes);//
                
                //verify crc

                if (verifyBLcrc(responseBuffer, (UInt16)bytes) == 0)
                {
                    totalCount++;
                    if (totalCount > 4)
                        return 0;
                    goto sendcommand;
                }

                //check command code

                if (responseBuffer[2] != commandCode)
                {
                    totalCount++;
                    if (totalCount > 10)
                        return 0;
                    goto sendcommand;
                }
                return (UInt16)(bytes + 2);
            }

            public static UInt16 crcBL16(byte[] dataP, UInt16 nBytes, UInt16 preset)
            {
                UInt16 i, j;
                uint mBits = 8;
                UInt16 crc_16 = preset;
                for( i=0; i<nBytes; i++ )
	                {
 	                    crc_16 ^= dataP[i];// changed from dataP[i]
 	               
 	                    for( j=0; j<mBits; j++ )
 	                    {
 	                        if((crc_16 & 0x0001) == 1) // ???
 	                        {
 	                                crc_16 >>= 1;
 	                                crc_16 ^= 0x8408;  
 	                        }
 	                        else
 	                        {
 	                                crc_16 >>= 1;
 	                        }
 	                    }
 	                }
 	
 	            return( crc_16 );
 	        }
            public static UInt16 verifyBLcrc(byte[] resp, UInt16 len)
            {
                UInt16 crc_check;
                UInt16 crc_shift;

                
                crc_check = crcBL16(resp, len, 0x0000);
                crc_shift = (UInt16)(crc_check >> 8);
                if ((resp[len] == crc_shift) && (resp[len+1] == (crc_check & 0x00FF)))
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }



            #endregion
        }
    }
}