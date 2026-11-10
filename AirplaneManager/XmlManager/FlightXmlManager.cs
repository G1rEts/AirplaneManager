using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AirplaneManager.Managers;
using AirplaneManager.Managers.Crud;
using AirplaneManager.Models;
using MySql.Data.MySqlClient;

namespace AirplaneManager.XmlManager
{
    /// <summary>
    /// Менеджер экспорта и импорта рейсов в формате XML.
    /// </summary>
    public static class FlightXmlManager
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm";

        /// <summary>
        /// Экспорт списка рейсов в XML-файл.
        /// </summary>
        public static void ExportFlights(string filePath, List<Flight> flights)
        {
            var root = new XElement("flights");

            foreach (var f in flights)
            {
                var flightElem = new XElement("flight",
                    new XAttribute("id", f.Id),
                    new XAttribute("date", f.FlightDate.ToString(DateFormat, CultureInfo.InvariantCulture))
                );

                // Маршрут и аэропорты
                if (f.Route != null)
                {
                    var routeElem = new XElement("route",
                        new XAttribute("id", f.Route.Id),
                        new XAttribute("number", f.Route.RouteNumber),
                        new XAttribute("duration", f.Route.FlightDurationHours.ToString(CultureInfo.InvariantCulture))
                    );

                    if (f.Route.DepartureAirport != null)
                    {
                        routeElem.Add(new XElement("departure",
                            new XAttribute("id", f.Route.DepartureAirport.Id),
                            new XAttribute("code", f.Route.DepartureAirport.Code),
                            new XAttribute("name", f.Route.DepartureAirport.Name),
                            new XAttribute("city", f.Route.DepartureAirport.City),
                            new XAttribute("country", f.Route.DepartureAirport.Country)
                        ));
                    }

                    if (f.Route.ArrivalAirport != null)
                    {
                        routeElem.Add(new XElement("arrival",
                            new XAttribute("id", f.Route.ArrivalAirport.Id),
                            new XAttribute("code", f.Route.ArrivalAirport.Code),
                            new XAttribute("name", f.Route.ArrivalAirport.Name),
                            new XAttribute("city", f.Route.ArrivalAirport.City),
                            new XAttribute("country", f.Route.ArrivalAirport.Country)
                        ));
                    }

                    flightElem.Add(routeElem);
                }

                // Самолет
                if (f.Airplane != null)
                {
                    flightElem.Add(new XElement("airplane",
                        new XAttribute("id", f.Airplane.Id),
                        new XAttribute("model", f.Airplane.Model),
                        new XAttribute("capacity", f.Airplane.Capacity)
                    ));
                }

                // Пилоты
                var pilotsElem = new XElement("pilots");
                foreach (var p in f.Pilots)
                {
                    pilotsElem.Add(new XElement("pilot",
                        new XAttribute("id", p.Id),
                        new XAttribute("firstName", p.FirstName),
                        new XAttribute("lastName", p.LastName),
                        new XAttribute("age", p.Age),
                        new XAttribute("passport", p.Passport),
                        new XAttribute("flightHours", p.FlightHours),
                        new XAttribute("license", p.LicenseNumber),
                        new XAttribute("rang", p.Rang)
                    ));
                }
                flightElem.Add(pilotsElem);

                // Бортпроводники
                var attElem = new XElement("flightAttendants");
                foreach (var a in f.FlightAttendants)
                {
                    attElem.Add(new XElement("attendant",
                        new XAttribute("id", a.Id),
                        new XAttribute("firstName", a.FirstName),
                        new XAttribute("lastName", a.LastName),
                        new XAttribute("age", a.Age),
                        new XAttribute("passport", a.Passport),
                        new XAttribute("service", a.Service),
                        new XAttribute("languages", string.Join(", ", a.Languages))
                    ));
                }
                flightElem.Add(attElem);

                // Пассажиры
                var passElem = new XElement("passengers");
                foreach (var pass in f.Passengers)
                {
                    passElem.Add(new XElement("passenger",
                        new XAttribute("id", pass.Id),
                        new XAttribute("firstName", pass.FirstName),
                        new XAttribute("lastName", pass.LastName),
                        new XAttribute("age", pass.Age),
                        new XAttribute("passport", pass.Passport),
                        new XAttribute("bonus", pass.Bonus)
                    ));
                }
                flightElem.Add(passElem);

                root.Add(flightElem);
            }

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
            doc.Save(filePath);
        }

