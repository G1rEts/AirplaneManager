using AirplaneManager.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace AirplaneManager.Managers.Crud
{
    public static class AirplaneCrudManager
    {
        #region Методы работы с БД

        // Добавление нового самолета в базу данных
        public static void Add(Airplane airplane)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "INSERT INTO airplanes (model, capacity) VALUES (@model, @capacity);";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@model", airplane.Model);
                    cmd.Parameters.AddWithValue("@capacity", airplane.Capacity);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Получение полного списка всех самолетов
        public static List<Airplane> GetAll()
        {
            var airplanes = new List<Airplane>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, model, capacity FROM airplanes;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");
                            string model = reader.GetString("model");
                            int capacity = reader.GetInt32("capacity");

                            airplanes.Add(new Airplane(id, model, capacity));
                        }
                    }
                }
            }

            return airplanes;
        }

        // Поиск самолета по первичному ключу ID
        public static Airplane? GetById(int id)
        {
            Airplane? airplane = null;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, model, capacity FROM airplanes WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string model = reader.GetString("model");
                            int capacity = reader.GetInt32("capacity");

                            airplane = new Airplane(id, model, capacity);
                        }
                    }
                }
            }

            return airplane;
        }

        // Обновление данных самолета
        public static void Update(Airplane airplane)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "UPDATE airplanes SET model = @model, capacity = @capacity WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", airplane.Id);
                    cmd.Parameters.AddWithValue("@model", airplane.Model);
                    cmd.Parameters.AddWithValue("@capacity", airplane.Capacity);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Удаление самолета по ID
        public static void Delete(int airplaneId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "DELETE FROM airplanes WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", airplaneId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}