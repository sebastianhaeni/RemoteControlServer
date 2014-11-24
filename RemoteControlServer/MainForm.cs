using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace RemoteControlServer
{
    public partial class MainForm : Form
    {
        System.Windows.Forms.Timer timer;
        Thread thread;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += SendBroadcast;
            timer.Start();

            thread = new Thread(new ThreadStart(AsynchronousSocketListener.StartListening));
            thread.Start();
        }

        private void SendBroadcast(object sender, EventArgs args)
        {
            UdpClient udp = new UdpClient();

            int GroupPort = 11111;
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Broadcast, GroupPort);

            byte[] sendBytes = Encoding.ASCII.GetBytes(Environment.MachineName);

            udp.Send(sendBytes, sendBytes.Length, groupEP);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer.Stop();
            thread.Abort();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            
        }

        private void btnOk_Click(object sender, EventArgs e)
        {

        }

    }



}
