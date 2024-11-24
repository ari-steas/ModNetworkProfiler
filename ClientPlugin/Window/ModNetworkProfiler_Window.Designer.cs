using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VRage.Network;

namespace ClientPlugin.Window
{
    partial class ModNetworkProfiler_Window
    {
        public static ProfilingTracker Tracker => Plugin.Instance?.Tracker;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private bool IsUpdating = false;
        private ProfileGraph ProfileGraph = null;
        private Stopwatch _watch = new Stopwatch();


        [DllImport("DwmApi")] //System.Runtime.InteropServices
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        protected override void OnHandleCreated(EventArgs e)
        {
            if (DwmSetWindowAttribute(Handle, 19, new[] { 1 }, 4) != 0)
                DwmSetWindowAttribute(Handle, 20, new[] { 1 }, 4);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleParam = base.CreateParams;
                handleParam.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED       
                return handleParam;
            }
        }

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

        private char _updateChar = '-';
        public void UpdateData()
        {
            if (IsDisposed || Tracker == null)
                return;

            if (ProfileGraph == null)
                ProfileGraph = new ProfileGraph(chart1);

            if (InvokeRequired)
            {
                if (IsUpdating)
                    return;
                BeginInvoke((MethodInvoker) delegate {
                    UpdateData();
                });
                return;
            }

            IsUpdating = true;
            if (!_watch.IsRunning)
                _watch.Start();

            try
            {
                this.Text = $"ModNetworkProfiler {Tracker.CurrentInterval/TimeSpan.TicksPerSecond}s {_updateChar}";
                switch (_updateChar)
                {
                    case '-':
                        _updateChar = '\\';
                        break;
                    case '\\':
                        _updateChar = '|';
                        break;
                    case '|':
                        _updateChar = '/';
                        break;
                    case '/':
                        _updateChar = '-';
                        break;
                }

                List<ushort> trackedTypes = new List<ushort>();
                NetworkDownList.BeginUpdate();
                foreach (var packetTypeKvp in Tracker.DeclaringTypeMap)
                {
                    var node = NetworkDownList.Nodes[Tracker.GetNetworkIdName(packetTypeKvp.Key)];
                    if (node == null || !node.IsExpanded)
                        continue;
                    trackedTypes.Add(packetTypeKvp.Key);

                    int pktCountDown = -1;
                    int loadDown = Tracker.GetNetworkLoadDown(packetTypeKvp.Key, out pktCountDown);
                    int pktCountUp = -1;
                    int loadUp = Tracker.GetNetworkLoadUp(packetTypeKvp.Key, out pktCountUp);

                    node.Nodes[1].Text = $"Packets: {pktCountUp} up | {pktCountDown} down";
                    node.Nodes[2].Text = $"Network Load: {loadUp}b up | {loadDown}b down";
                }
                NetworkDownList.EndUpdate();

                ProfileGraph.TrackedTypes = trackedTypes;

                if (_watch.ElapsedMilliseconds > 1000)
                {
                    _watch.Restart();
                    ProfileGraph.Update(Tracker);
                }
            }
            catch (Exception ex)
            {
                DebugLabel.Text = ex.ToString();
            }

            IsUpdating = false;
        }

