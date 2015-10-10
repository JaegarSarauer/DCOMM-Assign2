using System;
using System.Collections.Generic;
using System.Text;

using SkyeTek.Tags;
using SkyeTek.Devices;
using SkyeTek.STPv3;
using SkyeTek.Readers;
using System.Threading;
using System.Collections;
using System.Diagnostics;

public enum MUXType
{
    FOUR_PORT_HF = 0x01,
    FOUR_PORT_UHF = 0x02,
    TWELVE_PORT_HF = 0x03,
    TWELVE_PORT_UHF = 0x04,
    EIGHT_PORT_HF = 0x05,
    EIGHT_PORT_UHF = 0x06,
    SIXTEEN_PORT_HF = 0x07,
    SIXTEEN_PORT_UHF = 0x08
}

namespace SkyeTek
{
    public class SkyePlusMultiplexer
    {
        private MUXType multiplexerType;
        private int currentPort, maxPort, portIndex;

        public SkyePlusMultiplexer(MUXType muxType, int muxNum)
        {
            multiplexerType = muxType;
            maxPort = MultiplexerMaxPort[multiplexerType];
        }

        /// <summary>
        /// 
        /// </summary>
        public static IDictionary<MUXType, int> MultiplexerMaxPort
        {
            get
            {
                IDictionary<MUXType, int> muxMaxPort = new Dictionary<MUXType, int>();
                muxMaxPort.Add(MUXType.FOUR_PORT_HF, 3);
                muxMaxPort.Add(MUXType.FOUR_PORT_UHF, 3);
                muxMaxPort.Add(MUXType.EIGHT_PORT_HF, 7);
                muxMaxPort.Add(MUXType.EIGHT_PORT_UHF, 7);
                muxMaxPort.Add(MUXType.TWELVE_PORT_HF, 11);
                muxMaxPort.Add(MUXType.TWELVE_PORT_UHF, 11);
                muxMaxPort.Add(MUXType.SIXTEEN_PORT_HF, 15);
                muxMaxPort.Add(MUXType.SIXTEEN_PORT_UHF, 15);
                return muxMaxPort;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MUXType DetectMultiplexer(STPv3Reader reader)
        {
            MUXType muxType;

            byte[] data;

            data = reader.ReadSystemParameter((ushort)SYS_PARAMS.SYS_MUX_CONTROL, 1);

            muxType = MUXType.FOUR_PORT_HF + data[0] - 1;

            return muxType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool EnableMultiplexer(STPv3Reader reader)
        {
            /* This requires that the Mux System Parameter be set to 02 in Non-Volatile Memory  */
            byte[] data = new byte[1] {0x02};
            if (reader.StoreDefaultParameter(data, (ushort)SYS_PARAMS.SYS_MUX_CONTROL, 1) == true)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool DisableMultiplexer(STPv3Reader reader)
        {
            /* This requires that the Mux System Parameter be set to 00 in Non-Volatile Memory  */
            byte[] data = new byte[1] { 0x00 };
            if (reader.StoreDefaultParameter(data, (ushort)SYS_PARAMS.SYS_MUX_CONTROL, 1) == true)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="portVal"></param>
        /// <returns></returns>
        public bool SetMultiplexerPort(STPv3Reader reader, byte portVal)
        {
            byte[] data = new byte[1];
            byte[] muxPorts = MuxPortList(multiplexerType);

            if (multiplexerType.Equals(MUXType.FOUR_PORT_HF) || multiplexerType.Equals(MUXType.FOUR_PORT_UHF))
            {
                if ((portVal != 0) && (portVal != 2) && (portVal != 5) && (portVal != 7))
                {   
                    return false;
                }

                if (portVal == 0)
                    portIndex = 0;

                if (portVal == 2)
                    portIndex = 1;
                
                if (portVal == 5)
                    portIndex = 2;
                
                if (portVal == 7)
                    portIndex = 3;
            }
            else
            {
                if (portVal > maxPort)
                    return false;
            }

            data[0] = portVal;
            currentPort = portVal;

            if (reader.WriteSystemParameter(data, (ushort)SYS_PARAMS.SYS_MUX_CONTROL, 1) == true)
                return true;
            else
                return false;
        }

        public byte GetMultiplexerPort(STPv3Reader reader)
        {
            return (byte)currentPort;
        }

        /// <summary>
        /// This function checks the current port and switches to the next port. If the max port is reached,
        /// then 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool IncrementMultiplexerPort(STPv3Reader reader)
        {
            byte[] data = new byte[1];
            byte[] muxPorts = MuxPortList(multiplexerType);

            currentPort++;

            if (portIndex >= maxPort)
                portIndex = 0;

            data[0] = muxPorts[portIndex];

            if (reader.WriteSystemParameter(data, (ushort)SYS_PARAMS.SYS_MUX_CONTROL, 1) == true)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="muxType"></param>
        /// <returns></returns>
        private byte[] MuxPortList(MUXType muxType)
        {
            if (muxType.Equals(MUXType.FOUR_PORT_HF) || muxType.Equals(MUXType.FOUR_PORT_UHF))
            {
                return new byte[4] {0, 2, 5, 7};
            }

            if (muxType.Equals(MUXType.EIGHT_PORT_HF) || muxType.Equals(MUXType.EIGHT_PORT_UHF))
            {
                return new byte[8] { 0, 1, 2, 3, 4, 5, 6, 7 };
            }

            if (muxType.Equals(MUXType.TWELVE_PORT_HF) || muxType.Equals(MUXType.TWELVE_PORT_UHF))
            {
                return new byte[12] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            }

            if (muxType.Equals(MUXType.SIXTEEN_PORT_HF) || muxType.Equals(MUXType.SIXTEEN_PORT_UHF))
            {
                return new byte[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            }

            return null;
        }
    }
}