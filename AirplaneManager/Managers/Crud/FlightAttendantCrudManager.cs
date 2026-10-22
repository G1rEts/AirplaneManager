using AirplaneManager.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace AirplaneManager.Managers.Crud
{
    public static class FlightAttendantCrudManager
    {
        #region Методы работы с БД 

        public static void Add(FlightAttendant attendant)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                // 1. Вставляем стюардессу и получаем её ID
                string sqlAttendant = @"INSERT INTO flight_attendants (first_name, last_name, age, passport, service) 
                                       VALUES (@firstName, @lastName, @age, @passport, @service);
                                       SELECT LAST_INSERT_ID();";

                long attendantId;
                using (var cmd = new MySqlCommand(sqlAttendant, connection))
                {
                    cmd.Parameters.AddWithValue("@firstName", attendant.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", attendant.LastName);
                    cmd.Parameters.AddWithValue("@age", attendant.Age);
                    cmd.Parameters.AddWithValue("@passport", attendant.Passport);
                    cmd.Parameters.AddWithValue("@service", attendant.Service);

                    attendantId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                // 2. Вставляем языки в связующую таблицу
                if (attendant.Languages != null && attendant.Languages.Count > 0)
                {
                    string sqlLanguage = @"INSERT INTO flight_attendant_languages (flight_attendant_id, language) 
                                           VALUES (@attendantId, @language);";

                    foreach (var lang in attendant.Languages)
                    {
                        using (var cmdLang = new MySqlCommand(sqlLanguage, connection))
                        {
                            cmdLang.Parameters.AddWithValue("@attendantId", attendantId);
                            cmdLang.Parameters.AddWithValue("@language", lang);
                            cmdLang.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static List<FlightAttendant> GetAll()
        {
            var attendants = new Dictionary<int, FlightAttendant>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                // Используем LEFT JOIN, чтобы забрать стюардесс и их языки за один запрос без конфликтов DataReader
                string sql = @"SELECT f.id, f.first_name, f.last_name, f.age, f.passport, f.service, l.language 
                               FROM flight_attendants f 
                               LEFT JOIN flight_attendant_languages l ON f.id = l.flight_attendant_id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");

                            if (!attendants.ContainsKey(id))
                            {
                                string firstName = reader.GetString("first_name");
                                string lastName = reader.GetString("last_name");
                                int age = reader.GetInt32("age");
                                string passport = reader.GetString("passport");
                                int service = reader.GetInt32("service");

                                attendants[id] = new FlightAttendant(id, firstName, lastName, age, passport, service, new List<string>());
                            }

                            // Если у стюардессы есть язык (не NULL в левом соединении)
                            if (!reader.IsDBNull(reader.GetOrdinal("language")))
                            {
                                string lang = reader.GetString("language");
                                attendants[id].Languages.Add(lang);
                            }
                        }
                    }
                }
            }

            return new List<FlightAttendant>(attendants.Values);
        }

        public static FlightAttendant? GetById(int id)
        {
            FlightAttendant? attendant = null;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string sql = @"SELECT f.id, f.first_name, f.last_name, f.age, f.passport, f.service, l.language 
                               FROM flight_attendants f 
                               LEFT JOIN flight_attendant_languages l ON f.id = l.flight_attendant_id 
                               WHERE f.id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (attendant == null)
                            {
                                string firstName = reader.GetString("first_name");
                                string lastName = reader.GetString("last_name");
                                int age = reader.GetInt32("age");
                                string passport = reader.GetString("passport");
                                int service = reader.GetInt32("service");

                                attendant = new FlightAttendant(id, firstName, lastName, age, passport, service, new List<string>());
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("language")))
                            {
                                string lang = reader.GetString("language");
                                attendant.Languages.Add(lang);
                            }
                        }
                    }
                }
            }

            return attendant;
        }

        public static void Update(FlightAttendant attendant)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                // Обновляем основные данные
                string sql = @"UPDATE flight_attendants 
                               SET first_name = @firstName, last_name = @lastName, 
                                   age = @age, passport = @passport, service = @service 
                               WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", attendant.Id);
                    cmd.Parameters.AddWithValue("@firstName", attendant.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", attendant.LastName);
                    cmd.Parameters.AddWithValue("@age", attendant.Age);
                    cmd.Parameters.AddWithValue("@passport", attendant.Passport);
                    cmd.Parameters.AddWithValue("@service", attendant.Service);

                    cmd.ExecuteNonQuery();
                }

                // Для языков проще всего: удалить старые и записать новые актуальные
                string sqlDeleteLangs = "DELETE FROM flight_attendant_languages WHERE flight_attendant_id = @id;";
                using (var cmdDel = new MySqlCommand(sqlDeleteLangs, connection))
                {
                    cmdDel.Parameters.AddWithValue("@id", attendant.Id);
                    cmdDel.ExecuteNonQuery();
                }

                if (attendant.Languages != null && attendant.Languages.Count > 0)
                {
                    string sqlInsertLang = @"INSERT INTO flight_attendant_languages (flight_attendant_id, language) 
                                             VALUES (@attendantId, @language);";

                    foreach (var lang in attendant.Languages)
                    {
                        using (var cmdLang = new MySqlCommand(sqlInsertLang, connection))
                        {
                            cmdLang.Parameters.AddWithValue("@attendantId", attendant.Id);
                            cmdLang.Parameters.AddWithValue("@language", lang);
                            cmdLang.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static void Delete(int attendantId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                // Сначала удаляем связанные языки
                string sqlDelLangs = "DELETE FROM flight_attendant_languages WHERE flight_attendant_id = @id;";
                using (var cmdLangs = new MySqlCommand(sqlDelLangs, connection))
                {
                    cmdLangs.Parameters.AddWithValue("@id", attendantId);
                    cmdLangs.ExecuteNonQuery();
                }

                // Затем саму стюардессу
                string sql = "DELETE FROM flight_attendants WHERE id = @id;";
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", attendantId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Вспомогательный метод для загрузки языков
        private static List<string> GetLanguagesForAttendant(int attendantId, MySqlConnection connection)
        {
            var languages = new List<string>();
            string sql = "SELECT language FROM flight_attendant_languages WHERE flight_attendant_id = @id;";

            using (var cmd = new MySqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@id", attendantId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        languages.Add(reader.GetString("language"));
                    }
                }
            }

            return languages;
        }

        #endregion
    }
}