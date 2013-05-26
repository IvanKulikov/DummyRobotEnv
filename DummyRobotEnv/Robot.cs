using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyRobotEnv
{
    public class Robot
    {
        public int x;
        public int y;
        public int gtX;
        public int gtY;
        public int id;
        public int obstRange;
        public string data;
        public double facing;
        public bool selected;
        public bool connectionFailed;
        public string connection;

        public  byte[] buf;
        private int ipPort;
        private double dist1;
        private double dist2;
        private string address;
        private string portName;
        private SerialPort port;
        private Socket s;

        // Делегат события OnConnectionlost
        public delegate void RobotDelegate(Robot sender, EventArgs e);

        // Создаем событие с этим делегатом
        public event RobotDelegate OnConnectionLost;
        public event RobotDelegate OnDataRecievedExternal;


        // Всякие конструкторы.
        public Robot(Socket _s)
        {
            x = 0;
            y = 0;
            dist1 = 0;
            dist2 = 0;
            facing = 0;
            s = _s;
            buf = new byte[32];
            data = " ";
            s.BeginReceive(buf, 0, 32, SocketFlags.None, OnDataRecieved, null);
        }

        public void Disconnect()
        {
            // Вызывается при удалении робота.
            // Отключаемся.

            if (s != null && s.Connected)
            {
                s.Close();
            }
        }

        private void OnDataRecieved(IAsyncResult ar)
        {
            // Пришли данные. Пытаемся распаковать и ждем дальше.
            // SocketType.Stream какаято муть иногда слипается ногда не доходит 

            OnDataRecievedExternal(this, null);
            data = Encoding.ASCII.GetString(buf);

           // try
           // {
           //     // Если сообщения будут приходить ОЧЕНЬ часто то буду слипаться и неправильно парситься.
           //     // Нужен СДЦ.
           //
           //     //data = Encoding.ASCII.GetString(buf);
           //     //var dataArr = data.Split(',');
           //     //x = Convert.ToInt32(dataArr[0]);
           //     //y = Convert.ToInt32(dataArr[1]);
           //     //obstRange = Convert.ToInt32(dataArr[2]);
           //     //facing = Convert.ToDouble(dataArr[3]);
           // }
           //
           // catch
           // {
           //     // Посылка побилась
           // }

           Array.Clear(buf, 0, 32);

            try
            {
                // Слушаем дальше
                s.BeginReceive(buf, 0, 32, SocketFlags.None, OnDataRecieved, null);
            }
            catch
            {
                // Сокет таки не отвалился и все еще открыт, либо сервак лег
                // Кидаем ивент
                //OnConnectionLost(this, null);
                connectionFailed = true;
                s.Close();
            }
        }

        public void SendData(string msg)
        {
            // Посылаем данные виртуальному роботу

            if (s != null && s.Connected)
            {
                try
                {
                    s.Send(Encoding.ASCII.GetBytes(msg));
                }
                catch
                {
                    OnConnectionLost(this, null);
                    connectionFailed = true;
                }
            }

            else if (port != null && port.IsOpen)
            {
                try
                {
                    port.WriteLine(msg);
                }
                catch
                {
                    OnConnectionLost(this, null);
                    connectionFailed = true;
                }
            }
        }
    }
}
