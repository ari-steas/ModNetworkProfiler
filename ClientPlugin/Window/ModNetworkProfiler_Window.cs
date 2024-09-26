using System;
using System.Windows.Forms;

namespace ClientPlugin.Window
{
    public partial class ModNetworkProfiler_Window : Form
    {
        // Reference to the ProfilingTracker
        private ProfilingTracker _profilingTracker;

        // Constructor that takes a ProfilingTracker
        public ModNetworkProfiler_Window(ProfilingTracker profilingTracker)
        {
            InitializeComponent();
            _profilingTracker = profilingTracker; // Assign the tracker
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }


        private void HideAllButton_Click(object sender, EventArgs e)
        {
            NetworkDownList.CollapseAll();
        }

        private void ShowAllButton_Click(object sender, EventArgs e)
        {
            NetworkDownList.ExpandAll();
        }

        // Play button (button1) click event
        private void button1_Click(object sender, EventArgs e)
        {
            _profilingTracker.Play(); // Trigger the play action
            MessageBox.Show("Tracking resumed.");
        }

        // Pause button (button2) click event
        private void button2_Click(object sender, EventArgs e)
        {
            _profilingTracker.Pause(); // Trigger the pause action
            MessageBox.Show("Tracking paused.");
        }
    }
}