        /// <summary>
        /// Импорт списка рейсов из XML-файла в память.
        /// </summary>
        public static List<Flight> ImportFlights(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Указанный XML-файл не найден.", filePath);

            var doc = XDocument.Load(filePath);
            var result = new List<Flight>();

            var flightNodes = doc.Root?.Elements("flight") ?? Enumerable.Empty<XElement>();

            foreach (var fNode in flightNodes)
            {
                int flightId = (int?)fNode.Attribute("id") ?? 0;
                string dateStr = (string?)fNode.Attribute("date") ?? "";
                DateTime flightDate = DateTime.TryParseExact(dateStr, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                    ? dt
                    : DateTime.Now;

                // Парсинг маршрута
                Route route = null!;
                var routeNode = fNode.Element("route");
                if (routeNode != null)
                {
                    int routeId = (int?)routeNode.Attribute("id") ?? 0;
                    string routeNum = (string?)routeNode.Attribute("number") ?? "Б/Н";
                    double duration = double.TryParse((string?)routeNode.Attribute("duration"), NumberStyles.Any, CultureInfo.InvariantCulture, out var dur) ? dur : 1.0;

                    Airport dep = ParseAirport(routeNode.Element("departure"));
                    Airport arr = ParseAirport(routeNode.Element("arrival"));

                    route = new Route(routeId, routeNum, duration, dep, arr);
                }

                // Парсинг самолета
                Airplane? airplane = null;
                var planeNode = fNode.Element("airplane");
                if (planeNode != null)
                {
                    int planeId = (int?)planeNode.Attribute("id") ?? 0;
                    string model = (string?)planeNode.Attribute("model") ?? "Неизвестно";
                    int capacity = (int?)planeNode.Attribute("capacity") ?? 100;
                    airplane = new Airplane(planeId, model, capacity);
                }

                // Парсинг пилотов
                var pilots = new List<Pilot>();
                var pilotsNode = fNode.Element("pilots");
                if (pilotsNode != null)
                {
                    foreach (var pNode in pilotsNode.Elements("pilot"))
                    {
                        pilots.Add(new Pilot(
                            (int?)pNode.Attribute("id") ?? 0,
                            (string?)pNode.Attribute("firstName") ?? "",
                            (string?)pNode.Attribute("lastName") ?? "",
                            (int?)pNode.Attribute("age") ?? 30,
                            (string?)pNode.Attribute("passport") ?? "",
                            (int?)pNode.Attribute("flightHours") ?? 0,
                            (int?)pNode.Attribute("license") ?? 100000,
                            (int?)pNode.Attribute("rang") ?? 1
                        ));
                    }
                }

                // Парсинг бортпроводников
                var attendants = new List<FlightAttendant>();
                var attNode = fNode.Element("flightAttendants");
                if (attNode != null)
                {
                    foreach (var aNode in attNode.Elements("attendant"))
                    {
                        var langs = ((string?)aNode.Attribute("languages") ?? "")
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim()).ToList();

                        attendants.Add(new FlightAttendant(
                            (int?)aNode.Attribute("id") ?? 0,
                            (string?)aNode.Attribute("firstName") ?? "",
                            (string?)aNode.Attribute("lastName") ?? "",
                            (int?)aNode.Attribute("age") ?? 25,
                            (string?)aNode.Attribute("passport") ?? "",
                            (int?)aNode.Attribute("service") ?? 1,
                            langs
                        ));
                    }
                }

                // Парсинг пассажиров
                var passengers = new List<Passenger>();
                var passNode = fNode.Element("passengers");
                if (passNode != null)
                {
                    foreach (var psNode in passNode.Elements("passenger"))
                    {
                        passengers.Add(new Passenger(
                            (int?)psNode.Attribute("id") ?? 0,
                            (string?)psNode.Attribute("firstName") ?? "",
                            (string?)psNode.Attribute("lastName") ?? "",
                            (int?)psNode.Attribute("age") ?? 30,
                            (string?)psNode.Attribute("passport") ?? "",
                            (int?)psNode.Attribute("bonus") ?? 0
                        ));
                    }
                }

                result.Add(new Flight(flightId, flightDate, route, airplane, passengers, pilots, attendants));
            }

            return result;
        }