        public void RegisterDownHandler(string fullName, ushort id)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                Invoke((MethodInvoker) delegate {
                    RegisterDownHandler(fullName, id);
                });
                return;
            }

            NetworkDownList.BeginUpdate();
            if (!NetworkDownList.Nodes.ContainsKey(fullName))
            {
                var newNode = NetworkDownList.Nodes.Add(fullName, fullName);
                newNode.Nodes.Add($"Id: {id}");
                newNode.Nodes.Add($"Packets: 0 up | 0 down");
                newNode.Nodes.Add($"Network Load: 0b up | 0b down");
                newNode.Expand();
            }
            NetworkDownList.EndUpdate();
        }

        public void UnregisterDownHandler(string fullName)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                Invoke((MethodInvoker) delegate {
                    UnregisterDownHandler(fullName);
                });
                return;
            }

            NetworkDownList.BeginUpdate();
            NetworkDownList.Nodes.RemoveByKey(fullName);
            NetworkDownList.EndUpdate();
        }

        public void UnregisterAll()
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                Invoke((MethodInvoker) delegate {
                    UnregisterAll();
                });
                return;
            }

            NetworkDownList.BeginUpdate();
            NetworkDownList.Nodes.Clear();
            NetworkDownList.EndUpdate();
        }

        public void RegisterUpHandler(ushort id)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                Invoke((MethodInvoker) delegate {
                    RegisterUpHandler(id);
                });
                return;
            }

            string fullName = Tracker.GetNetworkIdName(id);
            NetworkDownList.BeginUpdate();
            if (!NetworkDownList.Nodes.ContainsKey(fullName))
            {
                var newNode = NetworkDownList.Nodes.Add(fullName, fullName);
                newNode.Nodes.Add($"Id: {id}");
                newNode.Nodes.Add($"Packets: 0");
                newNode.Nodes.Add($"Network Load: 0u | 0d");
                newNode.Expand();
            }
            NetworkDownList.EndUpdate();


        }

        public void UnregisterUpHandler(ushort networkId)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                Invoke((MethodInvoker) delegate {
                    UnregisterUpHandler(networkId);
                });
                return;
            }

            NetworkDownList.BeginUpdate();
            NetworkDownList.Nodes.RemoveByKey(Tracker.GetNetworkIdName(networkId));
            NetworkDownList.EndUpdate();
        }


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.NetworkDownTitle = new System.Windows.Forms.Label();
            this.NetworkDownList = new System.Windows.Forms.TreeView();
            this.DebugLabel = new System.Windows.Forms.Label();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.HideAllButton = new System.Windows.Forms.Button();
            this.ShowAllButton = new System.Windows.Forms.Button();
            this.playButton = new System.Windows.Forms.Button();
            this.pauseButton = new System.Windows.Forms.Button();
            this.ListLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.GraphLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ModeRadioButton_Network = new System.Windows.Forms.RadioButton();
            this.ModeLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ModeRadioButton_Serialization = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.ListLayoutPanel.SuspendLayout();
            this.GraphLayoutPanel.SuspendLayout();
            this.ModeLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // NetworkDownTitle
            // 
            this.NetworkDownTitle.AutoSize = true;
            this.NetworkDownTitle.Location = new System.Drawing.Point(3, 0);
            this.NetworkDownTitle.Name = "NetworkDownTitle";
            this.NetworkDownTitle.Size = new System.Drawing.Size(93, 16);
            this.NetworkDownTitle.TabIndex = 0;
            this.NetworkDownTitle.Text = "Network Down";
            this.NetworkDownTitle.Click += new System.EventHandler(this.label1_Click);
            // 
            // NetworkDownList
            // 
            this.NetworkDownList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.NetworkDownList.ForeColor = System.Drawing.Color.White;
            this.NetworkDownList.LineColor = System.Drawing.Color.White;
            this.NetworkDownList.Location = new System.Drawing.Point(3, 18);
            this.NetworkDownList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.NetworkDownList.Name = "NetworkDownList";
            this.NetworkDownList.Size = new System.Drawing.Size(631, 720);
            this.NetworkDownList.TabIndex = 1;
            // 
            // DebugLabel
            // 
            this.DebugLabel.AutoSize = true;
            this.DebugLabel.Location = new System.Drawing.Point(3, 724);
            this.DebugLabel.Name = "DebugLabel";
            this.DebugLabel.Size = new System.Drawing.Size(82, 16);
            this.DebugLabel.TabIndex = 2;
            this.DebugLabel.Text = "Errors: None";
            this.DebugLabel.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // chart1
            // 
            this.chart1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.chart1.BackSecondaryColor = System.Drawing.Color.Transparent;
            this.chart1.BorderlineColor = System.Drawing.Color.Transparent;
            chartArea3.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea3);
            this.chart1.Cursor = System.Windows.Forms.Cursors.Default;
            legend3.Name = "Legend1";
            this.chart1.Legends.Add(legend3);
            this.chart1.Location = new System.Drawing.Point(3, 2);
            this.chart1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            this.chart1.Size = new System.Drawing.Size(725, 720);
            this.chart1.TabIndex = 3;
            this.chart1.Text = "chart1";
            this.chart1.Click += new System.EventHandler(this.chart1_Click);
            // 
            // HideAllButton
            // 
            this.HideAllButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.HideAllButton.ForeColor = System.Drawing.Color.White;
            this.HideAllButton.Location = new System.Drawing.Point(328, 744);
            this.HideAllButton.Margin = new System.Windows.Forms.Padding(4);
            this.HideAllButton.Name = "HideAllButton";
            this.HideAllButton.Size = new System.Drawing.Size(100, 28);
            this.HideAllButton.TabIndex = 4;
            this.HideAllButton.Text = "Hide All";
            this.HideAllButton.UseVisualStyleBackColor = false;
            this.HideAllButton.Click += new System.EventHandler(this.HideAllButton_Click);
            // 
            // ShowAllButton
            // 
            this.ShowAllButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.ShowAllButton.ForeColor = System.Drawing.Color.White;
            this.ShowAllButton.Location = new System.Drawing.Point(220, 744);
            this.ShowAllButton.Margin = new System.Windows.Forms.Padding(4);
            this.ShowAllButton.Name = "ShowAllButton";
            this.ShowAllButton.Size = new System.Drawing.Size(100, 28);
            this.ShowAllButton.TabIndex = 5;
            this.ShowAllButton.Text = "Expand All";
            this.ShowAllButton.UseVisualStyleBackColor = false;
            this.ShowAllButton.Click += new System.EventHandler(this.ShowAllButton_Click);
            // 
            // playButton
            // 
            this.playButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.playButton.ForeColor = System.Drawing.Color.White;
            this.playButton.Location = new System.Drawing.Point(112, 744);
            this.playButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(100, 28);
            this.playButton.TabIndex = 6;
            this.playButton.Text = "PLAY";
            this.playButton.UseVisualStyleBackColor = false;
            this.playButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // pauseButton
            // 
            this.pauseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.pauseButton.ForeColor = System.Drawing.Color.White;
            this.pauseButton.Location = new System.Drawing.Point(4, 744);
            this.pauseButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pauseButton.Name = "pauseButton";
            this.pauseButton.Size = new System.Drawing.Size(100, 28);
            this.pauseButton.TabIndex = 7;
            this.pauseButton.Text = "PAUSE";
            this.pauseButton.UseVisualStyleBackColor = false;
            this.pauseButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // ListLayoutPanel
            // 
            this.ListLayoutPanel.Controls.Add(this.NetworkDownTitle);
            this.ListLayoutPanel.Controls.Add(this.NetworkDownList);
            this.ListLayoutPanel.Controls.Add(this.pauseButton);
            this.ListLayoutPanel.Controls.Add(this.playButton);
            this.ListLayoutPanel.Controls.Add(this.ShowAllButton);
            this.ListLayoutPanel.Controls.Add(this.HideAllButton);
            this.ListLayoutPanel.Controls.Add(this.ModeLayoutPanel);
            this.ListLayoutPanel.Location = new System.Drawing.Point(57, 43);
            this.ListLayoutPanel.Name = "ListLayoutPanel";
            this.ListLayoutPanel.Size = new System.Drawing.Size(631, 794);
            this.ListLayoutPanel.TabIndex = 8;
            // 
            // GraphLayoutPanel
            // 
            this.GraphLayoutPanel.Controls.Add(this.chart1);
            this.GraphLayoutPanel.Controls.Add(this.DebugLabel);
            this.GraphLayoutPanel.Location = new System.Drawing.Point(788, 43);
            this.GraphLayoutPanel.Name = "GraphLayoutPanel";
            this.GraphLayoutPanel.Size = new System.Drawing.Size(725, 794);
            this.GraphLayoutPanel.TabIndex = 9;
            // 
            // ModeRadioButton_Network
            // 
            this.ModeRadioButton_Network.AutoSize = true;
            this.ModeRadioButton_Network.Checked = true;
            this.ModeRadioButton_Network.Location = new System.Drawing.Point(3, 3);
            this.ModeRadioButton_Network.Name = "ModeRadioButton_Network";
            this.ModeRadioButton_Network.Size = new System.Drawing.Size(149, 20);
            this.ModeRadioButton_Network.TabIndex = 8;
            this.ModeRadioButton_Network.TabStop = true;
            this.ModeRadioButton_Network.Text = "Network Load Mode";
            this.ModeRadioButton_Network.UseVisualStyleBackColor = true;
            this.ModeRadioButton_Network.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // ModeLayoutPanel
            // 
            this.ModeLayoutPanel.Controls.Add(this.ModeRadioButton_Network);
            this.ModeLayoutPanel.Controls.Add(this.ModeRadioButton_Serialization);
            this.ModeLayoutPanel.Location = new System.Drawing.Point(435, 743);
            this.ModeLayoutPanel.Name = "ModeLayoutPanel";
            this.ModeLayoutPanel.Size = new System.Drawing.Size(193, 51);
            this.ModeLayoutPanel.TabIndex = 9;
            // 
            // ModeRadioButton_Serialization
            // 
            this.ModeRadioButton_Serialization.AutoSize = true;
            this.ModeRadioButton_Serialization.Location = new System.Drawing.Point(3, 29);
            this.ModeRadioButton_Serialization.Name = "ModeRadioButton_Serialization";
            this.ModeRadioButton_Serialization.Size = new System.Drawing.Size(139, 20);
            this.ModeRadioButton_Serialization.TabIndex = 9;
            this.ModeRadioButton_Serialization.Text = "Serialization Mode";
            this.ModeRadioButton_Serialization.UseVisualStyleBackColor = true;
            this.ModeRadioButton_Serialization.CheckedChanged += new System.EventHandler(this.ModeRadioButton_Serialization_CheckedChanged);
            // 
            // ModNetworkProfiler_Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.ClientSize = new System.Drawing.Size(1580, 879);
            this.Controls.Add(this.ListLayoutPanel);
            this.Controls.Add(this.GraphLayoutPanel);
            this.ForeColor = System.Drawing.SystemColors.Control;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ModNetworkProfiler_Window";
            this.Text = "ModNetworkProfiler";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ListLayoutPanel.ResumeLayout(false);
            this.ListLayoutPanel.PerformLayout();
            this.GraphLayoutPanel.ResumeLayout(false);
            this.GraphLayoutPanel.PerformLayout();
            this.ModeLayoutPanel.ResumeLayout(false);
            this.ModeLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label NetworkDownTitle;
        private System.Windows.Forms.TreeView NetworkDownList;
        private Label DebugLabel;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private Button HideAllButton;
        private Button ShowAllButton;
        private Button playButton;
        private Button pauseButton;
        private FlowLayoutPanel ListLayoutPanel;
        private FlowLayoutPanel GraphLayoutPanel;
        private FlowLayoutPanel ModeLayoutPanel;
        private RadioButton ModeRadioButton_Network;
        private RadioButton ModeRadioButton_Serialization;
    }
}