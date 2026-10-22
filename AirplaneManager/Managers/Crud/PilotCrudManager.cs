using AirplaneManager.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace AirplaneManager.Managers.Crud
{
    public static class PilotCrudManager
    {
        #region Методы работы с БД

        public static void Add(Pilot pilot)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = @"INSERT INTO pilots (first_name, last_name, age, passport, flight_hours, license_number, rang) 
                               VALUES (@firstName, @lastName, @age, @passport, @flightHours, @licenseNumber, @rang);";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@firstName", pilot.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", pilot.LastName);
                    cmd.Parameters.AddWithValue("@age", pilot.Age);
                    cmd.Parameters.AddWithValue("@passport", pilot.Passport);
                    cmd.Parameters.AddWithValue("@flightHours", pilot.FlightHours);
                    cmd.Parameters.AddWithValue("@licenseNumber", pilot.LicenseNumber);
                    cmd.Parameters.AddWithValue("@rang", pilot.Rang);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<Pilot> GetAll()
        {
            var pilots = new List<Pilot>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, first_name, last_name, age, passport, flight_hours, license_number, rang FROM pilots;";

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
                            int flightHours = reader.GetInt32("flight_hours");
                            int licenseNumber = reader.GetInt32("license_number");
                            int rang = reader.GetInt32("rang");

                            pilots.Add(new Pilot(id, firstName, lastName, age, passport, flightHours, licenseNumber, rang));
                        }
                    }
                }
            }

            return pilots;
        }

        public static Pilot? GetById(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, first_name, last_name, age, passport, flight_hours, license_number, rang FROM pilots WHERE id = @id;";

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
                            int flightHours = reader.GetInt32("flight_hours");
                            int licenseNumber = reader.GetInt32("license_number");
                            int rang = reader.GetInt32("rang");

                            return new Pilot(id, firstName, lastName, age, passport, flightHours, licenseNumber, rang);
                        }
                    }
                }
            }

            return null;
        }

        public static void Update(Pilot pilot)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = @"UPDATE pilots 
                               SET first_name = @firstName, last_name = @lastName, 
                                   age = @age, passport = @passport, 
                                   flight_hours = @flightHours, license_number = @licenseNumber, rang = @rang 
                               WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", pilot.Id);
                    cmd.Parameters.AddWithValue("@firstName", pilot.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", pilot.LastName);
                    cmd.Parameters.AddWithValue("@age", pilot.Age);
                    cmd.Parameters.AddWithValue("@passport", pilot.Passport);
                    cmd.Parameters.AddWithValue("@flightHours", pilot.FlightHours);
                    cmd.Parameters.AddWithValue("@licenseNumber", pilot.LicenseNumber);
                    cmd.Parameters.AddWithValue("@rang", pilot.Rang);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void Delete(int pilotId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = "DELETE FROM pilots WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", pilotId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}