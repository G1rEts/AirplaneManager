using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AirplaneManager.Reports;
using AirplaneManager.XmlManager;

namespace AirplaneManager
{
    // Точка входа в приложение
    internal class Program
    {
        // Главный метод запуска WinForms или пакетной генерации контрольных отчетов
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--generate-test-reports")
            {
                GenerateTestReports();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        // Автоматическая генерация контрольных отчетов и превью интерфейса в фоне
        private static void GenerateTestReports()
        {
            try
            {
                string targetDir = @"C:\Users\scpgg\Documents\LETI\OOP\airplanetask";
                string xmlPath = Path.Combine(targetDir, "test_flights.xml");

                if (!File.Exists(xmlPath))
                {
                    Console.WriteLine($"XML file not found: {xmlPath}");
                    return;
                }

                Console.WriteLine("Загрузка рейсов из XML...");
                var flights = FlightXmlManager.ImportFlights(xmlPath);
                Console.WriteLine($"Загружено рейсов: {flights.Count}");

                string pdfReportPath = Path.Combine(targetDir, "5314-Nagaev_Lab07_Report_Template1.pdf");
                string htmlReportPath = Path.Combine(targetDir, "5314-Nagaev_Lab07_Report_Template2.html");

                Console.WriteLine($"Генерация отчета PDF (Шаблон 1) -> {pdfReportPath}...");
                ReportManager.ExportFlightsToPdf(flights, pdfReportPath);

                // Сохранение визуального превью страниц PDF-отчета в PNG
                try
                {
                    var pdfImages = PdfReportBuilder.GenerateFlightsPdfImages(flights);
                    int pIndex = 1;
                    foreach (var imgBytes in pdfImages)
                    {
                        string imgPath = Path.Combine(targetDir, "media", $"lab07_pdf_template1_page{pIndex}.png");
                        File.WriteAllBytes(imgPath, imgBytes);
                        Console.WriteLine($"Сохранено превью PDF (Шаблон 1) -> {imgPath}");
                        pIndex++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка создания превью PDF: {ex.Message}");
                }

                Console.WriteLine($"Генерация отчета HTML (Шаблон 2) -> {htmlReportPath}...");
                ReportManager.ExportFlightsToHtml(flights, htmlReportPath);

                // Также генерируем отчет по отдельной таблице БД (Самолеты)
                var headers = new List<string> { "ID", "Модель самолета", "Пассажировместимость", "Класс ВС" };
                var planeRows = new List<List<string>>
                {
                    new List<string> { "1", "Boeing 737-800", "189", "Среднемагистральный" },
                    new List<string> { "2", "Airbus A320", "180", "Среднемагистральный" },
                    new List<string> { "3", "Boeing 777-300ER", "427", "Дальнемагистральный" },
                    new List<string> { "4", "Superjet 100", "103", "Ближнемагистральный" },
                    new List<string> { "5", "Airbus A350-900", "316", "Дальнемагистральный" }
                };

                string tablePdfPath = Path.Combine(targetDir, "5314-Nagaev_Lab07_TableReport_Airplanes.pdf");
                string tableHtmlPath = Path.Combine(targetDir, "5314-Nagaev_Lab07_TableReport_Airplanes.html");

                Console.WriteLine($"Генерация отчета по таблице Самолеты (PDF) -> {tablePdfPath}...");
                PdfReportBuilder.GenerateGenericTablePdfToFile("Воздушный флот (Airplanes)", headers, planeRows, tablePdfPath);

                try
                {
                    var tableImages = PdfReportBuilder.GenerateGenericTablePdfImages("Воздушный флот (Airplanes)", headers, planeRows);
                    int tIndex = 1;
                    foreach (var imgBytes in tableImages)
                    {
                        string tImgPath = Path.Combine(targetDir, "media", $"lab07_table_airplanes_page{tIndex}.png");
                        File.WriteAllBytes(tImgPath, imgBytes);
                        Console.WriteLine($"Сохранено превью таблицы PDF -> {tImgPath}");
                        tIndex++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка создания превью таблицы: {ex.Message}");
                }

                Console.WriteLine($"Генерация отчета по таблице Самолеты (HTML) -> {tableHtmlPath}...");
                HtmlReportBuilder.GenerateGenericTableHtmlToFile("Воздушный флот (Airplanes)", headers, planeRows, tableHtmlPath);

                Console.WriteLine("Создание снимков экранных форм интерфейса...");
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    using var form = new MainForm();
                    form.StartPosition = FormStartPosition.Manual;
                    form.Location = new System.Drawing.Point(-2000, -2000); // Offscreen
                    form.Size = new System.Drawing.Size(1280, 720);
                    form.Show();
                    Application.DoEvents();

                    form.PrepareViewForSnapshot(true);
                    Application.DoEvents();

                    using var bmpChoice = new System.Drawing.Bitmap(form.Width, form.Height);
                    form.DrawToBitmap(bmpChoice, new System.Drawing.Rectangle(0, 0, form.Width, form.Height));
                    string uiChoicePath = Path.Combine(targetDir, "media", "lab07_ui_choice.png");
                    bmpChoice.Save(uiChoicePath, System.Drawing.Imaging.ImageFormat.Png);
                    Console.WriteLine($"Сохранен скриншот выбора рейсов -> {uiChoicePath}");

                    form.PrepareViewForSnapshot(false);
                    Application.DoEvents();

                    using var bmpTable = new System.Drawing.Bitmap(form.Width, form.Height);
                    form.DrawToBitmap(bmpTable, new System.Drawing.Rectangle(0, 0, form.Width, form.Height));
                    string uiTablePath = Path.Combine(targetDir, "media", "lab07_ui_table.png");
                    bmpTable.Save(uiTablePath, System.Drawing.Imaging.ImageFormat.Png);
                    Console.WriteLine($"Сохранен скриншот таблицы с кнопками экспорта -> {uiTablePath}");

                    form.Hide();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка создания снимков формы: {ex.Message}");
                }

                Console.WriteLine("ВСЕ ОТЧЕТЫ УСПЕШНО СФОРМИРОВАНЫ!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА: {ex}");
            }
        }
    }
}