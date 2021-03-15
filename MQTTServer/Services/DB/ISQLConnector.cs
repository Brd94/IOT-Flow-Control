using System.Data;
using System.Threading.Tasks;

namespace MQTTServer
{
    public interface ISQLConnector
    {
        Task Connect();
        Task Disconnect();
        Task<int> NonQueryAsync(string sql);
        DataSet Query(string sql);
        Task<DataSet> QueryAsync(string sql);
        Task<object> ScalarQueryAsync(string sql);
    }

}