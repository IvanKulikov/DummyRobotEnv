using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DummyRobotEnv.Properties;

namespace DummyRobotEnv
{
    public class Map
    {
        public int offsetX;
        public int offsetY;
        private int width;
        public int height;
        public Form1 parent;
        private List<Robot> robotsList = new List<Robot>();
        private Bitmap robotBmp = new Bitmap(Resources.Robot);
        private int id;

        public Bitmap bgMap;


        public Map()
        {
            offsetX = 0;
            offsetY = 0;
        }

        public Map(int _offsetX, int _offsetY, int _width, int _height, Form1 _parent)
        {
            parent = _parent;
            offsetX = _offsetX;
            offsetY = _offsetY;
            width = _width;
            height = _height;
            id = 0;
            bgMap = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    bgMap.SetPixel(i, j, Color.White);
        }

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

        public void AddRobot(Robot rb)
        {
            // Добавляем робота в список на карте
            robotsList.Add(rb);

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
            parent.Invalidate();
        }

        public void RemoveRobot()
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

        public void Update()
        {
            foreach (var robot in robotsList)
            {
                //robot.Move(2);
            }
        }

        // Основной Loop рисования вызывается постоянно каждую сек
        public void Draw(Graphics canvas)
        {
            // Подкладываем полученную до этого карту под поле
            canvas.DrawImage(bgMap, offsetX, offsetY, width, height);

            foreach (var robot in robotsList)
            {
                robot.Draw(canvas, this);
            }
        }
    }
}
