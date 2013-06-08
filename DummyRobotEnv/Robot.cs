using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DummyRobotEnv.Properties;

namespace DummyRobotEnv
{
    public class Robot
    {
        
        public float x;               // Реальное положение
        public float y;
        public int gtX;             // Куда ехать Х
        public int gtY;             // Куда ехать У
        public int id;        
        public int obstRange;       // Расстояние до препятствия в см
        public string data;
        public double facing;       // Азимут в градусах
        public bool selected;
        private double dist1;       // Расстояние до опроных пунктов (базовых станций)
        private double dist2;       // Пока не задействовано
        private int ipPort; 
        private string address;
        public byte[] buf;
        public bool connectionFailed;
        public string connection;
        
        // Сокет для работы с клиентом, онициализируется в конструкторе
        private Socket s;
        
        private Bitmap robotBmp = new Bitmap(Resources.Robot);

        // Делегат события OnConnectionlost
        public delegate void RobotDelegate(Robot sender, EventArgs e);

        // Создаем событие с этим делегатом
        public event RobotDelegate OnConnectionLost;
        public event RobotDelegate OnDataRecievedExternal;


        // Всякие конструкторы.
        public Robot(Socket _s)
        {
            x = 100;
            y = 100;
            dist1 = 0;
            dist2 = 0;
            
            facing = 90; // Тест - вбито гвоздями
            s = _s; // Инитаем сокет тем что передал сервер
            buf = new byte[32]; // Буфер принемаемых данных
            data = " "; // Строка для него

            // Парсим удаленное соединение чтоб красиво писать в ГУЙ
            IPEndPoint remoteIpEndPoint = s.RemoteEndPoint as IPEndPoint;
            address = remoteIpEndPoint.Address.ToString();
            ipPort = remoteIpEndPoint.Port;
            connection = address + ":" + ipPort.ToString();
            
            // Начинаем асинхронно принемать данные
            s.BeginReceive(buf, 0, 32, SocketFlags.None, OnDataRecieved, null);
        }

        /// <summary>
        /// Двигвемся со скоростью в направлении куда facing
        /// </summary>
        /// <param name="speed"></param>
        public void Move(double speed, Form1 parent)
        {
            var dy = speed * Math.Cos(facing * Math.PI / 180);
            var dx = speed * Math.Sin(facing * Math.PI / 180);
            x += (float)dx; 
            y += (float)dy; 
            parent.listBox1.Items.Clear();
            parent.listBox1.Items.Add(dx);
            parent.listBox1.Items.Add(dy);
        }

