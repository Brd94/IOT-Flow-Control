using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace MQTTServer
{
    public class SQLiteConnector
    {
        private readonly string connectionString;
        SqliteConnection connection;
        public SQLiteConnector(string path)
        {
            BuildFile(path);

            var builder = new SqliteConnectionStringBuilder();
            builder.DataSource = path;
            this.connectionString = builder.ConnectionString;

        }

        public async Task Connect()
        {
            await Disconnect();

            connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
        }

        public async Task Disconnect()
        {
            if (connection != null)
                await connection.CloseAsync();
        }

        public void BuildFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileLoadException("File .db non trovato");
            }
        }

        public async Task<int> NonQuery(string sql) => await new SqliteCommand(sql, connection).ExecuteNonQueryAsync();
        public async Task<object> ScalarQuery(string sql) => await new SqliteCommand(sql, connection).ExecuteScalarAsync();

        public async Task<DataTable> Query(string sql)
        {
            using (var cmd = new SqliteCommand(sql, connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var table = new DataTable();
                    table.Load(reader);
                    return table;
                }
            }
        }

    }
}