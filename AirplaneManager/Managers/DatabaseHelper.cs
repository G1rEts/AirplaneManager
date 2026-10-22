using AirplaneManager.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirplaneManager.Managers
{
    public static class DatabaseHelper
    {
        private static string _connectionString = "Server=localhost;Database=db_airplanemanager;Uid=root;Pwd=idcbfmj#h)ch*^qj=7KaCabg;";

        public static bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
