using AirplaneManager.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace AirplaneManager.Managers.Crud
{
    public static class PassengerCrudManager
    {
        #region Методы работы с БД

        public static void Add(Passenger passenger)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = @"INSERT INTO passengers (first_name, last_name, age, passport, bonus) 
                               VALUES (@firstName, @lastName, @age, @passport, @bonus);";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@firstName", passenger.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", passenger.LastName);
                    cmd.Parameters.AddWithValue("@age", passenger.Age);
                    cmd.Parameters.AddWithValue("@passport", passenger.Passport);
                    cmd.Parameters.AddWithValue("@bonus", passenger.Bonus);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<Passenger> GetAll()
        {
            var passengers = new List<Passenger>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, first_name, last_name, age, passport, bonus FROM passengers;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");
                            string firstName = reader.GetString("first_name");
                            string lastName = reader.GetString("last_name");
                            int age = reader.GetInt32("age");
                            string passport = reader.GetString("passport");
                            int bonus = reader.GetInt32("bonus");

                            passengers.Add(new Passenger(id, firstName, lastName, age, passport, bonus));
                        }
                    }
                }
            }

            return passengers;
        }

        public static Passenger? GetById(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, first_name, last_name, age, passport, bonus FROM passengers WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string firstName = reader.GetString("first_name");
                            string lastName = reader.GetString("last_name");
                            int age = reader.GetInt32("age");
                            string passport = reader.GetString("passport");
                            int bonus = reader.GetInt32("bonus");

                            return new Passenger(id, firstName, lastName, age, passport, bonus);
                        }
                    }
                }
            }

            return null;
        }

        public static void Update(Passenger passenger)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = @"UPDATE passengers 
                               SET first_name = @firstName, last_name = @lastName, 
                                   age = @age, passport = @passport, bonus = @bonus 
                               WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", passenger.Id);
                    cmd.Parameters.AddWithValue("@firstName", passenger.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", passenger.LastName);
                    cmd.Parameters.AddWithValue("@age", passenger.Age);
                    cmd.Parameters.AddWithValue("@passport", passenger.Passport);
                    cmd.Parameters.AddWithValue("@bonus", passenger.Bonus);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void Delete(int passengerId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "DELETE FROM passengers WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", passengerId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}