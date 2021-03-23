using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;

namespace WEBServer.Server.Services.Infrastructure
{

    public class SQLiteDatabaseAccessor : IDatabaseAccessor
    {
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

            using (var connection = new SqliteConnection("Data Source=/Users/brd/Desktop/Work/Uni/Tesi/IOT-Flow-Control/Shared/Data.db"))
            {
                connection.Open();
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
}