        /// <summary>
        /// Определяем facing на основе полученных gtX, gtY
        /// </summary>
        public void GetDirection()
        {
            var centerX = x + 32;       //map.offsetX;
            var centerY = 532 - y;      //map.height + map.offsetY - y;
            double dX = centerX - gtX;
            double dY = centerY - gtY;
            var tmpFacing = Math.Acos(dX / Math.Sqrt(dX * dX + dY * dY)) * 180 / Math.PI;
            
            if (dX > 0 && dY < 0)
            {
                facing = -tmpFacing + 270;
            }
            else if (dY > 0 && dX < 0)
            {
                facing = tmpFacing - 90;
            }
            else if (dX < 0)
            {
                facing = 270 - tmpFacing;
            }
            else if (dY > 0)
            {
                facing = 270 + tmpFacing;
            }
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
            try
            {
                // Если сообщения будут приходить ОЧЕНЬ часто то буду слипаться и неправильно парситься.
                // Нужен СДЦ.

                data = Encoding.ASCII.GetString(buf);
                var dataArr = data.Split(',');
                gtX = Convert.ToInt32(dataArr[0]);
                gtY = Convert.ToInt32(dataArr[1]);
                GetDirection();
                //obstRange = Convert.ToInt32(dataArr[2]);
                //facing = Convert.ToDouble(dataArr[3]);
            }
            catch
            {
                // Посылка побилась
            }

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

        public void Draw(Graphics canvas, Map map)
        {
            // Координаты верхнего левого угла картинки робота
            var actualX = x + map.offsetX - robotBmp.Width / 2f;
            var actualY = map.height + map.offsetY - (y + robotBmp.Height / 2f);

            // Координаты центра робота
            var centerX = x + map.offsetX;
            var centerY = map.height + map.offsetY - y;

            // Координаты метки
            var markX = obstRange * Math.Sin((facing) * Math.PI / 180) + centerX;
            var markY = -obstRange * Math.Cos((facing) * Math.PI / 180) + centerY;

            // Поворачиваем робота
            var tmpBmp = RotateImageByAngle(robotBmp, (float)facing); // Пашет

            // Рисуем робота и его центр
            canvas.DrawImage(tmpBmp, actualX, actualY);
            canvas.FillRectangle(new SolidBrush(Color.Red), centerX, centerY, 2, 2);

            // Если на робота кликнули - рисуем рамку и выводим данные о нем на форму

            if (selected)
            {
                canvas.DrawRectangle(new Pen(Color.Red), actualX, actualY, robotBmp.Width, robotBmp.Height);
                if (map.parent != null)
                {
                    map.parent.listBox1.Items.Clear();
                    map.parent.listBox1.Items.Add("Номер: " + id);
                    map.parent.listBox1.Items.Add("X: " + x.ToString());
                    map.parent.listBox1.Items.Add("Y: " + y.ToString());
                    map.parent.listBox1.Items.Add("Дальномер: " + obstRange.ToString());
                    map.parent.listBox1.Items.Add("Азимут: " + facing.ToString());
                    map.parent.listBox1.Items.Add("Соединение: " + connection);
                    if (!connectionFailed)
                        map.parent.listBox1.Items.Add("Активно...");
                    else
                        map.parent.listBox1.Items.Add("РАЗОРВАНО!");
                }
            }

            if (connectionFailed)
            {
                // Рисуем рамку вокруг робота и крест (Х_х)
                canvas.DrawRectangle(new Pen(Color.Red), actualX, actualY, robotBmp.Width, robotBmp.Height);
                canvas.DrawLine(new Pen(Color.Red), actualX, actualY, actualX + robotBmp.Width, actualY + robotBmp.Height);
                canvas.DrawLine(new Pen(Color.Red), actualX + robotBmp.Width, actualY, actualX, actualY + robotBmp.Height);
            }

            // Куда робот движется
            canvas.DrawLine(new Pen(Color.Red), centerX, centerY, gtX, gtY);

            // Выводим данные (так, для теста)              
            canvas.DrawString(facing.ToString(), new Font(FontFamily.GenericSansSerif, 10), new SolidBrush(Color.Black), actualX + 32, actualY);
                

            // В разумных ли приделах показания дальномера?
            if (obstRange > 5 && obstRange < 150)
            {
                // Рисуем метку и линию до нее
                canvas.FillRectangle(new SolidBrush(Color.Black), (float)markX, (float)markY, 2, 2);
                canvas.DrawLine(new Pen(Color.Red), centerX, centerY, (float)markX, (float)markY);

                // Сохраняем метку на карте

                try
                {
                    map.bgMap.SetPixel((int)markX - map.offsetX, (int)markY - map.offsetY, Color.Black);
                }
                catch
                {
                    // Зашкалило.
                }
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
        }

        // Сниппет - поворачивает картинки

        private static Bitmap RotateImageByAngle(System.Drawing.Image oldBitmap, float angle)
        {
            var newBitmap = new Bitmap(oldBitmap.Width, oldBitmap.Height);
            var graphics = Graphics.FromImage(newBitmap);
            graphics.TranslateTransform((float)oldBitmap.Width / 2, (float)oldBitmap.Height / 2);
            graphics.RotateTransform(angle);
            graphics.TranslateTransform(-(float)oldBitmap.Width / 2, -(float)oldBitmap.Height / 2);
            graphics.DrawImage(oldBitmap, new Point(0, 0));
            return newBitmap;
        }
    }
}
