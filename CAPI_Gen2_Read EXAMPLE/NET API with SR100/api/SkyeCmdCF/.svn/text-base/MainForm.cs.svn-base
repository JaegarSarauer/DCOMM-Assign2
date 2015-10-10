using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

using SkyeTek.Devices;
using SkyeTek.Tags;
using SkyeTek.STPv3;

namespace SkyeCmdCF
{
    public partial class MainForm : Form
    {
        private STPv3Request m_gRequest;
        private Device m_gDevice;
        private System.Threading.Thread m_responseWorkThread;

        public MainForm()
        {
            InitializeComponent();

            this.m_gRequest = new STPv3Request();
            this.m_gRequest.Tag = new Tag();
            this.m_gDevice = new SkyeTek.Devices.SerialDevice();

            foreach (string sp in System.IO.Ports.SerialPort.GetPortNames())
                this.deviceBox.Items.Add(sp);

            if (this.deviceBox.Items.Count > 0)
                this.deviceBox.SelectedIndex = 0;

            foreach (STPv3Command command in STPv3Commands.GetCommands())
                this.commandBox.Items.Add(command);

            if (this.commandBox.Items.Count > 0)
                this.commandBox.SelectedIndex = 0;

            /*foreach (string tt in Enum.GetNames(typeof(SkyeTek.Tags.TagType)))
            {
                this.tagBox.Items.Add(tt);
            }

            if (this.tagBox.Items.Count > 0)
                this.tagBox.SelectedIndex = 0;*/
        }

        private void responseThread()
        {
            STPv3Response response;

            try
            {
                while (((response = this.m_gRequest.GetResponse()) != null))
                {
                    this.responsesBox.Invoke(new appendResponseCallback(this.appendResponse), new object[] { response });
                }
            }
            catch (Exception ex)
            {
                System.Console.Out.WriteLine(ex.ToString());
            }
        }

        private void sendRequestButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.m_gRequest.Issue(this.m_gDevice);
            }
            catch (Exception ex)
            {
                System.Console.Out.WriteLine(ex.ToString());
            }

            if ((this.m_responseWorkThread != null) && !this.m_responseWorkThread.Join(300))
            {
                System.Console.Out.WriteLine("responseWorkThread is still alive");
                return;
            }

            this.m_responseWorkThread = new System.Threading.Thread(this.responseThread);
            this.m_responseWorkThread.IsBackground = true;
            this.m_responseWorkThread.Start();
        }

        private void updateRequestBox()
        {
            try
            {
                this.requestBox.Text = this.m_gRequest.ToString();
            }
            catch (Exception ex)
            {
            }
        }

        private delegate void appendResponseCallback(STPv3Response response);

        private void appendResponse(STPv3Response response)
        {
            this.responsesBox.Items.Add(response);
        }

        private void commandBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.commandBox.SelectedItem != null)
            {
                this.m_gRequest.Command = (STPv3Command)this.commandBox.SelectedItem;
                this.updateRequestBox();
            }
        }

        private void deviceBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.m_gDevice.IsOpen)
                this.m_gDevice.Close();

            if (this.deviceBox.Text == "")
                return;

            this.m_gDevice.Address = this.deviceBox.Text;

            this.m_gDevice.Open();
        }

        /*private void requestProperties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.updateRequestBox();
        }*/

        /*private void tagBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tagBox.Text == "")
                return;

            this.m_gRequest.Tag.Type = (SkyeTek.Tags.TagType)Enum.Parse(typeof(SkyeTek.Tags.TagType), this.tagBox.Text);
            this.updateRequestBox();
        }

        private void tidBox_TextChanged(object sender, EventArgs e)
        {
            byte[] tid;

            if (this.tidBox.Text == "")
            {
                this.m_gRequest.Tag.TID = null;
            }
            else
            {

                if ((this.tidBox.Text.Length % 2) != 0)
                    return;

                tid = new byte[(this.tidBox.Text.Length / 2)];

                for (int ix = 0; ix < tid.Length; ix++)
                    tid[ix] = (byte)System.Convert.ToByte(this.tidBox.Text.Substring(ix * 2, 2), 16);

                this.m_gRequest.Tag.TID = tid;
            }

            this.updateRequestBox();
        }

        private void dataBox_TextChanged(object sender, EventArgs e)
        {
            byte[] data;

            if (this.dataBox.Text == "")
            {
                this.m_gRequest.Data = null;
            }
            else
            {
                if ((this.dataBox.Text.Length % 2) != 0)
                    return;

                data = new byte[(this.dataBox.Text.Length / 2)];

                for (int ix = 0; ix < data.Length; ix++)
                    data[ix] = (byte)System.Convert.ToByte(this.dataBox.Text.Substring(ix * 2, 2), 16);

                this.m_gRequest.Data = data;
            }

            this.updateRequestBox();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            this.responsesBox.Items.Clear();
        }

        private void copyTagItem_Click(object sender, EventArgs e)
        {
            STPv3Response response;

            if (this.responsesBox.SelectedItem != null)
            {
                response = (STPv3Response)this.responsesBox.SelectedItem;
                this.tidBox.Text = String.Join("", Array.ConvertAll<byte, string>(response.TID, delegate(byte value) { return String.Format("{0:X2}", value); }));
                this.tagBox.SelectedItem = Enum.GetName(typeof(SkyeTek.Tags.TagType), response.TagType);
            }
        }

        private void responsesContextMenu_Opening(object sender, CancelEventArgs e)
        {
            STPv3Response response;

            if (this.responsesBox.SelectedItem == null)
            {
                e.Cancel = true;
                return;
            }

            response = (STPv3Response)this.responsesBox.SelectedItem;

            if (response.ResponseCode == STPv3ResponseCode.SELECT_TAG_PASS)
                this.copyTagItem.Enabled = true;
            else
                this.copyTagItem.Enabled = false;
        }*/
    }
}