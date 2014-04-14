using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RailNet.Clients.Ecos;

namespace RailNet.Signalsteuerung.WF
{
    public partial class Form1 : Form
    {
        private RailClient rc;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            rc = new RailClient();
        }

        private void RadioClick(object sender, EventArgs e)
        {
            var btn = sender as RadioButton;

            if (btn == null)
                return;

            var tag = btn.Tag.ToString().Split('-');

            int addr = int.Parse(tag[0]) + (int)numAddr.Value ;
            bool red = tag[1] == "R";

            if (rc.Connected)
                rc.BasicClient.Set(11, "switch", "DCC" + addr + (red ? "r" : "g"));
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            txtHostname.Enabled = false;

            if (await rc.ConnectAsync(txtHostname.Text))
            {
                btnDisconnect.Enabled = true;
            }
            else
            {
                btnConnect.Enabled = true;
                txtHostname.Enabled = true;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            btnDisconnect.Enabled = false;

            rc.Disconnect();

            btnConnect.Enabled = true;
            txtHostname.Enabled = true;
        }
    }
}
