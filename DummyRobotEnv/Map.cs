using System;
using System.Collections.Generic;
using System.Drawing;
using DummyRobotEnv.Properties;

namespace DummyRobotEnv
{
    public class Map
    {
        public int offsetX;
        public int offsetY;
        private int width;
        public int height;
        public MainView parent;
        public List<Robot> robotsList = new List<Robot>();
        private Bitmap robotBmp = new Bitmap(Resources.Robot);
        private int id;
        public Bitmap bgMap;


        public Map()
        {
            offsetX = 0;
            offsetY = 0;
        }


        /// <summary>
        ///  Основной конструктор
        /// </summary>
        /// <param name="_offsetX">Отступ по Х от левого края формы</param>
        /// <param name="_offsetY">Отступ по У от левого края формы</param>
        /// <param name="_width"></param>
        /// <param name="_height"></param>
        /// <param name="_parent">Форма - родитель, в которой объявили экземпляр. Нужна для вывода инфы на гуй</param>
        public Map(int _offsetX, int _offsetY, int _width, int _height, MainView _parent)
        {
            parent = _parent;
            offsetX = _offsetX;
            offsetY = _offsetY;
            width = _width;
            height = _height;
            id = 0;

            // Создаем БМП-шку и заливаем белым
            bgMap = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    bgMap.SetPixel(i, j, Color.White);
        }

        /// <summary>
        /// Чистит карту (заливает белым)
        /// </summary>
        public void Clear()
        {
            if (bgMap != null)
            {
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                        bgMap.SetPixel(i, j, Color.White);
            }
            parent.Invalidate();
        }

        /// <summary>
        /// Добавляет робота на крту
        /// </summary>
        /// <param name="rb"></param>
        public void AddRobot(Robot rb)
        {
            // Добавляем робота в список на карте
            lock (robotsList)
            {
                robotsList.Add(rb); 
            }

            // Присваиваем ему id
            rb.id = id;
            id++;

            // Подписываемся на ивент дисконнекта
            rb.OnConnectionLost += rb_OnConnectionLost;

            // Подписываемся на ивент пришедших данных
            rb.OnDataRecievedExternal += rb_OnDataRecievedExternal;
        }

        void rb_OnDataRecievedExternal(Robot sender, EventArgs e)
        {
            parent.Invalidate();
        }

        void rb_OnConnectionLost(Robot sender, EventArgs e)
        {
            // Обрабатываем дисконнект робота
            // Удаляем его из списка
            lock (robotsList)
            {
                foreach (var robot in robotsList)
                {
                    if (robot == sender)
                    {
                        robotsList.Remove(robot);
                        break;
                    }
                }
                parent.Invalidate();
            }
        }

        /// <summary>
        /// Чисто удаляет робота, делая дисконнект
        /// </summary>
        public void RemoveRobot()
        {
            lock (robotsList)
            {
                foreach (var robot in robotsList)
                {
                    if (robot.selected)
                    {
                        robot.Disconnect();
                        robotsList.Remove(robot);
                        break;
                    }
                }
                parent.Invalidate();
            }
        }

        /// <summary>
        /// Здесь должна обрабатываться вся логика роботов, вызовы методов, обновление параметров и координат
        /// </summary>
        public void Update()
        {
            lock (robotsList)
            {
                foreach (var robot in robotsList)
                {
                    robot.Scan(bgMap);
                    if (robot.obstRange < 35)
                    {
                        robot.facing += 1;
                    }
                    else
                    {
                        robot.Move(2);
                    }
                    robot.SendStatus();
                }
            }
        }

        // Основной Loop рисования вызывается постоянно каждую сек
        public void Draw(Graphics canvas)
        {
            // Подкладываем полученную до этого карту под поле
            canvas.DrawImage(bgMap, offsetX, offsetY, width, height);

            // Рисуем всех роботов
            lock (robotsList)
            {
                foreach (var robot in robotsList)
                {
                    robot.Draw(canvas, this);
                }
            }
        }
    }
}
