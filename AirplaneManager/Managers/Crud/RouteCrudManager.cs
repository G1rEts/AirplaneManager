using AirplaneManager.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace AirplaneManager.Managers.Crud
{
    public static class RouteCrudManager
    {
        #region Методы работы с БД

        public static void Add(Route route)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = @"INSERT INTO routes (route_number, flight_duration_hours, departure_airport_id, arrival_airport_id) 
                               VALUES (@routeNumber, @duration, @depId, @arrId);";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@routeNumber", route.RouteNumber);
                    cmd.Parameters.AddWithValue("@duration", route.FlightDurationHours);
                    cmd.Parameters.AddWithValue("@depId", route.DepartureAirport.Id);
                    cmd.Parameters.AddWithValue("@arrId", route.ArrivalAirport.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<Route> GetAll()
        {
            var routes = new List<Route>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = @"SELECT r.id, r.route_number, r.flight_duration_hours, 
                                      dep.id AS dep_id, dep.name AS dep_name, dep.code AS dep_code, dep.city AS dep_city, dep.country AS dep_country,
                                      arr.id AS arr_id, arr.name AS arr_name, arr.code AS arr_code, arr.city AS arr_city, arr.country AS arr_country
                               FROM routes r
                               JOIN airports dep ON r.departure_airport_id = dep.id
                               JOIN airports arr ON r.arrival_airport_id = arr.id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");
                            string routeNumber = reader.GetString("route_number");
                            double duration = reader.GetDouble("flight_duration_hours");

                            var depAirport = new Airport(
                                reader.GetInt32("dep_id"),
                                reader.GetString("dep_name"),
                                reader.GetString("dep_code"),
                                reader.GetString("dep_city"),
                                reader.GetString("dep_country")
                            );

                            var arrAirport = new Airport(
                                reader.GetInt32("arr_id"),
                                reader.GetString("arr_name"),
                                reader.GetString("arr_code"),
                                reader.GetString("arr_city"),
                                reader.GetString("arr_country")
                            );

                            routes.Add(new Route(id, routeNumber, duration, depAirport, arrAirport));
                        }
                    }
                }
            }

            return routes;
        }

        public static Route? GetById(int id)
        {
            Route? route = null;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = @"SELECT r.id, r.route_number, r.flight_duration_hours, 
                                      dep.id AS dep_id, dep.name AS dep_name, dep.code AS dep_code, dep.city AS dep_city, dep.country AS dep_country,
                                      arr.id AS arr_id, arr.name AS arr_name, arr.code AS arr_code, arr.city AS arr_city, arr.country AS arr_country
                               FROM routes r
                               JOIN airports dep ON r.departure_airport_id = dep.id
                               JOIN airports arr ON r.arrival_airport_id = arr.id
                               WHERE r.id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string routeNumber = reader.GetString("route_number");
                            double duration = reader.GetDouble("flight_duration_hours");

                            var depAirport = new Airport(
                                reader.GetInt32("dep_id"),
                                reader.GetString("dep_name"),
                                reader.GetString("dep_code"),
                                reader.GetString("dep_city"),
                                reader.GetString("dep_country")
                            );

                            var arrAirport = new Airport(
                                reader.GetInt32("arr_id"),
                                reader.GetString("arr_name"),
                                reader.GetString("arr_code"),
                                reader.GetString("arr_city"),
                                reader.GetString("arr_country")
                            );

                            route = new Route(id, routeNumber, duration, depAirport, arrAirport);
                        }
                    }
                }
            }

            return route;
        }

        public static void Update(Route route)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = @"UPDATE routes 
                               SET route_number = @routeNumber, flight_duration_hours = @duration, 
                                   departure_airport_id = @depId, arrival_airport_id = @arrId 
                               WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", route.Id);
                    cmd.Parameters.AddWithValue("@routeNumber", route.RouteNumber);
                    cmd.Parameters.AddWithValue("@duration", route.FlightDurationHours);
                    cmd.Parameters.AddWithValue("@depId", route.DepartureAirport.Id);
                    cmd.Parameters.AddWithValue("@arrId", route.ArrivalAirport.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void Delete(int routeId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "DELETE FROM routes WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", routeId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}