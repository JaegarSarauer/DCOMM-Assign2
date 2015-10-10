using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

using SkyeTek.Tags;
using SkyeTek.Devices;
using SkyeTek.STPv3;
using SkyeTek.Readers;
using System.Threading;
using System.Collections;

namespace SkyeTek
{
    /// <summary>
    /// NXP Tag Class that inherits from the Class1 Gen2 Tag Class
    /// </summary>
    public class NXPTag : Class1Gen2Tag
    {
        /// <summary>
        /// Constructor for the NXP Tag Class. 
        /// </summary>
        /// <param name="newTag">Tag Type that gets passed in. Should be of type NXP G2XL or G2XM</param>
        /// <param name="pwd">Access Password to be used for the NXP Class1 Gen2 Tags</param>
        public NXPTag(Tag newTag, byte[] pwd)
        {
            tag = newTag;
            password = pwd;
            retries = 1;
        }

        public NXPTag(Tag newTag)
        {
            tag = newTag;
            password = new byte[4] { 0, 0, 0, 0 };        
            retries = 1;
        }

        public NXPTag(TagType type)
        {
            tag = new Tag();
            tag.Type = type;
            password = new byte[4] { 0, 0, 0, 0 };
            retries = 1;
        }

        /*
        /// <summary>
        /// This function is used to change 4-byte Class1 Gen2 Access Password to be used for 
        /// some commands.
        /// </summary>
        /// <param name="pwd">Access Password to be used for the NXP Class1 Gen2 Tags</param>
        /// <returns></returns>
        public bool ChangePassword(byte[] pwd)
        {
            password = pwd;
            return true;
        }*/

        /// <summary>
        /// This function is used to enable the EAS functionality for the NXP G2XM and G2XM tags.
        /// </summary>
        /// <param name="reader">SkyeTek UHF Reader used to send commands to the tag</param>
        /// <returns>Returns True if the operation passes. Else it returns False</returns>
        public bool EnableEAS(STPv3Reader reader)
        {
            int numRetries = retries;

            if (numRetries == 0)
                retries = 1;

            /* Send the Access Password to the tag first */
            if (this.SendTagPassword(reader) == true)
            {
                for (int i = 0; i < numRetries; i++)
                {
                    /* If Send Tag Password passes, then send the Enable EAS Command to the tag */
                    if (reader.enableEAS(tag) == true)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This function is used to disable the EAS functionality for the NXP G2XM and G2XM tags.
        /// </summary>
        /// <param name="reader">SkyeTek UHF Reader used to send commands to the tag</param>
        /// <returns>Returns True if the operation passes. Else it returns False</returns>
        public bool DisableEAS(STPv3Reader reader)
        {
            int numRetries = retries;

            if (numRetries == 0)
                retries = 1;

            /* Send the Access Password to the tag first */
            if (this.SendTagPassword(reader) == true)
            {
                for (int i = 0; i < numRetries; i++)
                {
                    /* If Send Tag Password passes, then send the Disable EAS Command to the tag */
                    if (reader.disableEAS(tag) == true)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This function scans for EAS Alarms that would be sent out by the NXP G2XL and G2XM
        /// tags that have the EAS functionality Enabled. 
        /// </summary>
        /// <param name="reader">SkyeTek UHF Reader used to send commands to the tag</param>
        /// <returns>Returns True if EAS Alrm is detected. Else it returns False</returns>
        public bool ScanEAS(STPv3Reader reader)
        {
            int numRetries = retries;

            if (numRetries == 0)
                retries = 1;

            for (int i = 0; i < numRetries; i++)
            {
                /* Send the Scan EAS Command to the reader */
                if (reader.scanEAS(tag) == true)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the Read Protection on the NXP G2XL and G2XM tags. This puts the tag in a mode
        /// so that the tag will respond with all 0s in place of its actual EPC.
        /// </summary>
        /// <param name="reader">SkyeTek UHF Reader used to send commands to the tag</param>
        /// <returns>Returns True if the operation passes. Else it returns False</returns>
        public bool SetReadProtection(STPv3Reader reader)
        {
            byte[] dataBuf = new byte[5];
            int numRetries = retries;
            byte[] data = new byte[4];

            if (numRetries == 0)
                retries = 1;

            data[0] = password[2];
            data[1] = password[3];
            data[2] = password[0];
            data[3] = password[1];

            /* Data - Config Command (1-byte) + Access Password (4-bytes) */
            dataBuf[0] = 0x01; // Command Code for Setting Read Protection
            System.Buffer.BlockCopy(data, 0, dataBuf, 1, data.Length);

            for (int i = 0; i < numRetries; i++)
            {
                /* Send the Write Tag Config Command to the Reader */
                if (reader.WriteTagConfig(tag, 0, 1, dataBuf) == true)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Resets the Read Protection that was set on an NXP G2XL or G2XM tag. 
        /// </summary>
        /// <param name="reader">SkyeTek UHF Reader used to send commands to the tag</param>
        /// <returns>Returns True if the operation passes. Else it returns False</returns>
        public bool ResetReadProtection(STPv3Reader reader)
        {
            byte[] dataBuf = new byte[5];
            int numRetries = retries;
            byte[] data = new byte[4];

            if (numRetries == 0)
                retries = 1;

            data[0] = password[2];
            data[1] = password[3];
            data[2] = password[0];
            data[3] = password[1];

            /* Data - Config Command (1-byte) + Access Password (4-bytes) */
            dataBuf[0] = 0x02; // Command Code for Resetting Read Protection
            System.Buffer.BlockCopy(data, 0, dataBuf, 1, data.Length);

            for (int i = 0; i < numRetries; i++)
            {
                /* Send the Write Tag Config Command to the Reader */
                if (reader.WriteTagConfig(tag, 0, 1, dataBuf) == true)
                    return true;
            }

            return false;
        }
    }
}