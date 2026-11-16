using AirplaneManager.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace AirplaneManager.Managers.Crud
{
    public static class AirportCrudManager
    {
        #region Методы работы с БД

        // Добавление нового аэропорта в базу данных
        public static void Add(Airport airport)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "INSERT INTO airports (name, code, city, country) VALUES (@name, @code, @city, @country);";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@name", airport.Name);
                    cmd.Parameters.AddWithValue("@code", airport.Code);
                    cmd.Parameters.AddWithValue("@city", airport.City);
                    cmd.Parameters.AddWithValue("@country", airport.Country);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Получение полного списка всех аэропортов
        public static List<Airport> GetAll()
        {
            var airports = new List<Airport>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, name, code, city, country FROM airports;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");
                            string name = reader.GetString("name");
                            string code = reader.GetString("code");
                            string city = reader.GetString("city");
                            string country = reader.GetString("country");

                            airports.Add(new Airport(id, name, code, city, country));
                        }
                    }
                }
            }

            return airports;
        }

        // Поиск аэропорта по первичному ключу ID
        public static Airport? GetById(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, name, code, city, country FROM airports WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string name = reader.GetString("name");
                            string code = reader.GetString("code");
                            string city = reader.GetString("city");
                            string country = reader.GetString("country");

                            return new Airport(id, name, code, city, country);
                        }
                    }
                }
            }

            return null;
        }

        // Обновление данных существующего аэропорта
        public static void Update(Airport airport)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "UPDATE airports SET name = @name, code = @code, city = @city, country = @country WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", airport.Id);
                    cmd.Parameters.AddWithValue("@name", airport.Name);
                    cmd.Parameters.AddWithValue("@code", airport.Code);
                    cmd.Parameters.AddWithValue("@city", airport.City);
                    cmd.Parameters.AddWithValue("@country", airport.Country);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Удаление аэропорта по ID
        public static void Delete(int airportId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "DELETE FROM airports WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", airportId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}