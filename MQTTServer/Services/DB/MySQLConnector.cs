using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MQTTServer
{

    public class MySQLConnector : ISQLConnector
    {
        private readonly string connectionString;
        MySqlConnection connection;
        public MySQLConnector(string server,string database,string uid,string password)
        {
            var builder = new MySqlConnectionStringBuilder();
            builder.Server = server;
            builder.Database = database;
            builder.UserID = uid;
            builder.Password = password;
            builder.Port = 3306;

            this.connectionString = builder.ConnectionString;

        }

        public async Task Connect()
        {
            await Disconnect();

            connection = new MySqlConnection(connectionString);
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
            var mysqlParams = new MySqlParameter[queryArgs.Length];

            for (int i = 0; i < queryArgs.Length; i++)
            {
                var param = new MySqlParameter(i.ToString(), queryArgs[i]);
                mysqlParams[i] = param;
                queryArgs[i] = "@" + i;
            }

            string sql = formattedsql.ToString();

            if (connection == null)
                await Connect();

            using (var cmd = new MySqlCommand(sql, connection))
            {
                cmd.Parameters.AddRange(mysqlParams);

                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<object> ScalarQueryAsync(FormattableString formattedsql)
        {
            var queryArgs = formattedsql.GetArguments();
            var mysqlParams = new MySqlParameter[queryArgs.Length];

            for (int i = 0; i < queryArgs.Length; i++)
            {
                var param = new MySqlParameter(i.ToString(), queryArgs[i]);
                mysqlParams[i] = param;
                queryArgs[i] = "@" + i;
            }

            string sql = formattedsql.ToString();

            if (connection == null)
                await Connect();

            using (var cmd = new MySqlCommand(sql, connection))
            {
                cmd.Parameters.AddRange(mysqlParams);

                return await cmd.ExecuteScalarAsync();
            }
        }

        public async Task<DataSet> QueryAsync(FormattableString formattedsql)
        {

            var queryArgs = formattedsql.GetArguments();
            var mysqlParams = new MySqlParameter[queryArgs.Length];

            for (int i = 0; i < queryArgs.Length; i++)
            {
                var param = new MySqlParameter(i.ToString(), queryArgs[i]);
                mysqlParams[i] = param;
                queryArgs[i] = "@" + i;
            }

            string sql = formattedsql.ToString();

            if (connection == null)
                await Connect();

            using (var cmd = new MySqlCommand(sql, connection))
            {
                cmd.Parameters.AddRange(mysqlParams);


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
            var mysqlParams = new MySqlParameter[queryArgs.Length];

            for (int i = 0; i < queryArgs.Length; i++)
            {
                var param = new MySqlParameter(i.ToString(), queryArgs[i]);
                mysqlParams[i] = param;
                queryArgs[i] = "@" + i;
            }

            string sql = formattedsql.ToString();

            if (connection == null)
                Connect().Wait();

            using (var cmd = new MySqlCommand(sql, connection))
            {
                cmd.Parameters.AddRange(mysqlParams);

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