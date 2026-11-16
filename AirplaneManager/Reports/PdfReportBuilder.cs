using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AirplaneManager.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AirplaneManager.Reports
{
    // Построитель отчетов в формате PDF на базе QuestPDF (Шаблон 1 и Шаблон 2)
    public static class PdfReportBuilder
    {
        static PdfReportBuilder()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.UseSystemFonts = true;
            QuestPDF.Settings.ThrowOnMissingFontFamilies = false;
        }

        // Создание документа отчета по расписанию рейсов (Шаблон 1)
        public static IDocument CreateFlightsDocument(List<Flight> flights, string title = "ОТЧЕТ ПО РАСПИСАНИЮ РЕЙСОВ")
        {
            int totalPassengers = flights.Sum(f => f.Passengers?.Count ?? 0);
            int uniquePlanes = flights.Select(f => f.Airplane?.Id).Where(id => id.HasValue).Distinct().Count();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4); // Книжная (Portrait) ориентация
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));

                    // ВЕРХНИЙ КОЛОНТИТУЛ / ШАПКА
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("AIRPLANEMANAGER — АВИАКОМПАНИЯ").FontSize(10).Bold().FontColor("#333333");
                                c.Item().Text(title).FontSize(15).Bold().FontColor("#111111");
                            });

                            row.ConstantItem(200).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).FontColor("#555555");
                                c.Item().Text($"Всего рейсов в отчете: {flights.Count}").FontSize(8).Bold().FontColor("#333333");
                            });
                        });

                        col.Item().PaddingTop(6).PaddingBottom(8).LineHorizontal(1f).LineColor("#333333");

                        // Краткая сводка
                        col.Item().PaddingBottom(10).Background("#F5F5F5").Padding(6).Row(s =>
                        {
                            s.RelativeItem().Text($"Сводка: Всего рейсов: {flights.Count} | Пассажиров: {totalPassengers} чел. | Воздушных судов: {uniquePlanes}").FontSize(8.5f).Bold().FontColor("#333333");
                        });
                    });

                    // СОДЕРЖИМОЕ — ТАБЛИЦА РЕЙСОВ
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25);  // №
                            columns.ConstantColumn(55);  // Рейс
                            columns.ConstantColumn(85);  // Дата/Время
                            columns.RelativeColumn(3);   // Маршрут
                            columns.RelativeColumn(2);   // Самолет
                            columns.RelativeColumn(3);   // Экипаж
                            columns.ConstantColumn(50);  // Пасс.
                        });

                        // Заголовки таблицы
                        table.Header(header =>
                        {
                            header.Cell().Background("#E8E8E8").Border(0.5f).BorderColor("#888888").Padding(4).AlignCenter().Text("№").Bold().FontSize(8.5f);
                            header.Cell().Background("#E8E8E8").Border(0.5f).BorderColor("#888888").Padding(4).Text("Рейс").Bold().FontSize(8.5f);
                            header.Cell().Background("#E8E8E8").Border(0.5f).BorderColor("#888888").Padding(4).Text("Дата/Время").Bold().FontSize(8.5f);
                            header.Cell().Background("#E8E8E8").Border(0.5f).BorderColor("#888888").Padding(4).Text("Маршрут").Bold().FontSize(8.5f);
                            header.Cell().Background("#E8E8E8").Border(0.5f).BorderColor("#888888").Padding(4).Text("Самолет").Bold().FontSize(8.5f);
                            header.Cell().Background("#E8E8E8").Border(0.5f).BorderColor("#888888").Padding(4).Text("Экипаж").Bold().FontSize(8.5f);
                            header.Cell().Background("#E8E8E8").Border(0.5f).BorderColor("#888888").Padding(4).AlignCenter().Text("Пасс.").Bold().FontSize(8.5f);
                        });

                        int idx = 1;
                        foreach (var f in flights)
                        {
                            string flightNum = f.Route?.RouteNumber ?? $"FL-{f.Id}";
                            string dateStr = f.FlightDate.ToString("dd.MM.yyyy HH:mm");
                            string dep = f.Route?.DepartureAirport != null ? $"{f.Route.DepartureAirport.City} ({f.Route.DepartureAirport.Code})" : "—";
                            string arr = f.Route?.ArrivalAirport != null ? $"{f.Route.ArrivalAirport.City} ({f.Route.ArrivalAirport.Code})" : "—";
                            string routeStr = $"{dep} → {arr}";
                            string planeStr = f.Airplane != null ? $"{f.Airplane.Model} ({f.Airplane.Capacity} м.)" : "—";

                            var crewList = new List<string>();
                            if (f.Pilots != null && f.Pilots.Count > 0)
                                crewList.Add("П: " + string.Join(", ", f.Pilots.Select(p => p.LastName)));
                            if (f.FlightAttendants != null && f.FlightAttendants.Count > 0)
                                crewList.Add("БП: " + string.Join(", ", f.FlightAttendants.Select(a => a.LastName)));
                            string crewStr = crewList.Count > 0 ? string.Join("; ", crewList) : "—";
                            int passCount = f.Passengers?.Count ?? 0;

                            string bg = (idx % 2 == 0) ? "#FAFAFA" : "#FFFFFF";

                            table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(4).AlignCenter().Text(idx.ToString()).FontSize(8);
                            table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(4).Text(flightNum).Bold().FontSize(8);
                            table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(4).Text(dateStr).FontSize(8);
                            table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(4).Text(routeStr).FontSize(8);
                            table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(4).Text(planeStr).FontSize(8);
                            table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(4).Text(crewStr).FontSize(7.5f);
                            table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(4).AlignCenter().Text($"{passCount}").FontSize(8);

                            idx++;
                        }
                    });

                    // НИЖНИЙ КОЛОНТИТУЛ
                    page.Footer().Column(col =>
                    {
                        col.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor("#999999");
                        col.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem(3).Text("Информационная система AirplaneManager • Лабораторная работа № 7").FontSize(8).FontColor("#777777");
                            row.RelativeItem(1).AlignRight().Text(x =>
                            {
                                x.Span("Страница ").FontSize(8).FontColor("#777777");
                                x.CurrentPageNumber().FontSize(8).Bold().FontColor("#555555");
                                x.Span(" из ").FontSize(8).FontColor("#777777");
                                x.TotalPages().FontSize(8).Bold().FontColor("#555555");
                            });
                        });
                    });
                });
            });
        }

        public static byte[] GenerateFlightsPdf(List<Flight> flights, string title = "ОТЧЕТ ПО РАСПИСАНИЮ РЕЙСОВ")
        {
            var doc = CreateFlightsDocument(flights, title);
            return doc.GeneratePdf();
        }

        // Рендеринг страниц отчета по рейсам в PNG-изображения
        public static IEnumerable<byte[]> GenerateFlightsPdfImages(List<Flight> flights, string title = "ОТЧЕТ ПО РАСПИСАНИЮ РЕЙСОВ")
        {
            var doc = CreateFlightsDocument(flights, title);
            return doc.GenerateImages();
        }

        // Сохранение PDF-отчета по рейсам в файл
        public static void GenerateFlightsPdfToFile(List<Flight> flights, string outputPath, string title = "ОТЧЕТ ПО РАСПИСАНИЮ РЕЙСОВ")
        {
            var doc = CreateFlightsDocument(flights, title);
            doc.GeneratePdf(outputPath);
        }

        // Создание документа отчета по отдельной таблице базы данных (Шаблон 2)
        public static IDocument CreateTableDocument(string tableName, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("AIRPLANEMANAGER — ОТЧЕТ ПО ТАБЛИЦЕ БД").FontSize(10).Bold().FontColor("#333333");
                                c.Item().Text($"Таблица «{tableName}»").FontSize(15).Bold().FontColor("#111111");
                            });

                            row.ConstantItem(200).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).FontColor("#555555");
                                c.Item().Text($"Всего записей: {rows.Count}").FontSize(8).Bold().FontColor("#333333");
                            });
                        });

                        col.Item().PaddingTop(6).PaddingBottom(12).LineHorizontal(1f).LineColor("#333333");
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            for (int i = 0; i < headers.Count; i++)
                            {
                                if (i == 0)
                                    columns.ConstantColumn(40);
                                else
                                    columns.RelativeColumn();
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell().Background("#E8E8E8").Border(0.5f).BorderColor("#888888").Padding(5).Text(h).Bold().FontSize(8.5f);
                            }
                        });

                        int rIdx = 0;
                        foreach (var row in rows)
                        {
                            string bg = (rIdx % 2 == 0) ? "#FAFAFA" : "#FFFFFF";
                            for (int cIdx = 0; cIdx < headers.Count; cIdx++)
                            {
                                string cellText = (cIdx < row.Count) ? row[cIdx] : "";
                                var cell = table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(5);
                                if (cIdx == 0)
                                    cell.AlignCenter().Text(cellText).FontSize(8);
                                else
                                    cell.Text(cellText).FontSize(8);
                            }
                            rIdx++;
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor("#999999");
                        col.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text($"Экспорт таблицы {tableName} • AirplaneManager").FontSize(8).FontColor("#777777");
                            row.RelativeItem().AlignRight().Text(x =>
                            {
                                x.Span("Страница ").FontSize(8).FontColor("#777777");
                                x.CurrentPageNumber().FontSize(8).Bold().FontColor("#555555");
                                x.Span(" из ").FontSize(8).FontColor("#777777");
                                x.TotalPages().FontSize(8).Bold().FontColor("#555555");
                            });
                        });
                    });
                });
            });
        }

        // Генерация байтового массива PDF-таблицы
        public static byte[] GenerateGenericTablePdf(string tableName, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            var doc = CreateTableDocument(tableName, headers, rows);
            return doc.GeneratePdf();
        }

        // Рендеринг страниц таблицы базы данных в PNG-изображения
        public static IEnumerable<byte[]> GenerateGenericTablePdfImages(string tableName, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            var doc = CreateTableDocument(tableName, headers, rows);
            return doc.GenerateImages();
        }

        // Сохранение PDF-отчета по таблице базы данных в файл
        public static void GenerateGenericTablePdfToFile(string tableName, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows, string outputPath)
        {
            var doc = CreateTableDocument(tableName, headers, rows);
            doc.GeneratePdf(outputPath);
        }
    }
}
