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
using System.Diagnostics;

namespace SkyeTek
{
    public class Class1Gen2Tag
    {
        protected byte[] password;
        protected Tag tag;
        public int retries;

        public Class1Gen2Tag()
        {
            password = new byte[4] {0,0,0,0};        
            tag = new Tag();
            tag.Type = TagType.ISO_18000_6C_AUTO_DETECT;
            retries = 1;
        }

        public Class1Gen2Tag(Tag newTag, byte[] pwd)
        {
            password = pwd;
            tag = newTag;
            retries = 1;
        }

        /// <summary>
        /// This function will detect a tag in the field and will return the tag or 
        /// just return Null.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="newTag">Reference which will be updated it a tag is detected.</param>
        /// <returns>Tag if True else it returns Null</returns>
        public Tag DetectTag(STPv3Reader reader)
        {
            int numRetries = retries;

            if (numRetries == 0)
                retries = 1;

            for (int i = 0; i < numRetries; i++)
            {
                if (reader.SelectTag(ref tag) == true)
                    return tag;
            }

            return null;
        }

        /// <summary>
        /// This function performs Inventory on the type of Tag Passes in and returns with a list of 
        /// tags detected. If no tags detected or if another error encountered, then Null is returned.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>An ArrayList of Tags detected</returns>
        public ArrayList DetectTags(STPv3Reader reader)
        {
            ArrayList x;
            x = new ArrayList();
            int numRetries = retries;

            if (numRetries == 0)
                retries = 1;

            for (int i = 0; i < numRetries; i++)
            {
                try
                {
                    x = reader.InventoryTags(tag);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                if (x != null)
                {
                    return x;
                }

                //if ((x = reader.InventoryTags(tag)) != null)
                //{
                //    return x;
                //}
            }

            return null;
        }
           
        /// <summary>
        /// This function is used to change 4-byte Class1 Gen2 Access Password to be used for 
        /// some commands.
        /// </summary>
        /// <param name="pwd">Access Password to be used for the NXP Class1 Gen2 Tags</param>
        /// <returns>True or False. Its always True.</returns>
        public bool ChangePassword(byte[] pwd)
        {
            password = pwd;
            return true;
        }

        /// <summary>
        /// This function Writes the data to the EPC Memory Bank. It will calculate the PC 
        /// value to be written automatically.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="data"></param>
        /// <returns>Returns the actual number of bytes written.</returns>
        public int WriteEPC(STPv3Reader reader, byte[] data)
        {
            UInt16 address = 0x1001;
            UInt16 blocks = 0x0001;
            byte[] tempData = new byte[2];
            int i = 0;

            /* Check the Length of the EPC (Data Length) and calculate the PC bits */
            tempData[0] = (byte)((data.Length / 2) << 3);
            tempData[1] = 0;

            if (WriteTagMemory(reader, address, blocks, tempData) == false)
            {
                return i;
            }

            /* Write the EPC to the tag one block at a time */
            for (i = 0; i < data.Length; i += 2)
            {
                tempData[0] = data[i];
                tempData[1] = data[i + 1];

                /* Update address to write to the next block. */
                address++;

                if (WriteTagMemory(reader, address, blocks, tempData) == false)
                {
                    return i;
                }

            }

            return i;
        }

        /// <summary>
        /// This will write the data to the User Memory Bank. The function will exit with a Pass 
        /// if all the data was written. If not, then a failure message is returned.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="data"></param>
        /// <returns>Returns the actual number of bytes written.</returns>
        public int WriteUserData(STPv3Reader reader, byte[] data)
        {
            UInt16 address = 0x3000;    // Starting Address for the User Memory Bank
            UInt16 blocks = 0x0001;
            byte[] tempData = new byte[2];
            int i = 0;

            /* Keep writing data till we encounter a failure */
            for (i = 0; i < data.Length; i += 2)
            {
                tempData[0] = data[i];
                tempData[1] = data[i + 1];

                if (WriteTagMemory(reader, address, blocks, tempData) == false)
                {
                    return i;
                }

                /* Update address to write to the next block. */
                address++;
            }

            return i;
        }

        /// <summary>
        /// This will write the 4-byte Access Password to the Reserved Memory Bank.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="data"></param>
        /// <returns>Returns True if Password written correctly else returns False.</returns>
        public bool WriteAccessPassword(STPv3Reader reader, byte[] data)
        {
            UInt16 address = 0x0002;    // Starting Address for the Access Password
            UInt16 blocks = 0x0002;

            if (WriteTagMemory(reader, address, blocks, data) == true)
                return true;

            return false;
        }

        /// <summary>
        /// This will write the 4-byte Kill Password to the Reserved Memory Bank.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="data"></param>
        /// <returns>Returns True if Password written correctly else returns False.</returns>
        public bool WriteKillPassword(STPv3Reader reader, byte[] data)
        {
            UInt16 address = 0x0000;    // Starting Address for the Reserved Memory Bank and Kill Password
            UInt16 blocks = 0x0002;

            if (WriteTagMemory(reader, address, blocks, data) == true)
                return true;

            return false;
        }
        
        /// <summary>
        /// This sends the EPC Class1 Gen2 Lock Value to the tag. The lock value would be the 
        /// different memory banks lock protection values ORed together into a single 32-bit value.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="lockVal"></param>
        /// <returns></returns>
        public bool LockTag(STPv3Reader reader, UInt32 lockVal)
        {
            byte[] data = new byte[4];
            UInt16 address = 0x0000;    // Always set to 0
            UInt16 blocks = 0x0000;     // Always set to 0

            int numRetries = retries;

            if (numRetries == 0)
                retries = 1;

            data[0] = (byte)((lockVal & 0xFF000000) >> 24);
            data[1] = (byte)((lockVal & 0x00FF0000) >> 16);
            data[2] = (byte)((lockVal & 0x0000FF00) >> 8);
            data[3] = (byte)(lockVal & 0x000000FF);

            if (this.SendTagPassword(reader) == true)
            {
                for (int i = 0; i < numRetries; i++)
                {
                    if (reader.LockTagData(tag, data, address, blocks) == true)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This function is used to Write Any Data to Any of the Meory banks. The calling 
        /// function will have to select the correct starting address block information 
        /// and data to be written.This can also be used for writing the Lock Values to 
        /// the EPC Tag.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="address"></param>
        /// <param name="blocks"></param>
        /// <param name="data"></param>
        /// <returns>Returns True if data written correctly else returns False.</returns>
        public bool WriteTagMemory(STPv3Reader reader, UInt16 address, UInt16 blocks, byte[] data)
        {
            int numRetries = retries;

            if (numRetries == 0)
                retries = 1;

            for (int i = 0; i < numRetries; i++)
            {
                if (reader.WriteTagData(tag, data, address, blocks) == true)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// This function will read all the contents of the EPC Memory Bank. The function 
        /// returns when no more data can be read.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public byte[] ReadEPC(STPv3Reader reader)
        {
            UInt16 address = 0x1000;   // Starting Address for the EPC Memory Bank
            UInt16 blocks = 0x0001;     
            byte[] data = new byte[1024];
            byte[] tempData = new byte[2];
            int i = 0, j = 0;

            while(i < 1024)
            {
                if ((tempData = ReadTagMemory(reader, address, blocks)) == null)
                {
                    break;
                }

                address++;
                i += 2;
                data[j++] = tempData[0];
                data[j++] = tempData[1];
            }

            if(i==0)
                return null;

            /* Send the byte buffer back with the data read */
            byte[] newDataBuf = new byte[j];

            System.Buffer.BlockCopy(data, 0, newDataBuf, 0, j);

            return newDataBuf;
        }

        /// <summary>
        /// This function will read all the contents of the User Memory Bank. The function 
        /// returns when no more data can be read.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public byte[] ReadUserData(STPv3Reader reader)
        {
            UInt16 address = 0x3000;   // Starting Address for the User Memory Bank
            UInt16 blocks = 0x0001;
            byte[] data = new byte[1024];
            byte[] tempData = new byte[2];
            int i = 0, j = 0;

            while (i < 1024)
            {
                if ((tempData = ReadTagMemory(reader, address, blocks)) == null)
                {
                    break;
                }

                address++;
                i += 2;
                data[j++] = tempData[0];
                data[j++] = tempData[1];
            }

            if (i == 0)
                return null;

            /* Send the byte buffer back with the data read */
            byte[] newDataBuf = new byte[j];

            System.Buffer.BlockCopy(data, 0, newDataBuf, 0, j);

            return newDataBuf;
        }

        /// <summary>
        /// This function will read the Access Password from the Reserved Memory Bank of the
        /// EPC Tag.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public byte[] ReadAccessPassword(STPv3Reader reader)
        {
            UInt16 address = 0x0002;   // Starting Address for the Access Password
            UInt16 blocks = 0x0002;

            return (ReadTagMemory(reader, address, blocks));
        }

        /// <summary>
        /// This function will read the Kill Password from the Reserved Memory Bank of the
        /// EPC Tag
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public byte[] ReadKillPassword(STPv3Reader reader)
        {
            UInt16 address = 0x0000;   // Starting Address for the Kill Password
            UInt16 blocks = 0x0002;

            return (ReadTagMemory(reader, address, blocks));
        }

        /// <summary>
        /// This function is used to read data from Any Memory bank, address and any number 
        /// of blocks.
        /// The function will return a NULL if no data was read, else it will return the 
        /// data read and set the length to the data read. (?)
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="address"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public byte[] ReadTagMemory(STPv3Reader reader, UInt16 address, UInt16 blocks)    
        {
            byte[] data;
            int numRetries = retries;

            if (numRetries == 0)
                retries = 1;

            for (int i = 0; i < numRetries; i++)
            {
                if ((data = reader.ReadTagData(tag, address, blocks)) != null)
                    return data;
            }

            return null;
        }

        /// <summary>
        /// This sends the Access Password to the tag to put it in Secure Mode.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool SendTagPassword(STPv3Reader reader)
        {
            byte[] data = new byte[4];
            int numRetries = retries;
            
            if (numRetries == 0)
                numRetries = 1;

            /* Copy the password and swap the lower and higher words */
            data[0] = password[2];
            data[1] = password[3]; 
            data[2] = password[0]; 
            data[3] = password[1];

            for (int i = 0; i < numRetries; i++)
            {
                if (reader.SendTagPassword(tag, data) == true)
                    return true;
            }

            return false;
        }

    }
}