using System;
using System.IO;
using MySql.Data.MySqlClient;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Server=localhost;Database=webstraction_db;User=root;Password=Dt%g_9W3z0*!I;";
        string scriptPath = "create_missing_tables.sql";

        try
        {
            // Read the SQL script
            string script = File.ReadAllText(scriptPath);

            // Execute the script
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                
                // Split the script by semicolons to execute each statement separately
                string[] commands = script.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string command in commands)
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        using (MySqlCommand cmd = new MySqlCommand(command, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                
                Console.WriteLine("SQL script executed successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing SQL script: {ex.Message}");
        }
    }
}
