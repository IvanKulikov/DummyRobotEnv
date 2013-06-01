using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DummyRobotEnv
{
    static class Program
    {
        /// <summary>
        /// Точка входа в приложение 
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
