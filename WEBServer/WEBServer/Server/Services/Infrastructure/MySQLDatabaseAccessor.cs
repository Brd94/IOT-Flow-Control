using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace WEBServer.Server.Services.Infrastructure
{
    public class MySQLDatabaseAccessor : IDatabaseAccessor
    {

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

            using (var connection = new MySqlConnection("server=192.168.178.20;database=dbIOTFC;user id=admin;password=errata")) //Da spostare in appsettings.json
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
