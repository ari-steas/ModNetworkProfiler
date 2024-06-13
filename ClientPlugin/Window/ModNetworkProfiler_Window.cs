using System;
using System.Windows.Forms;

namespace ClientPlugin.Window
{
    public partial class ModNetworkProfiler_Window : Form
    {
        public ModNetworkProfiler_Window()
        {
            InitializeComponent();
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
    }
}
