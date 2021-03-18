using System;
using System.Data;
using System.Threading.Tasks;

namespace MQTTServer
{
    public interface ISQLConnector
    {
        Task<int> NonQueryAsync(FormattableString sql);
        DataSet Query(FormattableString sql);
        Task<DataSet> QueryAsync(FormattableString sql);
        Task<object> ScalarQueryAsync(FormattableString sql);
    }

}