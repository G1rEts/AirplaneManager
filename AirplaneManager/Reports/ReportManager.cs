using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AirplaneManager.Models;
using AirplaneManager.XmlManager;

namespace AirplaneManager.Reports
{
    // Фасад управления формированием и экспортом отчетов в форматах PDF и HTML
    public static class ReportManager
    {
        // Экспорт списка рейсов в PDF-документ (Шаблон 1)
        public static void ExportFlightsToPdf(List<Flight> flights, string outputPath)
        {
            if (flights == null) throw new ArgumentNullException(nameof(flights));
            // Передача списка рейсов построителю PDF для генерации файла
            PdfReportBuilder.GenerateFlightsPdfToFile(flights, outputPath);
        }

        // Экспорт списка рейсов в HTML-документ (Шаблон 2)
        public static void ExportFlightsToHtml(List<Flight> flights, string outputPath)
        {
            if (flights == null) throw new ArgumentNullException(nameof(flights));
            // Передача списка рейсов построителю HTML для формирования отчета
            HtmlReportBuilder.GenerateFlightsHtmlToFile(flights, outputPath);
        }

        // Загрузка рейсов из XML-файла и экспорт в PDF
        public static void ExportFlightsFromXmlToPdf(string xmlPath, string outputPath)
        {
            if (!File.Exists(xmlPath))
                throw new FileNotFoundException($"Файл XML не найден: {xmlPath}");

            // Десериализация XML-документа в коллекцию объектов Flight
            var flights = FlightXmlManager.ImportFlights(xmlPath);
            // Создание PDF-файла на основе распарсенных данных
            PdfReportBuilder.GenerateFlightsPdfToFile(flights, outputPath, "ОТЧЕТ ПО РЕЙСАМ (ИСТОЧНИК: XML)");
        }

        // Загрузка рейсов из XML-файла и экспорт в HTML
        public static void ExportFlightsFromXmlToHtml(string xmlPath, string outputPath)
        {
            if (!File.Exists(xmlPath))
                throw new FileNotFoundException($"Файл XML не найден: {xmlPath}");

            // Десериализация XML-документа в коллекцию объектов Flight
            var flights = FlightXmlManager.ImportFlights(xmlPath);
            // Создание HTML-файла на основе распарсенных данных
            HtmlReportBuilder.GenerateFlightsHtmlToFile(flights, outputPath, "Ведомость расписания рейсов (Источник: XML)");
        }

        // Экспорт данных текущей таблицы DataGridView в PDF (Шаблон 2)
        public static void ExportTableToPdf(string tableName, DataGridView grid, string outputPath)
        {
            // Извлечение заголовков столбцов и текстовых значений ячеек
            ExtractGridData(grid, out var headers, out var rows);
            // Генерация табличного PDF-документа
            PdfReportBuilder.GenerateGenericTablePdfToFile(tableName, headers, rows, outputPath);
        }

        // Экспорт данных текущей таблицы DataGridView в HTML (Шаблон 2)
        public static void ExportTableToHtml(string tableName, DataGridView grid, string outputPath)
        {
            // Извлечение заголовков столбцов и текстовых значений ячеек
            ExtractGridData(grid, out var headers, out var rows);
            // Генерация табличного HTML-документа
            HtmlReportBuilder.GenerateGenericTableHtmlToFile(tableName, headers, rows, outputPath);
        }

        // Извлечение видимых заголовков и значений ячеек из DataGridView
        private static void ExtractGridData(DataGridView grid, out List<string> headers, out List<List<string>> rows)
        {
            headers = new List<string>();
            // Список индексов только видимых пользователю столбцов
            var visibleColIndices = new List<int>();

            // Сбор заголовков видимых колонок таблицы
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                var col = grid.Columns[i];
                if (col.Visible)
                {
                    // Использование HeaderText либо имени колонки, если заголовок пуст
                    headers.Add(string.IsNullOrWhiteSpace(col.HeaderText) ? col.Name : col.HeaderText);
                    visibleColIndices.Add(i);
                }
            }

            rows = new List<List<string>>();
            // Обход строк таблицы и извлечение строковых значений ячеек
            foreach (DataGridViewRow dgvRow in grid.Rows)
            {
                // Пропуск пустой строки добавления новой записи
                if (dgvRow.IsNewRow) continue;

                var rowValues = new List<string>();
                foreach (int colIdx in visibleColIndices)
                {
                    var val = dgvRow.Cells[colIdx].Value;
                    rowValues.Add(val?.ToString() ?? "");
                }
                rows.Add(rowValues);
            }
        }
    }
}
