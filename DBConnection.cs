using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace MerakiLocationVisualizer
{
    static class DBConnection
    {
        // DB connection string - SQL on network - CHANGE TO YOUR SERVER IP ADDRESS AND CREDENTIALS
        private static readonly string connectionString = "Persist Security Info = False; User ID = sa; Password = Password123;" +
                                                 "Initial Catalog = MerakiLocationVisualizer; Server = 172.30.254.10";
        // DB connection string - SQL on localhost
        //private static readonly string connectionString = "Persist Security Info = False; User ID = sa; Password = Password123;" +
        //                                         "Initial Catalog = MerakiLocationVisualizer; Server = (localdb)\\MSSQLLocalDB";

        internal static void UpdateDB(string queryString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Connection.Open();

                int returnedRows = command.ExecuteNonQuery();

                if (returnedRows > 0)
                {
                    //Console.WriteLine("SQL WRITE:{0} returned {1} rows.", queryString, returnedRows);
                }
                else
                {
                    Console.WriteLine("Update Failed");
                }
            }
        }

        internal static string[] RetrieveAPs()
        {
            string queryString = "SELECT apMac FROM ReportingAPs";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                List<string> ApMacs = new List<string>();

                SqlCommand command = new SqlCommand(queryString, connection);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ApMacs.Add(reader.GetString(0));
                    }
                    reader.Close();
                    return ApMacs.ToArray();
                }

                string[] x = new string[0];
                x[0] = "none";
                return x;
            }
        }

        // Retrieve observation reports made by specified access point
        internal static List<string> RetrieveReports(string ApMac)
        {
            string queryString = "SELECT seenByApMac, x, y, seenEpoch, ssid FROM ObservationReports " +
                                 "WHERE seenByApMac = '" + ApMac  + "' ORDER BY seenEpoch DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                List<string> observationReports = new List<string>();

                SqlCommand command = new SqlCommand(queryString, connection);
                command.Connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        observationReports.Add(reader.GetString(1)); // x
                        observationReports.Add(reader.GetString(2)); // y
                        observationReports.Add(reader.GetString(0)); // seenByAP
                        observationReports.Add((reader.GetInt32(3).ToString())); // seenEpoch 
                        observationReports.Add(reader.GetString(4)); // ssid
                    }
                    reader.Close();

                    return observationReports;  // x, y, ap, timestamp, ssid
                }

                return null;
            }
        }

        internal static bool DoesRecordExist(string queryString)
        {
            bool recordExists;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    recordExists = true;
                }
                else
                {
                    recordExists = false;
                }

                reader.Close();

                return recordExists;
            }
        }
    }
}
