using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Timer = System.Windows.Forms.Timer;

namespace RemoteControlServer
{
    public partial class MainForm : Form
    {
        private Timer _timer;
        private Thread _thread;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _timer = new Timer {Interval = 1000};
            _timer.Tick += SendBroadcast;
            _timer.Start();

            _thread = new Thread(AsynchronousSocketListener.StartListening);
            _thread.Start();
        }

        private static void SendBroadcast(object sender, EventArgs args)
        {
            var udp = new UdpClient();

            const int groupPort = 11111;
            var groupEp = new IPEndPoint(IPAddress.Broadcast, groupPort);

            var sendBytes = Encoding.ASCII.GetBytes(Environment.MachineName);

            udp.Send(sendBytes, sendBytes.Length, groupEp);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _timer.Stop();
            _thread.Abort();
        }
    }
}