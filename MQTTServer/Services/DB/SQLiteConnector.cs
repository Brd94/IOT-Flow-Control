using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace MQTTServer
{
    
    public class SQLiteConnector : ISQLConnector
    {
        private readonly string connectionString;
        SqliteConnection connection;
        public SQLiteConnector(string path)
        {
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

            connection = null;
        }

        public async Task<int> NonQueryAsync(string sql)
        {
            if (connection == null)
                await Connect();

            return await new SqliteCommand(sql, connection).ExecuteNonQueryAsync();
        }
        public async Task<object> ScalarQueryAsync(string sql)
        {
            if (connection == null)
                await Connect();

            return await new SqliteCommand(sql, connection).ExecuteScalarAsync();
        }

        public async Task<DataSet> QueryAsync(string sql)
        {
            if (connection == null)
                await Connect();

            using (var cmd = new SqliteCommand(sql, connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    DataSet dataSet = new DataSet();
                    DataTable dataTable = new DataTable();
                    dataSet.Tables.Add(dataTable);
                    dataTable.Load(reader);
                    return dataSet;
                }
            }
        }

        public DataSet Query(string sql)
        {

            if (connection == null)
                Connect().Wait();

            using (var cmd = new SqliteCommand(sql, connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    DataSet dataSet = new DataSet();
                    DataTable dataTable = new DataTable();
                    dataSet.Tables.Add(dataTable);
                    dataTable.Load(reader);
                    return dataSet;
                }
            }
        }

    }
}