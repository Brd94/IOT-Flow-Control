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

        public MySQLConnector(string server, string database, string uid, string password)
        {
            var builder = new MySqlConnectionStringBuilder();
            builder.Server = server;
            builder.Database = database;
            builder.UserID = uid;
            builder.Password = password;

            this.connectionString = builder.ConnectionString;

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

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddRange(mysqlParams);

                    return await cmd.ExecuteNonQueryAsync();
                }

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

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddRange(mysqlParams);

                    return await cmd.ExecuteScalarAsync();
                }
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

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

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

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

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
}