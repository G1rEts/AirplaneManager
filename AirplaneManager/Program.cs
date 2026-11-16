using System;
using System.Windows.Forms;

namespace AirplaneManager
{
    // Точка входа в приложение
    internal static class Program
    {
        // Главный метод запуска формы приложения
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
