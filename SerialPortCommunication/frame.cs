using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PCComm;
namespace PCComm
{
    public partial class frame : Form
    {
        CommunicationManager comm = new CommunicationManager();
        string transType = string.Empty;
        public frame()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
           LoadValues();
           SetControlState();
        }

        private void cmdOpen_Click(object sender, EventArgs e)
        {
            comm.Parity = cboParity.Text;
            comm.BaudRate = cboBaud.Text;
            comm.PortName = cboPorts.Text;
            comm.DisplayWindow = rtbDisplay;
            comm.OpenPort();

            cmdOpen.Enabled = false;
            cmdSend.Enabled = true;
        }

        private void LoadValues()
        {
            comm.SetParityValues(cboParity);
            comm.SetPortValues(cboPorts);
        }

        private void SetControlState()
        {
            cmdSend.Enabled = false;
        }

        private void cmdSend_Click(object sender, EventArgs e)
        {
            comm.WriteData(txtSend.Text);
        }
    }
}