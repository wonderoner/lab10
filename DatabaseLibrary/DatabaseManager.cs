using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;

namespace DatabaseLibrary
{
    public class ForumMessage
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class DatabaseManager : IDisposable
    {
        private SqliteConnection _connection;
        private readonly string _connectionString;

        public DatabaseManager(string databasePath)
        {
            _connectionString = $"Data Source={databasePath}";
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
            CreateTable();
        }

        private void CreateTable()
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS ForumMessages (
                    ID INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Message TEXT
                )";

            using (var command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public ForumMessage? GetByID(int id)
        {
            string sql = "SELECT * FROM ForumMessages WHERE ID = @ID";

            using (var command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@ID", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new ForumMessage
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            Name = reader["Name"]?.ToString() ?? string.Empty,
                            Message = reader["Message"]?.ToString() ?? string.Empty
                        };
                    }
                }
            }

            return null;
        }

        public List<ForumMessage> GetByName(string name)
        {
            var messages = new List<ForumMessage>();
            string sql = "SELECT * FROM ForumMessages WHERE Name = @Name";

            using (var command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Name", name);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        messages.Add(new ForumMessage
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            Name = reader["Name"]?.ToString() ?? string.Empty,
                            Message = reader["Message"]?.ToString() ?? string.Empty
                        });
                    }
                }
            }

            return messages;
        }

        public void Add(int id, string name, string message)
        {
            string sql = "INSERT OR REPLACE INTO ForumMessages (ID, Name, Message) VALUES (@ID, @Name, @Message)";

            using (var command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@ID", id);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Message", message);
                command.ExecuteNonQuery();
            }
        }

        public void Update(int id, string newMessage)
        {
            string sql = "UPDATE ForumMessages SET Message = @Message WHERE ID = @ID";

            using (var command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Message", newMessage);
                command.Parameters.AddWithValue("@ID", id);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM ForumMessages WHERE ID = @ID";

            using (var command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@ID", id);
                command.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}