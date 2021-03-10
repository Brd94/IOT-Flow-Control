using System;
using System.Data;
using Microsoft.Data.Sqlite;

namespace WEBServer.Server.Models.Infrastructure
{

    public class SQLiteDatabaseAccessor : IDatabaseAccessor
    {
        public DataSet Query(string SQL)
        {
            using (var conn = new SqliteConnection("Data Source=/Users/brd/Desktop/Work/Uni/Tesi/IOT-Flow-Control/Shared/Data.db"))
            {

                conn.Open();
                using (var cmd = new SqliteCommand(SQL, conn))
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
}
