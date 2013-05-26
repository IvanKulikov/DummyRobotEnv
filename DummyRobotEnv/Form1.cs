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
        private List<Robot> RobotList; 
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Srv = new Server();
            RobotList = new List<Robot>();
            Srv.OnConnectionAccepted += Srv_OnConnectionAccepted;
        }

        void Srv_OnConnectionAccepted(object sender, Socket s)
        {
            var Robot1 = new Robot(s);
            RobotList.Add(Robot1);
            Robot1.OnDataRecievedExternal +=Robot1_OnDataRecievedExternal;
            RefreshForm();
        }

        private void Robot1_OnDataRecievedExternal(Robot sender, EventArgs e)
        {
            RefreshForm();
        }

        private void RefreshForm()
        {
            listBox1.Items.Clear();
            foreach (var robot in RobotList)
            {
                listBox1.Items.Add(robot.data);
            }
        }
    }
}
