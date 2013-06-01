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
        /// <summary>
        /// Объявление экземпляров Карты и Сервера
        /// </summary>
 
        private Server Srv;
        private Map map;

        public Form1()
        {
            InitializeComponent();
            // Делаем так чтоб студия не ругалась когда мы обращаемся из разных тредов
            // в класс формы к элементам интерфейса
            CheckForIllegalCrossThreadCalls = false;
            
            // Инитаемс сервер
            Srv = new Server();
            
            // Инитаем карту
            map = new Map(32,32,500,500,this);

            // Подписываемся на событие когда сервер принимает подключение 
            Srv.OnConnectionAccepted += Srv_OnConnectionAccepted;
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
            var Robot1 = new Robot(s);

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

            // Обновляем - ходьба роботов и проч
            map.Update();
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
    }
}