        /// <summary>
        /// Импорт списка рейсов из XML-файла напрямую в базу данных MySQL.
        /// </summary>
        public static int ImportFlightsToDatabase(string filePath, bool clearExisting = false)
        {
            var flights = ImportFlights(filePath);
            if (flights.Count == 0) return 0;

            var existingRoutes = RouteCrudManager.GetAll();
            var existingPlanes = AirplaneCrudManager.GetAll();
            var existingPilots = PilotCrudManager.GetAll();
            var existingAttendants = FlightAttendantCrudManager.GetAll();
            var existingPassengers = PassengerCrudManager.GetAll();

            if (clearExisting)
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    new MySqlCommand("DELETE FROM flight_passengers;", conn).ExecuteNonQuery();
                    new MySqlCommand("DELETE FROM flight_pilots;", conn).ExecuteNonQuery();
                    new MySqlCommand("DELETE FROM flight_flight_attendants;", conn).ExecuteNonQuery();
                    new MySqlCommand("DELETE FROM flights;", conn).ExecuteNonQuery();
                }
            }

            int insertedCount = 0;
            foreach (var flight in flights)
            {
                // 1. Привязка маршрута
                Route? dbRoute = null;
                if (flight.Route != null)
                {
                    dbRoute = existingRoutes.Find(r => r.Id == flight.Route.Id)
                           ?? existingRoutes.Find(r => string.Equals(r.RouteNumber, flight.Route.RouteNumber, StringComparison.OrdinalIgnoreCase));
                }
                if (dbRoute == null)
                    dbRoute = existingRoutes.FirstOrDefault();

                if (dbRoute == null)
                    throw new InvalidOperationException("В базе данных нет ни одного маршрута для привязки рейса.");

                flight.Route = dbRoute;

                // 2. Привязка самолета
                if (flight.Airplane != null)
                {
                    flight.Airplane = existingPlanes.Find(p => p.Id == flight.Airplane.Id)
                                   ?? existingPlanes.Find(p => string.Equals(p.Model, flight.Airplane.Model, StringComparison.OrdinalIgnoreCase));
                }

                // 3. Привязка пилотов
                var pilotIds = new List<int>();
                foreach (var p in flight.Pilots)
                {
                    var dbPilot = existingPilots.Find(x => x.Id == p.Id)
                               ?? existingPilots.Find(x => x.LicenseNumber == p.LicenseNumber)
                               ?? existingPilots.Find(x => x.LastName == p.LastName && x.FirstName == p.FirstName);
                    if (dbPilot != null && !pilotIds.Contains(dbPilot.Id))
                        pilotIds.Add(dbPilot.Id);
                }

                // 4. Привязка бортпроводников
                var attendantIds = new List<int>();
                foreach (var a in flight.FlightAttendants)
                {
                    var dbAtt = existingAttendants.Find(x => x.Id == a.Id)
                             ?? existingAttendants.Find(x => x.LastName == a.LastName && x.FirstName == a.FirstName);
                    if (dbAtt != null && !attendantIds.Contains(dbAtt.Id))
                        attendantIds.Add(dbAtt.Id);
                }

                // 5. Привязка пассажиров
                var dbPassengers = new List<Passenger>();
                foreach (var pass in flight.Passengers)
                {
                    var dbPass = existingPassengers.Find(x => x.Id == pass.Id)
                              ?? existingPassengers.Find(x => x.Passport == pass.Passport)
                              ?? existingPassengers.Find(x => x.LastName == pass.LastName && x.FirstName == pass.FirstName);
                    if (dbPass != null && !dbPassengers.Exists(x => x.Id == dbPass.Id))
                        dbPassengers.Add(dbPass);
                }
                flight.Passengers = dbPassengers;

                // Сохранение рейса в БД через CrudManager
                FlightCrudManager.Add(flight, pilotIds, attendantIds);
                insertedCount++;
            }

            return insertedCount;
        }

        private static Airport ParseAirport(XElement? node)
        {
            if (node == null) return new Airport(0, "Неизвестный", "UNK", "Город", "Страна");

            return new Airport(
                (int?)node.Attribute("id") ?? 0,
                (string?)node.Attribute("name") ?? "",
                (string?)node.Attribute("code") ?? "UNK",
                (string?)node.Attribute("city") ?? "",
                (string?)node.Attribute("country") ?? ""
            );
        }
    }
}
