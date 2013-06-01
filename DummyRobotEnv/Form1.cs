using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace DummyRobotEnv
{
    public partial class Form1 : Form
    {
        private Server Srv;
        private Map map;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Srv = new Server();
            map = new Map(32,32,500,500,this);
            Srv.OnConnectionAccepted += Srv_OnConnectionAccepted;
        }

        void Srv_OnConnectionAccepted(object sender, Socket s)
        {
            var Robot1 = new Robot(s);
            map.AddRobot(Robot1);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            map.Draw(g);
            map.Update();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
