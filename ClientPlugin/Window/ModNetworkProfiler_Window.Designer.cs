using System;
using System.Collections.Generic;
using System.Linq;
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

                List<ushort> trackedTypes = Tracker.DeclaringTypeMap.Keys.ToList();
                foreach (var packetTypeKvp in Tracker.DeclaringTypeMap)
                {
                    var node = NetworkDownList.Nodes[packetTypeKvp.Value.FullName];
                    if (node == null)
                        continue;

                    int pktCountDown = -1;
                    int loadDown = Tracker.GetNetworkLoadDown(packetTypeKvp.Key, out pktCountDown);
                    int pktCountUp = -1;
                    int loadUp = Tracker.GetNetworkLoadUp(packetTypeKvp.Key, out pktCountDown);

                    node.Nodes[1].Text = $"Packets: {pktCountUp}u | {pktCountDown}d";
                    node.Nodes[2].Text = $"Network Load: {loadUp}u | {loadDown}d";
                }

                ProfileGraph.Update(Tracker, trackedTypes);
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
                newNode.Nodes.Add($"Packets: 0");
                newNode.Nodes.Add($"Network Load: 0u | 0d");
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.NetworkDownTitle = new System.Windows.Forms.Label();
            this.NetworkDownList = new System.Windows.Forms.TreeView();
            this.DebugLabel = new System.Windows.Forms.Label();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // NetworkDownTitle
            // 
            this.NetworkDownTitle.AutoSize = true;
            this.NetworkDownTitle.Location = new System.Drawing.Point(54, 43);
            this.NetworkDownTitle.Name = "NetworkDownTitle";
            this.NetworkDownTitle.Size = new System.Drawing.Size(93, 16);
            this.NetworkDownTitle.TabIndex = 0;
            this.NetworkDownTitle.Text = "Network Down";
            this.NetworkDownTitle.Click += new System.EventHandler(this.label1_Click);
            // 
            // NetworkDownList
            // 
            this.NetworkDownList.Location = new System.Drawing.Point(57, 83);
            this.NetworkDownList.Name = "NetworkDownList";
            this.NetworkDownList.Size = new System.Drawing.Size(630, 720);
            this.NetworkDownList.TabIndex = 1;
            // 
            // DebugLabel
            // 
            this.DebugLabel.AutoSize = true;
            this.DebugLabel.Location = new System.Drawing.Point(705, 43);
            this.DebugLabel.Name = "DebugLabel";
            this.DebugLabel.Size = new System.Drawing.Size(82, 16);
            this.DebugLabel.TabIndex = 2;
            this.DebugLabel.Text = "Errors: None";
            this.DebugLabel.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Cursor = System.Windows.Forms.Cursors.Default;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(788, 83);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            this.chart1.Size = new System.Drawing.Size(726, 720);
            this.chart1.TabIndex = 3;
            this.chart1.Text = "chart1";
            // 
            // ModNetworkProfiler_Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1580, 879);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.DebugLabel);
            this.Controls.Add(this.NetworkDownList);
            this.Controls.Add(this.NetworkDownTitle);
            this.Name = "ModNetworkProfiler_Window";
            this.Text = "ModNetworkProfiler";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label NetworkDownTitle;
        private System.Windows.Forms.TreeView NetworkDownList;
        private Label DebugLabel;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
    }
}