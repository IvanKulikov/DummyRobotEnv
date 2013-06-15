using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Sockets;

namespace DummyRobotEnv
{
    public partial class MainView : Form
    {
        /// <summary>
        /// Объявление экземпляров Карты и Сервера
        /// </summary>

        private Server Srv;
        private Map map;
        private Random r;

        public MainView()
        {
            InitializeComponent();
            // Делаем так чтоб студия не ругалась когда мы обращаемся из разных тредов
            // в класс формы к элементам интерфейса
            //CheckForIllegalCrossThreadCalls = false;

            // Инитаемс сервер
            Srv = new Server();

            // Инитаем карту
            map = new Map(32, 32, 500, 500, this);

            // Подписываемся на событие когда сервер принимает подключение 
            Srv.OnConnectionAccepted += Srv_OnConnectionAccepted;

            r = new Random(1488);
        }


        /// <summary>
        /// Обработчик события когда сервер принимает новый коннект
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="s"></param>
        void Srv_OnConnectionAccepted(object sender, Socket s)
        {
            // Инитаем нового робота, передаем ему сокет на котором был коннект
            // дальше робот работает с подключением сам
            var Robot1 = new Robot(s, r.Next(360));

            // Добавляем его на карту
            map.AddRobot(Robot1);
        }


        /// <summary>
        /// Событие отрисовки, генерируется студией
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            // Рисуем карту и предметы на ней
            map.Draw(g);
        }

        /// <summary>
        /// Таймер тик хуле
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Вызывает Form1_Paint
            Invalidate();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            map.Update();
        }

        private void Load_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog foo = new OpenFileDialog();
                foo.ShowDialog();
                map.bgMap = new Bitmap(foo.FileName);
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Srv.Sart(Convert.ToInt32(textBox1.Text));
                button1.Enabled = false;
            }
            catch
            {
                MessageBox.Show("Введены некорректные данные, попробуйте еще раз!");
            }
        }
    }
}
