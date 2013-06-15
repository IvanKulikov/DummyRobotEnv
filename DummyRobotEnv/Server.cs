using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace DummyRobotEnv
{
    class Server
    {
        // Объявляем сокет
        private Socket ListenSoc;

        // Делегат события приема подключения
        public delegate void OnConnectionAcceptedArgs(object sender, Socket s);

        // Событие с этим делегатом
        public event OnConnectionAcceptedArgs OnConnectionAccepted;

        /// <summary>
        /// Конструктор
        /// </summary>
        public Server()
        {
            // Инитаем сокет, тип, протокол
            ListenSoc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Sart(int port)
        {
            // Делаем Бинд - вешаем его на слушать все айпишники на нужном порту
            ListenSoc.Bind(new IPEndPoint(IPAddress.Any, port));

            // Макс очередь 2 в 16
            ListenSoc.Listen(65536);

            // Асинхроно ждем подключений 
            ListenSoc.BeginAccept(OnConnectionAcceptedHandler, null);
        }

        /// <summary>
        /// Обработчик подключений 
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnectionAcceptedHandler(IAsyncResult ar)
        {
            if (ListenSoc != null)
            {
                // Сразу асинхронно ждем следующего
                ListenSoc.BeginAccept(OnConnectionAcceptedHandler, null);

                // Создаем новый сокет и инитаем его тем что получили при коннекте
                // Т.е. передаем ссылку
                Socket RobotSocket = ListenSoc.EndAccept(ar);

                // Выстреливаем эвент приема
                OnConnectionAccepted(this, RobotSocket);
            }
        }
    }
}
