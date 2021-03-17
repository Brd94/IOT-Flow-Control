using System;
using System.Collections.Generic;
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

        public async Task<int> NonQueryAsync(FormattableString formattedsql)
        {
            var queryArgs = formattedsql.GetArguments();
            var sqliteParams = new List<SqliteParameter>();

            for (int i = 0; i < queryArgs.Length; i++)
            {
                var param = new SqliteParameter(i.ToString(), queryArgs[i]);
                sqliteParams.Add(param);
                queryArgs[i] = "@" + i;
            }

            string sql = formattedsql.ToString();

            if (connection == null)
                await Connect();

            using (var cmd = new SqliteCommand(sql, connection))
            {
                cmd.Parameters.AddRange(sqliteParams);

                return await cmd.ExecuteNonQueryAsync();
            }
        }
        public async Task<object> ScalarQueryAsync(FormattableString formattedsql)
        {
            var queryArgs = formattedsql.GetArguments();
            var sqliteParams = new List<SqliteParameter>();

            for (int i = 0; i < queryArgs.Length; i++)
            {
                var param = new SqliteParameter(i.ToString(), queryArgs[i]);
                sqliteParams.Add(param);
                queryArgs[i] = "@" + i;
            }

            string sql = formattedsql.ToString();

            if (connection == null)
                await Connect();

            using (var cmd = new SqliteCommand(sql, connection))
            {
                cmd.Parameters.AddRange(sqliteParams);

                return await cmd.ExecuteScalarAsync();
            }
        }

        public async Task<DataSet> QueryAsync(FormattableString formattedsql)
        {

            var queryArgs = formattedsql.GetArguments();
            var sqliteParams = new List<SqliteParameter>();

            for (int i = 0; i < queryArgs.Length; i++)
            {
                var param = new SqliteParameter(i.ToString(), queryArgs[i]);
                sqliteParams.Add(param);
                queryArgs[i] = "@" + i;
            }

            string sql = formattedsql.ToString();

            if (connection == null)
                await Connect();

            using (var cmd = new SqliteCommand(sql, connection))
            {

                cmd.Parameters.AddRange(sqliteParams);


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

        public DataSet Query(FormattableString formattedsql)
        {

            var queryArgs = formattedsql.GetArguments();
            var sqliteParams = new List<SqliteParameter>();

            //Aggiungere riferimento : Moreno Gentili - Programmazione CS

            for (int i = 0; i < queryArgs.Length; i++)
            {
                var param = new SqliteParameter(i.ToString(), queryArgs[i]);
                sqliteParams.Add(param);
                queryArgs[i] = "@" + i;
            }

            string sql = formattedsql.ToString();

            if (connection == null)
                Connect().Wait();

            using (var cmd = new SqliteCommand(sql, connection))
            {

                cmd.Parameters.AddRange(sqliteParams);

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