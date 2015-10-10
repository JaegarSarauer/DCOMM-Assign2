namespace SkyeTek.SkyeCmd
{
    partial class MainForm  
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.deviceBox = new System.Windows.Forms.ComboBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.requestBox = new System.Windows.Forms.TextBox();
            this.requestLabel = new System.Windows.Forms.Label();
            this.sendRequestButton = new System.Windows.Forms.Button();
            this.responseLabel = new System.Windows.Forms.Label();
            this.commandBox = new System.Windows.Forms.ComboBox();
            this.commandLabel = new System.Windows.Forms.Label();
            this.requestProperties = new System.Windows.Forms.PropertyGrid();
            this.tagBox = new System.Windows.Forms.ComboBox();
            this.tagLabel = new System.Windows.Forms.Label();
            this.tidBox = new System.Windows.Forms.TextBox();
            this.tidLabel = new System.Windows.Forms.Label();
            this.dataLabel = new System.Windows.Forms.Label();
            this.dataBox = new System.Windows.Forms.TextBox();
            this.responsesBox = new System.Windows.Forms.ListBox();
            this.responsesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyTagItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearButton = new System.Windows.Forms.Button();
            this.responsesContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // deviceBox
            // 
            this.deviceBox.FormattingEnabled = true;
            this.deviceBox.Location = new System.Drawing.Point(70, 12);
            this.deviceBox.Name = "deviceBox";
            this.deviceBox.Size = new System.Drawing.Size(99, 21);
            this.deviceBox.Sorted = true;
            this.deviceBox.TabIndex = 0;
            this.deviceBox.SelectedIndexChanged += new System.EventHandler(this.deviceBox_SelectedIndexChanged);
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(12, 15);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(41, 13);
            this.portLabel.TabIndex = 1;
            this.portLabel.Text = "Device";
            // 
            // requestBox
            // 
            this.requestBox.Location = new System.Drawing.Point(70, 270);
            this.requestBox.Name = "requestBox";
            this.requestBox.Size = new System.Drawing.Size(436, 20);
            this.requestBox.TabIndex = 2;
            // 
            // requestLabel
            // 
            this.requestLabel.AutoSize = true;
            this.requestLabel.Location = new System.Drawing.Point(5, 273);
            this.requestLabel.Name = "requestLabel";
            this.requestLabel.Size = new System.Drawing.Size(47, 13);
            this.requestLabel.TabIndex = 3;
            this.requestLabel.Text = "Request";
            // 
            // sendRequestButton
            // 
            this.sendRequestButton.Location = new System.Drawing.Point(197, 478);
            this.sendRequestButton.Name = "sendRequestButton";
            this.sendRequestButton.Size = new System.Drawing.Size(83, 23);
            this.sendRequestButton.TabIndex = 4;
            this.sendRequestButton.Text = "Send Request";
            this.sendRequestButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.sendRequestButton.UseVisualStyleBackColor = true;
            this.sendRequestButton.Click += new System.EventHandler(this.sendRequestButton_Click);
            // 
            // responseLabel
            // 
            this.responseLabel.AutoSize = true;
            this.responseLabel.Location = new System.Drawing.Point(5, 312);
            this.responseLabel.Name = "responseLabel";
            this.responseLabel.Size = new System.Drawing.Size(55, 13);
            this.responseLabel.TabIndex = 6;
            this.responseLabel.Text = "Response";
            // 
            // commandBox
            // 
            this.commandBox.FormattingEnabled = true;
            this.commandBox.Location = new System.Drawing.Point(286, 12);
            this.commandBox.Name = "commandBox";
            this.commandBox.Size = new System.Drawing.Size(220, 21);
            this.commandBox.TabIndex = 7;
            this.commandBox.SelectedIndexChanged += new System.EventHandler(this.commandBox_SelectedIndexChanged);
            // 
            // commandLabel
            // 
            this.commandLabel.AutoSize = true;
            this.commandLabel.Location = new System.Drawing.Point(226, 15);
            this.commandLabel.Name = "commandLabel";
            this.commandLabel.Size = new System.Drawing.Size(54, 13);
            this.commandLabel.TabIndex = 8;
            this.commandLabel.Text = "Command";
            // 
            // requestProperties
            // 
            this.requestProperties.Location = new System.Drawing.Point(229, 42);
            this.requestProperties.Name = "requestProperties";
            this.requestProperties.Size = new System.Drawing.Size(277, 209);
            this.requestProperties.TabIndex = 9;
            this.requestProperties.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.requestProperties_PropertyValueChanged);
            // 
            // tagBox
            // 
            this.tagBox.FormattingEnabled = true;
            this.tagBox.Location = new System.Drawing.Point(44, 47);
            this.tagBox.Name = "tagBox";
            this.tagBox.Size = new System.Drawing.Size(179, 21);
            this.tagBox.TabIndex = 10;
            this.tagBox.SelectedIndexChanged += new System.EventHandler(this.tagBox_SelectedIndexChanged);
            // 
            // tagLabel
            // 
            this.tagLabel.AutoSize = true;
            this.tagLabel.Location = new System.Drawing.Point(12, 50);
            this.tagLabel.Name = "tagLabel";
            this.tagLabel.Size = new System.Drawing.Size(26, 13);
            this.tagLabel.TabIndex = 11;
            this.tagLabel.Text = "Tag";
            // 
            // tidBox
            // 
            this.tidBox.Location = new System.Drawing.Point(44, 89);
            this.tidBox.Name = "tidBox";
            this.tidBox.Size = new System.Drawing.Size(179, 20);
            this.tidBox.TabIndex = 12;
            this.tidBox.TextChanged += new System.EventHandler(this.tidBox_TextChanged);
            // 
            // tidLabel
            // 
            this.tidLabel.AutoSize = true;
            this.tidLabel.Location = new System.Drawing.Point(12, 92);
            this.tidLabel.Name = "tidLabel";
            this.tidLabel.Size = new System.Drawing.Size(25, 13);
            this.tidLabel.TabIndex = 13;
            this.tidLabel.Text = "TID";
            // 
            // dataLabel
            // 
            this.dataLabel.AutoSize = true;
            this.dataLabel.Location = new System.Drawing.Point(12, 163);
            this.dataLabel.Name = "dataLabel";
            this.dataLabel.Size = new System.Drawing.Size(30, 13);
            this.dataLabel.TabIndex = 15;
            this.dataLabel.Text = "Data";
            // 
            // dataBox
            // 
            this.dataBox.Location = new System.Drawing.Point(44, 160);
            this.dataBox.Name = "dataBox";
            this.dataBox.Size = new System.Drawing.Size(179, 20);
            this.dataBox.TabIndex = 14;
            this.dataBox.TextChanged += new System.EventHandler(this.dataBox_TextChanged);
            // 
            // responsesBox
            // 
            this.responsesBox.ContextMenuStrip = this.responsesContextMenu;
            this.responsesBox.FormattingEnabled = true;
            this.responsesBox.Location = new System.Drawing.Point(70, 312);
            this.responsesBox.Name = "responsesBox";
            this.responsesBox.Size = new System.Drawing.Size(436, 160);
            this.responsesBox.TabIndex = 16;
            // 
            // responsesContextMenu
            // 
            this.responsesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyTagItem});
            this.responsesContextMenu.Name = "responsesContextMenu";
            this.responsesContextMenu.ShowImageMargin = false;
            this.responsesContextMenu.Size = new System.Drawing.Size(165, 26);
            this.responsesContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.responsesContextMenu_Opening);
            // 
            // copyTagItem
            // 
            this.copyTagItem.Name = "copyTagItem";
            this.copyTagItem.Size = new System.Drawing.Size(164, 22);
            this.copyTagItem.Text = "Copy Tag To Request";
            this.copyTagItem.Click += new System.EventHandler(this.copyTagItem_Click);
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(431, 478);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 23);
            this.clearButton.TabIndex = 17;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 516);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.responsesBox);
            this.Controls.Add(this.dataLabel);
            this.Controls.Add(this.dataBox);
            this.Controls.Add(this.tidLabel);
            this.Controls.Add(this.tidBox);
            this.Controls.Add(this.tagLabel);
            this.Controls.Add(this.tagBox);
            this.Controls.Add(this.requestProperties);
            this.Controls.Add(this.commandLabel);
            this.Controls.Add(this.commandBox);
            this.Controls.Add(this.responseLabel);
            this.Controls.Add(this.sendRequestButton);
            this.Controls.Add(this.requestLabel);
            this.Controls.Add(this.requestBox);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.deviceBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "SkyeCmd CLR";
            this.responsesContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox deviceBox;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox requestBox;
        private System.Windows.Forms.Label requestLabel;
        private System.Windows.Forms.Button sendRequestButton;
        private System.Windows.Forms.Label responseLabel;
        private System.Windows.Forms.ComboBox commandBox;
        private System.Windows.Forms.Label commandLabel;
        private System.Windows.Forms.PropertyGrid requestProperties;
        private System.Windows.Forms.ComboBox tagBox;
        private System.Windows.Forms.Label tagLabel;
        private System.Windows.Forms.TextBox tidBox;
        private System.Windows.Forms.Label tidLabel;
        private System.Windows.Forms.Label dataLabel;
        private System.Windows.Forms.TextBox dataBox;
        private System.Windows.Forms.ListBox responsesBox;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.ContextMenuStrip responsesContextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyTagItem;

    }
}

