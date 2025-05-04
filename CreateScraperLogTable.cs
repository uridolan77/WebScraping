using System;
using System.IO;
using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "Server=localhost;Database=webstraction_db;User=root;Password=Dt%g_9W3z0*!I;";
        string sqlScript = File.ReadAllText("create_scraperlog_table.sql");

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                Console.WriteLine("Connected to MySQL database.");

                using (MySqlCommand command = new MySqlCommand(sqlScript, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("SQL script executed successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}
