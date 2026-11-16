using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using AirplaneManager.Models;

namespace AirplaneManager.Reports
{
    // Построитель HTML-отчетов со строгой версткой и стилями печати (Шаблон 1 и Шаблон 2)
    public static class HtmlReportBuilder
    {
        // Формирование строки HTML-отчета по списку рейсов (Шаблон 2)
        public static string GenerateFlightsHtml(List<Flight> flights, string title = "Отчет по расписанию рейсов")
        {
            var sb = new StringBuilder();
            // Расчет агрегированных показателей расписания рейсов
            int totalPassengers = flights.Sum(f => f.Passengers.Count);
            int totalPilots = flights.Sum(f => f.Pilots.Count);
            int totalAttendants = flights.Sum(f => f.FlightAttendants.Count);
            int uniquePlanes = flights.Select(f => f.Airplane?.Id).Where(id => id.HasValue).Distinct().Count();

            // Формирование HTML-документа и стилей оформления таблицы
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"ru\">");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset=\"utf-8\">");
            sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine($"  <title>{WebUtility.HtmlEncode(title)} — AirplaneManager</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine(@"
    body {
      font-family: 'Times New Roman', Times, serif;
      margin: 40px;
      color: #111;
      background-color: #fff;
      font-size: 14px;
      line-height: 1.4;
    }
    .header {
      border-bottom: 2px solid #333;
      padding-bottom: 12px;
      margin-bottom: 20px;
    }
    h1 {
      font-size: 22px;
      margin: 0 0 6px 0;
      color: #222;
    }
    .meta {
      font-size: 13px;
      color: #555;
    }
    .summary-box {
      background-color: #f9f9f9;
      border: 1px solid #ddd;
      padding: 10px 14px;
      margin-bottom: 20px;
      font-size: 13px;
    }
    .summary-box strong {
      color: #222;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 10px;
    }
    th, td {
      border: 1px solid #888;
      padding: 8px 10px;
      text-align: left;
      vertical-align: top;
    }
    th {
      background-color: #f0f0f0;
      font-weight: bold;
      color: #222;
    }
    tr:nth-child(even) td {
      background-color: #fafafa;
    }
    .footer {
      margin-top: 30px;
      font-size: 12px;
      color: #777;
      text-align: right;
      border-top: 1px solid #ccc;
      padding-top: 8px;
    }
    @media print {
      body { margin: 10mm; }
      .summary-box { border: 1px solid #000; }
      th, td { border: 1px solid #000; }
    }
  ");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Заголовок отчета
            sb.AppendLine("  <div class=\"header\">");
            sb.AppendLine($"    <h1>{WebUtility.HtmlEncode(title)}</h1>");
            sb.AppendLine($"    <div class=\"meta\">Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm} &nbsp;|&nbsp; Программа: AirplaneManager</div>");
            sb.AppendLine("  </div>");

            // Сводная строка показателей
            sb.AppendLine("  <div class=\"summary-box\">");
            sb.AppendLine($"    <strong>Сводные показатели:</strong> Всего рейсов: {flights.Count} &nbsp;|&nbsp; " +
                          $"Пассажиропоток: {totalPassengers} чел. &nbsp;|&nbsp; " +
                          $"Воздушных судов в расписании: {uniquePlanes} &nbsp;|&nbsp; " +
                          $"Экипажи: {totalPilots} пилотов, {totalAttendants} бортпроводников");
            sb.AppendLine("  </div>");

            // Таблица расписания рейсов
            sb.AppendLine("  <table>");
            sb.AppendLine("    <thead>");
            sb.AppendLine("      <tr>");
            sb.AppendLine("        <th style=\"width: 40px;\">№</th>");
            sb.AppendLine("        <th style=\"width: 80px;\">Рейс</th>");
            sb.AppendLine("        <th style=\"width: 130px;\">Дата и время</th>");
            sb.AppendLine("        <th>Маршрут</th>");
            sb.AppendLine("        <th>Воздушное судно</th>");
            sb.AppendLine("        <th>Экипаж</th>");
            sb.AppendLine("        <th style=\"width: 90px; text-align: center;\">Пассажиры</th>");
            sb.AppendLine("      </tr>");
            sb.AppendLine("    </thead>");
            sb.AppendLine("    <tbody>");

            int idx = 1;
            foreach (var f in flights)
            {
                string flightNum = WebUtility.HtmlEncode(f.Route?.RouteNumber ?? $"FL-{f.Id}");
                string dateStr = f.FlightDate.ToString("dd.MM.yyyy HH:mm");
                string dep = f.Route?.DepartureAirport != null
                    ? $"{WebUtility.HtmlEncode(f.Route.DepartureAirport.City)} ({f.Route.DepartureAirport.Code})"
                    : "Не указан";
                string arr = f.Route?.ArrivalAirport != null
                    ? $"{WebUtility.HtmlEncode(f.Route.ArrivalAirport.City)} ({f.Route.ArrivalAirport.Code})"
                    : "Не указан";
                string routeStr = $"{dep} &rarr; {arr}";

                string planeStr = f.Airplane != null
                    ? $"{WebUtility.HtmlEncode(f.Airplane.Model)} (мест: {f.Airplane.Capacity})"
                    : "Не назначен";

                var crewParts = new List<string>();
                if (f.Pilots != null && f.Pilots.Count > 0)
                {
                    crewParts.Add("Пилоты: " + string.Join(", ", f.Pilots.Select(p => $"{p.LastName} {p.FirstName}")));
                }
                if (f.FlightAttendants != null && f.FlightAttendants.Count > 0)
                {
                    crewParts.Add("Проводники: " + string.Join(", ", f.FlightAttendants.Select(a => $"{a.LastName} {a.FirstName}")));
                }
                string crewStr = crewParts.Count > 0 ? WebUtility.HtmlEncode(string.Join("; ", crewParts)) : "—";
                int passCount = f.Passengers?.Count ?? 0;

                sb.AppendLine("      <tr>");
                sb.AppendLine($"        <td>{idx}</td>");
                sb.AppendLine($"        <td><strong>{flightNum}</strong></td>");
                sb.AppendLine($"        <td>{dateStr}</td>");
                sb.AppendLine($"        <td>{routeStr}</td>");
                sb.AppendLine($"        <td>{planeStr}</td>");
                sb.AppendLine($"        <td>{crewStr}</td>");
                sb.AppendLine($"        <td style=\"text-align: center;\">{passCount} чел.</td>");
                sb.AppendLine("      </tr>");
                idx++;
            }

            sb.AppendLine("    </tbody>");
            sb.AppendLine("  </table>");

            sb.AppendLine("  <div class=\"footer\">");
            sb.AppendLine($"    Информационная система AirplaneManager &bull; Лабораторная работа № 7 &bull; Всего записей: {flights.Count}");
            sb.AppendLine("  </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        // Сохранение HTML-отчета по рейсам в файл
        public static void GenerateFlightsHtmlToFile(List<Flight> flights, string outputPath, string title = "Отчет по расписанию рейсов")
        {
            var html = GenerateFlightsHtml(flights, title);
            File.WriteAllText(outputPath, html, Encoding.UTF8);
        }

        // Формирование строки HTML-отчета по отдельной таблице базы данных
        public static string GenerateGenericTableHtml(string tableName, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"ru\">");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset=\"utf-8\">");
            sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine($"  <title>{WebUtility.HtmlEncode(tableName)} — Отчет по таблице</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine(@"
    body {
      font-family: 'Times New Roman', Times, serif;
      margin: 40px;
      color: #111;
      background-color: #fff;
      font-size: 14px;
      line-height: 1.4;
    }
    .header {
      border-bottom: 2px solid #333;
      padding-bottom: 12px;
      margin-bottom: 20px;
    }
    h1 {
      font-size: 22px;
      margin: 0 0 6px 0;
      color: #222;
    }
    .meta {
      font-size: 13px;
      color: #555;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 15px;
    }
    th, td {
      border: 1px solid #888;
      padding: 8px 10px;
      text-align: left;
    }
    th {
      background-color: #f0f0f0;
      font-weight: bold;
      color: #222;
    }
    tr:nth-child(even) td {
      background-color: #fafafa;
    }
    .footer {
      margin-top: 30px;
      font-size: 12px;
      color: #777;
      text-align: right;
      border-top: 1px solid #ccc;
      padding-top: 8px;
    }
    @media print {
      body { margin: 10mm; }
      th, td { border: 1px solid #000; }
    }
  ");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine("  <div class=\"header\">");
            sb.AppendLine($"    <h1>Отчет по таблице «{WebUtility.HtmlEncode(tableName)}»</h1>");
            sb.AppendLine($"    <div class=\"meta\">Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm} &nbsp;|&nbsp; Всего записей: {rows.Count}</div>");
            sb.AppendLine("  </div>");

            sb.AppendLine("  <table>");
            sb.AppendLine("    <thead>");
            sb.AppendLine("      <tr>");
            foreach (var h in headers)
            {
                sb.AppendLine($"        <th>{WebUtility.HtmlEncode(h)}</th>");
            }
            sb.AppendLine("      </tr>");
            sb.AppendLine("    </thead>");
            sb.AppendLine("    <tbody>");

            foreach (var row in rows)
            {
                sb.AppendLine("      <tr>");
                foreach (var cell in row)
                {
                    sb.AppendLine($"        <td>{WebUtility.HtmlEncode(cell)}</td>");
                }
                sb.AppendLine("      </tr>");
            }

            sb.AppendLine("    </tbody>");
            sb.AppendLine("  </table>");

            sb.AppendLine("  <div class=\"footer\">");
            sb.AppendLine($"    Информационная система AirplaneManager &bull; Лабораторная работа № 7 &bull; Всего строк: {rows.Count}");
            sb.AppendLine("  </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        // Сохранение HTML-отчета по отдельной таблице в файл
        public static void GenerateGenericTableHtmlToFile(string tableName, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows, string outputPath)
        {
            var html = GenerateGenericTableHtml(tableName, headers, rows);
            File.WriteAllText(outputPath, html, Encoding.UTF8);
        }
    }
}
