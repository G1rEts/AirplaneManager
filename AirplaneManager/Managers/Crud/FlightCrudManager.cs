using AirplaneManager.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace AirplaneManager.Managers.Crud
{
    public static class FlightCrudManager
    {
        public static void Add(Flight flight, List<int> pilotIds, List<int> attendantIds)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string sqlFlight = @"INSERT INTO flights (flight_date, route_id, airplane_id) 
                                     VALUES (@flightDate, @routeId, @airplaneId);
                                     SELECT LAST_INSERT_ID();";

                long flightId;
                using (var cmd = new MySqlCommand(sqlFlight, connection))
                {
                    cmd.Parameters.AddWithValue("@flightDate", flight.FlightDate);
                    cmd.Parameters.AddWithValue("@routeId", flight.Route.Id);

                    if (flight.Airplane != null)
                        cmd.Parameters.AddWithValue("@airplaneId", flight.Airplane.Id);
                    else
                        cmd.Parameters.AddWithValue("@airplaneId", DBNull.Value);

                    flightId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                // Сохраняем пассажиров
                if (flight.Passengers != null)
                {
                    foreach (var p in flight.Passengers)
                    {
                        using (var cmdPass = new MySqlCommand("INSERT INTO flight_passengers (flight_id, passenger_id) VALUES (@fId, @pId);", connection))
                        {
                            cmdPass.Parameters.AddWithValue("@fId", flightId);
                            cmdPass.Parameters.AddWithValue("@pId", p.Id);
                            cmdPass.ExecuteNonQuery();
                        }
                    }
                }

                // Сохраняем пилотов
                if (pilotIds != null)
                {
                    foreach (var pilotId in pilotIds)
                    {
                        using (var cmdPilot = new MySqlCommand("INSERT INTO flight_pilots (flight_id, pilot_id) VALUES (@fId, @pId);", connection))
                        {
                            cmdPilot.Parameters.AddWithValue("@fId", flightId);
                            cmdPilot.Parameters.AddWithValue("@pId", pilotId);
                            cmdPilot.ExecuteNonQuery();
                        }
                    }
                }

                // Сохраняем стюардесс
                if (attendantIds != null)
                {
                    foreach (var attId in attendantIds)
                    {
                        using (var cmdAtt = new MySqlCommand("INSERT INTO flight_flight_attendants (flight_id, flight_attendant_id) VALUES (@fId, @aId);", connection))
                        {
                            cmdAtt.Parameters.AddWithValue("@fId", flightId);
                            cmdAtt.Parameters.AddWithValue("@aId", attId);
                            cmdAtt.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static List<Flight> GetAll()
        {
            var flightsDict = new Dictionary<int, Flight>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = @"SELECT f.id, f.flight_date, 
                                      r.id AS route_id, r.route_number, r.flight_duration_hours,
                                      dep.id AS dep_id, dep.name AS dep_name, dep.code AS dep_code, dep.city AS dep_city, dep.country AS dep_country,
                                      arr.id AS arr_id, arr.name AS arr_name, arr.code AS arr_code, arr.city AS arr_city, arr.country AS arr_country,
                                      a.id AS airplane_id, a.model, a.capacity,
                                      p.id AS passenger_id, p.first_name AS p_fn, p.last_name AS p_ln, p.age AS p_age, p.passport AS p_pass, p.bonus AS p_bonus,
                                      pil.id AS pilot_id, pil.first_name AS pil_fn, pil.last_name AS pil_ln, pil.age AS pil_age, pil.passport AS pil_pass, pil.flight_hours, pil.license_number, pil.rang,
                                      att.id AS att_id, att.first_name AS att_fn, att.last_name AS att_ln, att.age AS att_age, att.passport AS att_pass, att.service
                               FROM flights f
                               JOIN routes r ON f.route_id = r.id
                               JOIN airports dep ON r.departure_airport_id = dep.id
                               JOIN airports arr ON r.arrival_airport_id = arr.id
                               LEFT JOIN airplanes a ON f.airplane_id = a.id
                               LEFT JOIN flight_passengers fp ON f.id = fp.flight_id
                               LEFT JOIN passengers p ON fp.passenger_id = p.id
                               LEFT JOIN flight_pilots fpi ON f.id = fpi.flight_id
                               LEFT JOIN pilots pil ON fpi.pilot_id = pil.id
                               LEFT JOIN flight_flight_attendants ffa ON f.id = ffa.flight_id
                               LEFT JOIN flight_attendants att ON ffa.flight_attendant_id = att.id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");

                            if (!flightsDict.ContainsKey(id))
                            {
                                DateTime flightDate = reader.GetDateTime("flight_date");

                                var depAirport = new Airport(reader.GetInt32("dep_id"), reader.GetString("dep_name"), reader.GetString("dep_code"), reader.GetString("dep_city"), reader.GetString("dep_country"));
                                var arrAirport = new Airport(reader.GetInt32("arr_id"), reader.GetString("arr_name"), reader.GetString("arr_code"), reader.GetString("arr_city"), reader.GetString("arr_country"));

                                var route = new Route(reader.GetInt32("route_id"), reader.GetString("route_number"), reader.GetDouble("flight_duration_hours"), depAirport, arrAirport);

                                Airplane? airplane = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("airplane_id")))
                                {
                                    airplane = new Airplane(reader.GetInt32("airplane_id"), reader.GetString("model"), reader.GetInt32("capacity"));
                                }

                                flightsDict[id] = new Flight(id, flightDate, route, airplane, new List<Passenger>(), new List<Pilot>(), new List<FlightAttendant>());
                            }

                            var flight = flightsDict[id];

                            // Добавляем пассажира, если есть и еще не добавлен
                            if (!reader.IsDBNull(reader.GetOrdinal("passenger_id")))
                            {
                                int pId = reader.GetInt32("passenger_id");
                                if (!flight.Passengers.Exists(x => x.Id == pId))
                                {
                                    flight.Passengers.Add(new Passenger(
                                        pId,
                                        reader.GetString("p_fn"),
                                        reader.GetString("p_ln"),
                                        reader.GetInt32("p_age"),
                                        reader.GetString("p_pass"),
                                        reader.GetInt32("p_bonus")
                                    ));
                                }
                            }

                            // Добавляем пилота, если есть и еще не добавлен
                            if (!reader.IsDBNull(reader.GetOrdinal("pilot_id")))
                            {
                                int pilId = reader.GetInt32("pilot_id");
                                if (!flight.Pilots.Exists(x => x.Id == pilId))
                                {
                                    flight.Pilots.Add(new Pilot(
                                        pilId,
                                        reader.GetString("pil_fn"),
                                        reader.GetString("pil_ln"),
                                        reader.GetInt32("pil_age"),
                                        reader.GetString("pil_pass"),
                                        reader.GetInt32("flight_hours"),
                                        reader.GetInt32("license_number"),
                                        reader.GetInt32("rang")
                                    ));
                                }
                            }

                            // Добавляем стюардессу, если есть и еще не добавлена
                            if (!reader.IsDBNull(reader.GetOrdinal("att_id")))
                            {
                                int attId = reader.GetInt32("att_id");
                                if (!flight.FlightAttendants.Exists(x => x.Id == attId))
                                {
                                    flight.FlightAttendants.Add(new FlightAttendant(
                                        attId,
                                        reader.GetString("att_fn"),
                                        reader.GetString("att_ln"),
                                        reader.GetInt32("att_age"),
                                        reader.GetString("att_pass"),
                                        reader.GetInt32("service")
                                    ));
                                }
                            }
                        }
                    }
                }
            }

            return new List<Flight>(flightsDict.Values);
        }

        public static Flight? GetById(int id)
        {
            var allFlights = GetAll();
            return allFlights.Find(f => f.Id == id);
        }

        public static void Update(Flight flight, List<int> pilotIds, List<int> attendantIds)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                // 1. Обновляем основные данные рейса
                string sql = @"UPDATE flights 
                               SET flight_date = @flightDate, route_id = @routeId, airplane_id = @airplaneId 
                               WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", flight.Id);
                    cmd.Parameters.AddWithValue("@flightDate", flight.FlightDate);
                    cmd.Parameters.AddWithValue("@routeId", flight.Route.Id);

                    if (flight.Airplane != null)
                        cmd.Parameters.AddWithValue("@airplaneId", flight.Airplane.Id);
                    else
                        cmd.Parameters.AddWithValue("@airplaneId", DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                // 2. Перезаписываем связи пассажиров
                new MySqlCommand($"DELETE FROM flight_passengers WHERE flight_id = {flight.Id};", connection).ExecuteNonQuery();
                if (flight.Passengers != null)
                {
                    foreach (var p in flight.Passengers)
                    {
                        using (var cmdPass = new MySqlCommand("INSERT INTO flight_passengers (flight_id, passenger_id) VALUES (@fId, @pId);", connection))
                        {
                            cmdPass.Parameters.AddWithValue("@fId", flight.Id);
                            cmdPass.Parameters.AddWithValue("@pId", p.Id);
                            cmdPass.ExecuteNonQuery();
                        }
                    }
                }

                // 3. Перезаписываем связи пилотов
                new MySqlCommand($"DELETE FROM flight_pilots WHERE flight_id = {flight.Id};", connection).ExecuteNonQuery();
                if (pilotIds != null)
                {
                    foreach (var pilotId in pilotIds)
                    {
                        using (var cmdPilot = new MySqlCommand("INSERT INTO flight_pilots (flight_id, pilot_id) VALUES (@fId, @pId);", connection))
                        {
                            cmdPilot.Parameters.AddWithValue("@fId", flight.Id);
                            cmdPilot.Parameters.AddWithValue("@pId", pilotId);
                            cmdPilot.ExecuteNonQuery();
                        }
                    }
                }

                // 4. Перезаписываем связи стюардесс
                new MySqlCommand($"DELETE FROM flight_flight_attendants WHERE flight_id = {flight.Id};", connection).ExecuteNonQuery();
                if (attendantIds != null)
                {
                    foreach (var attId in attendantIds)
                    {
                        using (var cmdAtt = new MySqlCommand("INSERT INTO flight_flight_attendants (flight_id, flight_attendant_id) VALUES (@fId, @aId);", connection))
                        {
                            cmdAtt.Parameters.AddWithValue("@fId", flight.Id);
                            cmdAtt.Parameters.AddWithValue("@aId", attId);
                            cmdAtt.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static void Delete(int flightId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                new MySqlCommand($"DELETE FROM flight_passengers WHERE flight_id = {flightId};", connection).ExecuteNonQuery();
                new MySqlCommand($"DELETE FROM flight_pilots WHERE flight_id = {flightId};", connection).ExecuteNonQuery();
                new MySqlCommand($"DELETE FROM flight_flight_attendants WHERE flight_id = {flightId};", connection).ExecuteNonQuery();
                new MySqlCommand($"DELETE FROM flights WHERE id = {flightId};", connection).ExecuteNonQuery();
            }
        }

        public static void PromptAndAdd()
        {
            Console.WriteLine("--- ДОБАВЛЕНИЕ РЕЙСА ---");
            Console.Write("Дата и время рейса (например, 2026-12-31 15:30:00): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime flightDate))
            {
                Console.WriteLine("\n[ОШИБКА] Неверный формат даты и времени!");
                return;
            }

            Console.Write("Введите ID маршрута: ");
            int routeId = int.Parse(Console.ReadLine() ?? "0");

            Console.Write("Введите ID самолета (можно пусто): ");
            string airplaneInput = Console.ReadLine() ?? "";
            Airplane? airplane = null;
            if (int.TryParse(airplaneInput, out int airplaneId))
            {
                airplane = AirplaneCrudManager.GetById(airplaneId);
            }

            var route = RouteCrudManager.GetById(routeId);
            if (route == null)
            {
                Console.WriteLine("\n[ОШИБКА] Маршрут не найден!");
                return;
            }

            Console.Write("Введите ID пассажиров через запятую (можно пусто): ");
            string passInput = Console.ReadLine() ?? "";
            List<Passenger> passengers = new List<Passenger>();
            foreach (var item in passInput.Split(','))
            {
                if (int.TryParse(item.Trim(), out int pId))
                {
                    var p = PassengerCrudManager.GetById(pId);
                    if (p != null) passengers.Add(p);
                }
            }

            Console.Write("Введите ID пилотов через запятую (можно пусто): ");
            string pilotInput = Console.ReadLine() ?? "";
            List<int> pilotIds = new List<int>();
            foreach (var item in pilotInput.Split(','))
            {
                if (int.TryParse(item.Trim(), out int piId)) pilotIds.Add(piId);
            }

            Console.Write("Введите ID стюардесс через запятую (можно пусто): ");
            string attInput = Console.ReadLine() ?? "";
            List<int> attendantIds = new List<int>();
            foreach (var item in attInput.Split(','))
            {
                if (int.TryParse(item.Trim(), out int atId)) attendantIds.Add(atId);
            }

            Flight flight = new Flight(flightDate, route, airplane, passengers);
            Add(flight, pilotIds, attendantIds);

            Console.WriteLine("\n[УСПЕХ] Рейс со всей командой успешно создан!");
        }
    }
}