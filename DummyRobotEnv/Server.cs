using System;
using System.Net;
using System.Net.Sockets;

namespace DummyRobotEnv
{
    class Server
    {
        private Socket ListenSoc;

        public delegate void OnConnectionAcceptedArgs(object sender, Socket s);

        public event OnConnectionAcceptedArgs OnConnectionAccepted; 

        public Server()
        {
            ListenSoc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ListenSoc.Bind(new IPEndPoint(IPAddress.Any, 666));
            ListenSoc.Listen(65565);
            ListenSoc.BeginAccept(OnConnectionAcceptedHandler, null);
        }

        private void OnConnectionAcceptedHandler(IAsyncResult ar)
        {
            ListenSoc.BeginAccept(OnConnectionAcceptedHandler, null);

            Socket RobotSocket = ListenSoc.EndAccept(ar);

            OnConnectionAccepted(this, RobotSocket);
        }
    }
}
