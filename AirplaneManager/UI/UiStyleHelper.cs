using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace AirplaneManager.UI
{
    public static class UiStyleHelper
    {
        // Цветовая палитра
        public static readonly Color SidebarBgColor = Color.FromArgb(180, 240, 240, 240);
        public static readonly Color HeaderBgColor = Color.FromArgb(180, 245, 245, 245);
        public static readonly Color BottomBarBgColor = Color.FromArgb(210, 230, 230, 230);

        public static readonly Color SidebarBtnDefault = Color.FromArgb(215, 215, 215);
        public static readonly Color SidebarBtnActive = Color.FromArgb(50, 50, 50);
        public static readonly Color ActionBtnBg = Color.FromArgb(200, 190, 190);

        // Включает двойную буферизацию у любого элемента 
        public static void EnableDoubleBuffering(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        // Создание центральной стилизованной кнопки
        public static Button CreateStyledButton(string text, Size size)
        {
            return new Button
            {
                Text = text,
                Font = new Font("Arial", 13, FontStyle.Bold),
                Size = size,
                BackColor = ActionBtnBg,
                FlatStyle = FlatStyle.Flat
            };
        }

        // Создание кнопки бокового меню
        public static Button CreateSidebarButton(string text, int y, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, y),
                Size = new Size(220, 40),
                BackColor = SidebarBtnDefault,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        // Поиск картинок в папках Properties и View
        public static Image? LoadImageFromViewFolder(string fileName)
        {
            string? currentDir = Application.StartupPath;
            for (int i = 0; i < 5; i++)
            {
                if (currentDir == null) break;

                string propPath = Path.Combine(currentDir, "Properties", fileName);
                if (File.Exists(propPath)) return Image.FromFile(propPath);

                currentDir = Directory.GetParent(currentDir)?.FullName;
            }
            return null;
        }
    }

    // Специальная панель со встроенной двойной буферизацией
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.SupportsTransparentBackColor, true);
            this.UpdateStyles();
        }
    }

    // Специальная адаптивная сетка со встроенной двойной буферизацией
    public class BufferedTableLayoutPanel : TableLayoutPanel
    {
        public BufferedTableLayoutPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.SupportsTransparentBackColor, true);
            this.UpdateStyles();
        }
    }
}